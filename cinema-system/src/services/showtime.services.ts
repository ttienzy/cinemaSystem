import { api } from "../configs/api";
import type { InputShowtimeDto, PaymentInfoRequest } from "../types/showtime.type";


export const showtimeServices = {
    getSeatingPlan: async (showtimeId: string) => {
        const response = await api.get('/showtimes/' + showtimeId + '/seating-plan/');
        return response.data;
    },
    getPaymentUrl: async (paymentInfo: PaymentInfoRequest) => {
        const response = await api.post('/bookings/create-booking', paymentInfo);
        return response.data;
    },
    getShowtimeSetUpData: async (cinemaId: string) => {
        const response = await api.get('/showtimes/setup-data/' + cinemaId);
        return response.data;
    },
    getShowtimePerformance: async (cinemaId: string) => {
        const response = await api.get('/showtimes/performance/' + cinemaId);
        return response.data;
    },
    getShowtimeById: async (showtimeId: string) => {
        const response = await api.get('/showtimes/' + showtimeId);
        return response.data;
    },
    postShowtimeForm: async (showtimeData: InputShowtimeDto) => {
        const response = await api.post('/showtimes', showtimeData);
        return response.data;
    },
    putShowtimeForm: async (showtimeId: string, showtimeData: InputShowtimeDto) => {
        const response = await api.put('/showtimes/' + showtimeId, showtimeData);
        return response.data;
    },
    putConfirmShowtime: async (showtimeId: string) => {
        const response = await api.put('/showtimes/' + showtimeId + '/status/confirm');
        return response.data;
    },
    putCancelShowtime: async (showtimeId: string) => {
        const response = await api.put('/showtimes/' + showtimeId + '/status/cancel');
        return response.data;
    },
}