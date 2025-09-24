import { api } from "../configs/api";


export const bookingServices = {
    async checkInBooking(bookingId: string) {
        const response = await api.get(`/bookings/check-in/${bookingId}`);
        return response.data;
    },
    async confirmBooking(bookingId: string) {
        const response = await api.post(`/bookings/confirm-check-in/${bookingId}`);
        return response.data;
    },
}