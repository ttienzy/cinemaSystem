// ============================================================
// useAuth Hook - Standard Response Aware
// ============================================================
// Truy cập .data từ ApiResponse<T> khi cần dữ liệu thực.
// Tận dụng .message từ server để hiển thị thông báo.
// ============================================================

import { useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { message } from 'antd';
import { useAuthStore } from '../store/useAuthStore';
import { authService } from '../services/auth/authService';
import { setCachedUser } from '../utils/tokenStorage';
import type { LoginRequest } from '../types/auth.types';

export const useAuth = () => {
    const navigate = useNavigate();
    const {
        user,
        isAuthenticated,
        isLoading,
        isBackgroundVerifying,
        setUser,
        logout: logoutStore,
    } = useAuthStore();

    const login = useCallback(async (credentials: LoginRequest) => {
        try {
            const response = await authService.login(credentials);

            // response = ApiResponse<LoginResponse>
            // Truy cập .data để lấy LoginResponse bên trong
            const loginData = response.data;

            // Lưu tokens vào kho
            authService.saveTokens(loginData.accessToken, loginData.refreshToken);

            // Tạo user object
            const user = {
                id: loginData.userId,
                email: loginData.email,
                fullName: loginData.fullName,
                roles: loginData.roles,
            };

            setCachedUser(user);
            setUser(user);

            // Dùng message từ server thay vì hardcode
            message.success(response.message || 'Đăng nhập thành công!');

            // Redirect dựa vào role
            if (loginData.roles.includes('Admin')) {
                navigate('/admin');
            } else {
                navigate('/');
            }
        } catch (error: any) {
            // Nếu error là ApiResponse (success=false), message đã được hiện bởi interceptor
            // Nếu là AxiosError, hiện message từ response hoặc fallback
            if (!error?.success && error?.message) {
                // Đã được xử lý bởi interceptor, không cần hiện thêm
            } else {
                message.error(error.response?.data?.message || 'Đăng nhập thất bại');
            }
            throw error;
        }
    }, [navigate, setUser]);

    const logout = useCallback(async () => {
        try {
            await authService.logout();
        } catch (error) {
            console.warn('[useAuth] Logout API error (ignored):', error);
        } finally {
            logoutStore();
            navigate('/login');
            message.info('Đã đăng xuất');
        }
    }, [navigate, logoutStore]);

    const hasRole = useCallback((role: string): boolean => {
        return user?.roles.includes(role) ?? false;
    }, [user]);

    const hasAnyRole = useCallback((roles: string[]): boolean => {
        return roles.some(role => user?.roles.includes(role)) ?? false;
    }, [user]);

    return {
        user,
        isAuthenticated,
        isLoading,
        isBackgroundVerifying,
        login,
        logout,
        hasRole,
        hasAnyRole,
    };
};
