import axios from 'axios';
import type { AxiosError, InternalAxiosRequestConfig, AxiosResponse } from 'axios';
import { message } from 'antd';
import { Endpoints } from './endpoints';
import { getAccessToken, getRefreshToken, setTokens, clearTokens } from '../utils/token';

// Create axios instance
const api = axios.create({
    baseURL: import.meta.env.VITE_API_URL || 'https://localhost:7227',
    timeout: 30000,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Flag to prevent multiple refresh attempts simultaneously
let isRefreshing = false;
let failedQueue: Array<{
    resolve: (value: unknown) => void;
    reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: AxiosError | null, token: string | null = null) => {
    failedQueue.forEach((prom) => {
        if (error) {
            prom.reject(error);
        } else {
            prom.resolve(token);
        }
    });
    failedQueue = [];
};

// Request interceptor - Add auth token
api.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        // Skip auth for public endpoints
        const publicEndpoints = [
            Endpoints.AUTH.LOGIN,
            Endpoints.AUTH.REGISTER,
            Endpoints.AUTH.REFRESH,
            '/api/genres',
            '/api/movies',
            '/api/cinemas',
            '/api/showtimes',
        ];

        const isPublic = publicEndpoints.some(endpoint =>
            config.url?.includes(endpoint.replace(api.defaults.baseURL || '', ''))
        );

        if (!isPublic) {
            const token = getAccessToken();
            if (token) {
                config.headers.Authorization = `Bearer ${token}`;
            }
        }

        return config;
    },
    (error: AxiosError) => {
        return Promise.reject(error);
    }
);

// Response interceptor - Handle 401 and token refresh
api.interceptors.response.use(
    (response: AxiosResponse) => {
        return response;
    },
    async (error: AxiosError) => {
        const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

        // Handle 401 Unauthorized
        if (error.response?.status === 401 && !originalRequest._retry) {
            if (isRefreshing) {
                return new Promise((resolve, reject) => {
                    failedQueue.push({ resolve, reject });
                })
                    .then((token) => {
                        originalRequest.headers.Authorization = `Bearer ${token}`;
                        return api(originalRequest);
                    })
                    .catch((err) => {
                        return Promise.reject(err);
                    });
            }

            originalRequest._retry = true;
            isRefreshing = true;

            const refreshToken = getRefreshToken();
            const accessToken = getAccessToken();

            if (!refreshToken || !accessToken) {
                clearTokens();
                window.location.href = '/login';
                return Promise.reject(error);
            }

            try {
                const response = await axios.post(
                    Endpoints.AUTH.REFRESH,
                    {
                        accessToken: accessToken,
                        refreshToken: refreshToken,
                    }
                );

                // Backend now returns flat structure: { accessToken, refreshToken }
                const { accessToken: newAccessToken, refreshToken: newRefreshToken } = response.data;

                // Save new tokens
                setTokens(newAccessToken, newRefreshToken);

                processQueue(null, newAccessToken);

                originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
                return api(originalRequest);
            } catch (refreshError) {
                processQueue(refreshError as AxiosError, null);
                clearTokens();
                window.location.href = '/login';
                return Promise.reject(refreshError);
            } finally {
                isRefreshing = false;
            }
        }

        // Handle other errors
        const errorMessage = (error.response?.data as { message?: string })?.message
            || error.message
            || 'An error occurred';

        // Don't show error toast for certain endpoints
        const silentEndpoints = ['/callback'];
        const isSilent = silentEndpoints.some(endpoint =>
            originalRequest.url?.includes(endpoint)
        );

        if (!isSilent) {
            // Handle different error types with appropriate messages
            switch (error.response?.status) {
                case 400: {
                    // Show validation errors if available
                    const validationErrors = error.response?.data as { errors?: Record<string, string[]> };
                    if (validationErrors?.errors) {
                        // Show first validation error
                        const firstError = Object.values(validationErrors.errors).flat()[0];
                        message.error(firstError || 'Dữ liệu không hợp lệ');
                    } else {
                        message.error(errorMessage);
                    }
                    break;
                }
                case 403:
                    message.error('Bạn không có quyền thực hiện thao tác này');
                    break;
                case 404:
                    message.error('Không tìm thấy tài nguyên');
                    break;
                case 500:
                    message.error('Lỗi máy chủ, vui lòng thử lại sau');
                    break;
                default:
                    message.error(errorMessage);
            }
        }

        return Promise.reject(error);
    }
);

export default api;
