import { api } from "../configs/api";


export const cinemaServices = {
    getCinemas: async () => {
        const response = await api.get('/cinemas');
        return response.data;
    },
    getCinemaById: async (id: string) => {
        const response = await api.get('/cinemas/' + id);
        return response.data;
    }
}