import { api } from "../configs/api";


export const authServices = {
    login: async (credentials: { email: string; password: string }) => {
        const response = await api.post('/auth/login', credentials);
        console.log(response.data);
        return response.data;
    },
    register: async (userData: { userName: string; email: string; password: string; phoneNumber: string }) => {
        const response = await api.post('/auth/register', userData);
        return response.data;
    },
    refreshAccessToken: async (tokenModels: { accessToken: string; refreshToken: string }) => {
        const response = await api.post('/auth/refresh', tokenModels);
        return response.data;
    },
}