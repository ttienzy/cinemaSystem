import { api } from "../configs/api";


export const profileServices = {
    getCurrentUser: async (id: string) => {
        const response = await api.get('/identity/profile/' + id);
        return response.data;
    },
    purchaseHistory: async (userId: string) => {
        const response = await api.get('/bookings/purchase-history/' + userId);
        return response.data;
    }
}