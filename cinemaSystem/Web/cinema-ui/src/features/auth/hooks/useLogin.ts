// useLogin hook - Login user with role-based redirect
import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { message } from 'antd';
import { authApi } from '../api/authApi';
import { useAuthStore } from '../store/authStore';
import { getUserFromToken, setTokens } from '../../../shared/utils/token';
import { getRedirectPath } from '../../../shared/utils/authRedirect';
import type { LoginRequest } from '../types/auth.types';

export function useLogin() {
    const navigate = useNavigate();
    const setAuth = useAuthStore((state) => state.setAuth);

    return useMutation({
        mutationFn: (data: LoginRequest) => authApi.login(data),
        onSuccess: (response) => {
            // Save tokens
            setTokens(response.accessToken, response.refreshToken);

            // Get user info from token
            const userInfo = getUserFromToken(response.accessToken);

            if (userInfo) {
                setAuth(
                    {
                        id: userInfo.userId || '',
                        email: userInfo.email || '',
                        username: userInfo.username || '',
                        roles: userInfo.roles || [],
                        emailConfirmed: false,
                        createdAt: new Date().toISOString(),
                    },
                    response.accessToken,
                    response.refreshToken
                );

                // Role-based redirect with replace to prevent back navigation to login
                const redirectPath = getRedirectPath(userInfo.roles || []);

                message.success('Đăng nhập thành công!');
                navigate(redirectPath, { replace: true });
            } else {
                // Fallback if can't decode token
                message.success('Đăng nhập thành công!');
                navigate('/', { replace: true });
            }
        },
        onError: (error: Error) => {
            message.error(error.message || 'Đăng nhập thất bại');
        },
    });
}

export default useLogin;
