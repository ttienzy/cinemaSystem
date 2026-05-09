// ============================================================
// Axios Client - Interceptor + Queue + Standard Response
// ============================================================
// Kiến trúc "Silent Refresh with Request Queueing":
//
// Response Interceptor Logic:
// 1. Nhận response từ server (luôn có dạng ApiResponse<T>)
// 2. Kiểm tra `success` field:
//    - success === true  → trả về NGUYÊN KHỐI ApiResponse<T>
//    - success === false → hiển thị message, reject với ApiResponse
// 3. Nếu HTTP 401 → kích hoạt Silent Refresh + Queue
//
// Quy tắc:
// - KHÔNG unwrap data.data — giữ nguyên lớp vỏ ApiResponse
// - Mọi service/feature API đều nhận được ApiResponse<T>
// - UI component tự truy cập .data khi cần
// ============================================================

import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { message } from 'antd';
import { getAccessToken, getRefreshToken, setTokens, clearAuth } from '../utils/tokenStorage';
import { getApiGatewayBaseUrl } from '../utils/apiConfig';
import type { ApiResponse } from '../types/api';

// ---- Khởi tạo Axios Instance ----

const axiosClient = axios.create({
  baseURL: getApiGatewayBaseUrl(),
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15000,
});

// ---- Queue Manager ----

interface QueueItem {
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}

let isRefreshing = false;
let failedQueue: QueueItem[] = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) {
      reject(error);
    } else {
      resolve(token!);
    }
  });
  failedQueue = [];
};

// ---- Request Interceptor: Gắn token vào mỗi request ----

axiosClient.interceptors.request.use(
  (config) => {
    const token = getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ---- Response Interceptor ----

axiosClient.interceptors.response.use(
  (response) => {
    // ============================================================
    // SUCCESS PATH: HTTP 2xx
    // ============================================================
    const body = response.data as ApiResponse;

    // Nếu backend trả về Standard Response Wrapper
    if (body && typeof body === 'object' && 'success' in body) {
      if (body.success === false) {
        // Business logic error (ví dụ: email đã tồn tại, validation fail)
        // Hiển thị message từ server cho user
        const errorMsg = body.message || 'Có lỗi xảy ra';
        message.error(errorMsg);

        // Log traceId ra console để debug
        if (body.traceId) {
          console.warn(`[API Error] traceId: ${body.traceId}`, body.errors);
        }

        return Promise.reject(body);
      }

      // success === true → trả về nguyên khối ApiResponse<T>
      return body;
    }

    // Fallback: nếu response không theo chuẩn wrapper (hiếm gặp)
    return response.data;
  },
  async (error: AxiosError<ApiResponse>) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // ============================================================
    // 401 HANDLER: Silent Refresh + Queue
    // ============================================================
    if (error.response?.status === 401 && !originalRequest._retry) {

      // Refresh-token request bị 401 → force logout
      if (originalRequest.url?.includes('/refresh-token')) {
        processQueue(error, null);
        performForceLogout();
        return Promise.reject(error);
      }

      // Đã có request khác đang refresh → xếp hàng chờ
      if (isRefreshing) {
        return new Promise<string>((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((newToken) => {
          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          return axiosClient(originalRequest);
        });
      }

      // Bắt đầu refresh
      originalRequest._retry = true;
      isRefreshing = true;

      const refreshToken = getRefreshToken();

      if (!refreshToken) {
        isRefreshing = false;
        performForceLogout();
        return Promise.reject(error);
      }

      try {
        const response = await axios.post(
          `${getApiGatewayBaseUrl()}/api/auth/refresh-token`,
          { refreshToken },
          {
            headers: { 'Content-Type': 'application/json' },
            timeout: 10000,
          }
        );

        // Refresh response cũng theo Standard Response Wrapper
        const refreshBody = response.data as ApiResponse;
        const tokenData = refreshBody?.data || response.data;
        const { accessToken: newAccessToken, refreshToken: newRefreshToken } = tokenData;

        setTokens(newAccessToken, newRefreshToken);
        processQueue(null, newAccessToken);

        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
        return axiosClient(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        performForceLogout();
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    // ============================================================
    // NON-401 ERROR HANDLER
    // ============================================================
    return handleNon401Error(error);
  }
);

// ---- Helpers ----

function handleNon401Error(error: AxiosError<ApiResponse>): Promise<never> {
  const status = error.response?.status;
  const body = error.response?.data;

  // Ưu tiên hiển thị message từ Standard Response Wrapper
  if (body && typeof body === 'object' && 'message' in body && body.message) {
    message.error(body.message);

    if (body.traceId) {
      console.warn(`[API Error] traceId: ${body.traceId}`, body.errors);
    }
  } else {
    // Fallback cho các lỗi không theo chuẩn wrapper
    if (status === 403) {
      message.error('Bạn không có quyền truy cập');
    } else if (status === 500) {
      message.error('Hệ thống đang gặp sự cố, vui lòng thử lại sau');
    }
  }

  return Promise.reject(error);
}

function performForceLogout(): void {
  clearAuth();
  window.dispatchEvent(new CustomEvent('auth:forceLogout'));
}

export default axiosClient;
