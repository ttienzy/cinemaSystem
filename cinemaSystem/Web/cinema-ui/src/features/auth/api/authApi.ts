import api from '../../../shared/api/axios.instance';
import { Endpoints } from '../../../shared/api/endpoints';
import type { LoginRequest, RegisterRequest, LoginResponse } from '../types/auth.types';

export const authApi = {
    login: async (data: LoginRequest): Promise<LoginResponse> => {
        const response = await api.post<LoginResponse>(Endpoints.AUTH.LOGIN, data);

        // Backend now returns flat structure: { accessToken, refreshToken }
        return {
            accessToken: response.data.accessToken,
            refreshToken: response.data.refreshToken,
            expiresIn: 3600, // Default 1 hour
            tokenType: 'Bearer'
        };
    },

    register: async (data: RegisterRequest): Promise<void> => {
        await api.post(Endpoints.AUTH.REGISTER, data);
    },

    logout: async (): Promise<void> => {
        await api.post(Endpoints.AUTH.LOGOUT);
    },

    refreshToken: async (accessToken: string, refreshToken: string): Promise<LoginResponse> => {
        const response = await api.post<LoginResponse>(Endpoints.AUTH.REFRESH, {
            accessToken,
            refreshToken,
        });

        // Backend now returns flat structure: { accessToken, refreshToken }
        return {
            accessToken: response.data.accessToken,
            refreshToken: response.data.refreshToken,
            expiresIn: 3600,
            tokenType: 'Bearer'
        };
    },
};

export default authApi;
