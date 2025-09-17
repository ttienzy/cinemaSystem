import { api } from "../configs/api";



export const inventoryServices = {
    getInventory: async (cinemaId: string) => {
        const response = await api.get(`/inventoryItems?cinameId=${cinemaId}`);
        return response.data;
    },
};