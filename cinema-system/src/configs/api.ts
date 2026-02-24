// api.ts
import axios, { AxiosError, type AxiosRequestConfig, type AxiosResponse } from "axios";

const API_BASE_URL = import.meta.env.API_BASE_URL || "https://localhost:7227/api";

export const api = axios.create({
    baseURL: API_BASE_URL,
    timeout: 30000, // Reduced to 30s for better UX; adjust as needed
    headers: {
        "Content-Type": "application/json"
    }
});

// Interface for API error response (based on your template)
interface ApiErrorResponse {
    success: boolean;
    statusCode: number;
    message: string;
}

// Custom ApiError class (for thrown errors)
// This wraps API errors for easier handling in calling code
export class ApiError extends Error {
    public statusCode: number;
    public originalError?: AxiosError;

    constructor(message: string, statusCode: number, originalError?: AxiosError) {
        super(message);
        this.name = "ApiError";
        this.statusCode = statusCode;
        this.originalError = originalError;
    }
}

// Flag to prevent infinite refresh loops
let isRefreshing = false;
let failedQueue: Array<{
    resolve: (value: string | null) => void;
    reject: (error: any) => void;
}> = [];

// Process queued requests after token refresh
const processQueue = (error: any, token: string | null = null) => {
    failedQueue.forEach(({ resolve, reject }) => {
        if (error) {
            reject(error);
        } else {
            resolve(token);
        }
    });
    failedQueue = [];
};

// Handle logout functionality
const handleLogout = (message: string) => {
    localStorage.clear();
    window.location.href = "/login";
};

// Request interceptor - Add auth token and proactive token check
import type { InternalAxiosRequestConfig } from "axios";

api.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const accessToken = localStorage.getItem("accessToken");
        const refreshToken = localStorage.getItem("refreshToken");

        // Proactive check: If both tokens missing, logout immediately (covers client-deleted tokens)
        if (!accessToken && !refreshToken && config.url !== "/auth/login" && config.url !== "/auth/refresh") {
            handleLogout("Tokens missing. Please login again.");
            return Promise.reject(new ApiError("Tokens missing", 401));
        }

        if (accessToken && config.headers) {
            config.headers.Authorization = `Bearer ${accessToken}`;
        }
        return config;
    },
    (error: AxiosError) => {
        return Promise.reject(error);
    }
);

// Response interceptor - Handle errors based on status codes
api.interceptors.response.use(
    (response: AxiosResponse<any>) => {
        // Optionally transform: Return data directly if success (assumes your template)
        if (response.data?.success === true) {
            return response.data;
        }
        return response;
    },
    async (error: AxiosError<ApiErrorResponse>) => {
        const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };
        const statusCode = error.response?.status;
        const errorData = (error.response?.data ?? {}) as Partial<ApiErrorResponse>;
        const errorMessage = errorData.message || "An unknown error occurred.";

        // Handle 401 - Unauthorized (try to refresh token)
        if (statusCode === 401 && !originalRequest._retry) {
            const accessToken = localStorage.getItem("accessToken");
            const refreshToken = localStorage.getItem("refreshToken");

            // If no tokens, logout immediately
            if (!accessToken || !refreshToken) {
                handleLogout("You have been logged out.");
                return Promise.reject(new ApiError("Tokens missing", 401, error));
            }

            // If already refreshing, queue this request
            if (isRefreshing) {
                return new Promise((resolve, reject) => {
                    failedQueue.push({ resolve, reject });
                })
                    .then((token) => {
                        if (originalRequest.headers && token) {
                            originalRequest.headers.Authorization = `Bearer ${token}`;
                        }
                        return api(originalRequest);
                    })
                    .catch((err) => Promise.reject(err));
            }

            originalRequest._retry = true;
            isRefreshing = true;

            try {
                const response = await axios.post<{ newAccessToken: string }>(
                    `${API_BASE_URL}/auth/refresh`,
                    { accessToken, refreshToken }
                );

                const { newAccessToken } = response.data; // Added support for newRefreshToken if returned
                if (!newAccessToken) {
                    throw new Error("Refresh successful but no new access token was provided.");
                }

                localStorage.setItem("accessToken", newAccessToken);

                // Update authorization header for the original request
                if (originalRequest.headers) {
                    originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
                }

                processQueue(null, newAccessToken);
                isRefreshing = false;

                return api(originalRequest);
            } catch (refreshError: unknown) {
                processQueue(refreshError, null);
                isRefreshing = false;

                // Refresh failed, logout user
                handleLogout("Session expired. Please login again.");
                return Promise.reject(new ApiError("Refresh failed", 401, refreshError as AxiosError));
            }
        }

        // Additional handlers for other errors:

        // Handle 400 - Bad Request (e.g., validation errors like "Invalid password")
        // No logout; throw custom error for component to handle (e.g., show form error)
        if (statusCode === 400) {
            return Promise.reject(new ApiError(errorMessage, 400, error));
        }

        // Handle 403 - Forbidden (authenticated but no permission)
        // Alert without logout; throw custom error
        if (statusCode === 403) {
            alert("Access denied: " + errorMessage);
            return Promise.reject(new ApiError(errorMessage, 403, error));
        }

        // Handle 404 - Not Found
        // No alert/logout; throw custom error (e.g., for "resource not found")
        if (statusCode === 404) {
            return Promise.reject(new ApiError(errorMessage, 404, error));
        }

        // Handle 409 - Conflict (e.g., duplicate resource)
        // No logout; throw custom error for optimistic concurrency handling
        if (statusCode === 409) {
            alert("Conflict: " + errorMessage);
            return Promise.reject(new ApiError(errorMessage, 409, error));
        }

        // Handle 5xx - Server errors (refined: only logout on 500/501, alert on others)
        if (statusCode && statusCode >= 500) {
            if (statusCode === 500 || statusCode === 501) {
                handleLogout("Critical server error: " + errorMessage);
            } else {
                alert("Temporary server issue: " + errorMessage + ". Please try again.");
            }
            return Promise.reject(new ApiError(errorMessage, statusCode, error));
        }

        // For all other errors - just pass through with custom wrapper
        return Promise.reject(new ApiError(errorMessage, statusCode || 0, error));
    }
);