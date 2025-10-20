import { api } from "../configs/api";
import type { CartItem } from "../types/dashboard.types";
import type { ConcessionSaleQueryParameters } from "../types/inventory.types";



export const inventoryServices = {
    getInventory: async (cinemaId: string) => {
        const response = await api.get(`/inventoryItems?cinameId=${cinemaId}`);
        return response.data;
    },
    confirmConcessionPurchase: async ({ cinemaId, cartItem }: { cinemaId: string; cartItem: CartItem }) => {
        const response = await api.post(`/inventoryItems/confirm-payment/${cinemaId}`, cartItem);
        return response.data;
    },
    getConcessionSaleHistory: async (cinemaId: string, queryParams: ConcessionSaleQueryParameters) => {
        const response = await api.get(`/inventoryItems/sale-history/${cinemaId}`, { params: queryParams });
        return response.data;
    },
    getReportRevenueAndStock: async (cinemaId: string) => {
        const response = await api.get(`/inventoryItems/revenue-report/${cinemaId}`);
        return response.data;
    },

};