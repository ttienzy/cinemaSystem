import axiosClient from '../../shared/api/axiosClient';
import {
  clearAuth,
  getRefreshToken,
  setCachedUser,
  setTokens,
} from '../../shared/auth/tokenStorage';
import type { ApiResponse } from '../../shared/types/api';
import type { LoginRequest, LoginResponse, User, UserInfoResponse } from '../../shared/types/auth';

function toUser(response: LoginResponse | UserInfoResponse): User {
  const userInfo = 'user' in response ? response.user : response;

  return {
    id: userInfo.id,
    email: userInfo.email,
    fullName: userInfo.fullName,
    roles: [...userInfo.roles],
  };
}

export const authService = {
  async login(request: LoginRequest): Promise<User> {
    const data = await axiosClient.post<never, LoginResponse>(
      '/api/v1/identity/login',
      request,
    );
    const user = toUser(data);

    setTokens(data.accessToken, data.refreshToken);
    setCachedUser(user);

    return user;
  },

  async getCurrentUser(): Promise<User> {
    const data = await axiosClient.get<never, UserInfoResponse>('/api/v1/identity/me');
    const user = toUser(data);
    setCachedUser(user);
    return user;
  },

  async logout(): Promise<void> {
    const refreshToken = getRefreshToken();

    if (refreshToken) {
      try {
        await axiosClient.post('/api/v1/identity/logout', { refreshToken });
      } catch {
        // Local logout should still complete if revoke fails.
      }
    }

    clearAuth();
  },
};
