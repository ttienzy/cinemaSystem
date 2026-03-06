// Payment API - VNPay integration
import axios from '../../../shared/api/axios.instance';
import { Endpoints } from '../../../shared/api/endpoints';
import type { CreateBookingRequest, CreateBookingResponse, PaymentCallbackResponse } from '../../booking/types/booking.types';

export const paymentApi = {
    // Create booking and get payment URL
    createBookingWithPayment: async (data: CreateBookingRequest): Promise<CreateBookingResponse> => {
        const response = await axios.post<CreateBookingResponse>(Endpoints.BOOKINGS.BASE, data);
        return response.data;
    },

    // Handle payment callback from VNPay
    handlePaymentCallback: async (params: Record<string, string>): Promise<PaymentCallbackResponse> => {
        const response = await axios.get<PaymentCallbackResponse>(Endpoints.BOOKINGS.CALLBACK, { params });
        return response.data;
    },

    // Parse VNPay return URL parameters
    parseVnpayReturn: (url: string): Record<string, string> => {
        const params: Record<string, string> = {};
        const urlObj = new URL(url);
        urlObj.searchParams.forEach((value, key) => {
            params[key] = value;
        });
        return params;
    },
};

export default paymentApi;
