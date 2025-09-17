import { api } from "../configs/api";
import type { PaymentInfoRequest } from "../types/showtime.type";


export const showtimeServices = {
    getSeatingPlan: async (showtimeId: string) => {
        const response = await api.get('/showtimes/' + showtimeId + '/seating-plan/');
        return response.data;
    },
    getPaymentUrl: async (paymentInfo: PaymentInfoRequest) => {
        const response = await api.post('/bookings/create-booking', paymentInfo);
        return response.data;
    }
}