// useRegister hook - Register user with auto-login
import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { message } from 'antd';
import { authApi } from '../api/authApi';
import { useAuthStore } from '../store/authStore';
import { getUserFromToken, setTokens } from '../../../shared/utils/token';
import { getRedirectPath } from '../../../shared/utils/authRedirect';
import type { RegisterRequest } from '../types/auth.types';

export function useRegister() {
    const navigate = useNavigate();
    const setAuth = useAuthStore((state) => state.setAuth);

    return useMutation({
        mutationFn: async (data: RegisterRequest) => {
            // First register the user
            await authApi.register(data);

            // Then auto-login with the same credentials
            const loginResponse = await authApi.login({
                email: data.email,
                password: data.password,
            });

            return loginResponse;
        },
        onSuccess: (loginResponse) => {
            // Save tokens
            setTokens(loginResponse.accessToken, loginResponse.refreshToken);

            // Get user info from token
            const userInfo = getUserFromToken(loginResponse.accessToken);

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
                    loginResponse.accessToken,
                    loginResponse.refreshToken
                );

                message.success('Đăng ký thành công!');

                // Role-based redirect
                const redirectPath = getRedirectPath(userInfo.roles || []);
                navigate(redirectPath, { replace: true });
            } else {
                message.success('Đăng ký thành công!');
                navigate('/', { replace: true });
            }
        },
        onError: (error: Error) => {
            message.error(error.message || 'Đăng ký thất bại');
        },
    });
}

export default useRegister;
