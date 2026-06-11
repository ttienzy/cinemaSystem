import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { App } from 'antd';
import { getApiGatewayBaseUrl } from '../utils/apiConfig';
import { clearAuth, getAccessToken, getRefreshToken, setTokens } from '../auth/tokenStorage';
import type { ApiResponse } from '../types/api';
import type { LoginResponse } from '../types/auth';

const axiosClient = axios.create({
  baseURL: getApiGatewayBaseUrl(),
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15000,
});

let messageApi: ReturnType<typeof App.useApp>['message'] | null = null;
let isRefreshing = false;
let queue: Array<{
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}> = [];

export function bindApiMessage(api: ReturnType<typeof App.useApp>['message']): void {
  messageApi = api;
}

function flushQueue(error: unknown, token?: string): void {
  queue.forEach(({ resolve, reject }) => {
    if (error || !token) reject(error);
    else resolve(token);
  });
  queue = [];
}

axiosClient.interceptors.request.use((config) => {
  const token = getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

axiosClient.interceptors.response.use(
  (response) => {
    const body = response.data as ApiResponse;

    if (body && typeof body === 'object' && 'success' in body) {
      if (!body.success) {
        if (body.message) messageApi?.error(body.message);
        return Promise.reject(body);
      }

      return body;
    }

    return response.data;
  },
  async (error: AxiosError<ApiResponse>) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (originalRequest.url?.includes('/api/v1/identity/refresh')) {
        performForceLogout();
        return Promise.reject(error);
      }

      if (isRefreshing) {
        return new Promise<string>((resolve, reject) => {
          queue.push({ resolve, reject });
        }).then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return axiosClient(originalRequest);
        });
      }

      const refreshToken = getRefreshToken();
      if (!refreshToken) {
        performForceLogout();
        return Promise.reject(error);
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const response = await axios.post<LoginResponse>(
          `${getApiGatewayBaseUrl()}/api/v1/identity/refresh`,
          { refreshToken },
        );
        const tokenData = response.data;
        setTokens(tokenData.accessToken, tokenData.refreshToken);
        flushQueue(null, tokenData.accessToken);
        originalRequest.headers.Authorization = `Bearer ${tokenData.accessToken}`;
        return axiosClient(originalRequest);
      } catch (refreshError) {
        flushQueue(refreshError);
        performForceLogout();
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    const body = error.response?.data;
    if (body?.message) messageApi?.error(body.message);
    else if (error.response?.status === 403) messageApi?.error('Access denied');
    else if (error.response?.status === 500) messageApi?.error('Server error');

    return Promise.reject(error);
  },
);

function performForceLogout(): void {
  clearAuth();
  window.dispatchEvent(new CustomEvent('auth:forceLogout'));
}

export default axiosClient;
