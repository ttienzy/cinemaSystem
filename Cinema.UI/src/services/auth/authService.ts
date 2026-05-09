// ============================================================
// Auth Service - API Layer (Standard Response Aware)
// ============================================================
// Mọi method trả về ApiResponse<T> đúng chuẩn.
// UI/Hook layer sẽ truy cập .data khi cần dữ liệu bên trong.
// ============================================================

import axiosClient from '../../api/axiosClient';
import {
  setTokens,
  clearAuth,
  getRefreshToken,
  hasTokens,
} from '../../utils/tokenStorage';
import type { ApiResponse } from '../../types/api';
import type {
  LoginRequest,
  LoginResponse,
  UserInfoResponse,
  RegisterRequest,
} from '../../types/auth.types';

class AuthService {
  // ---- Public API ----

  async register(data: RegisterRequest): Promise<ApiResponse<void>> {
    return await axiosClient.post('/api/auth/register', data);
  }

  async login(credentials: LoginRequest): Promise<ApiResponse<LoginResponse>> {
    return await axiosClient.post('/api/auth/login', credentials);
  }

  async refreshToken(refreshToken: string): Promise<ApiResponse<LoginResponse>> {
    return await axiosClient.post('/api/auth/refresh-token', { refreshToken });
  }

  async getCurrentUser(): Promise<ApiResponse<UserInfoResponse>> {
    return await axiosClient.get('/api/auth/me');
  }

  async logout(): Promise<void> {
    const refreshToken = getRefreshToken();
    if (refreshToken) {
      try {
        await axiosClient.post('/api/auth/revoke-token', { refreshToken });
      } catch (error) {
        console.warn('[AuthService] Failed to revoke token on server:', error);
      }
    }
    clearAuth();
  }

  // ---- Token helpers (delegate to tokenStorage) ----

  saveTokens(accessToken: string, refreshToken: string): void {
    setTokens(accessToken, refreshToken);
  }

  hasValidSession(): boolean {
    return hasTokens();
  }
}

export const authService = new AuthService();
