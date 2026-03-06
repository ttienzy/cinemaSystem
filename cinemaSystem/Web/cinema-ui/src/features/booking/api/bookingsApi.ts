// Bookings API - Based on Backend API endpoints
import axios from '../../../shared/api/axios.instance';
import { Endpoints } from '../../../shared/api/endpoints';
import type {
    Booking,
    BookingDetail,
    CreateBookingRequest,
    CreateBookingResponse,
    MyBookingsQuery,
    PaymentCallbackResponse
} from '../types/booking.types';

export interface BookingsResponse {
    items: Booking[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
}

export const bookingsApi = {
    // Get all bookings (admin/manager)
    getBookings: async (pageIndex = 1, pageSize = 20): Promise<BookingsResponse> => {
        const response = await axios.get<BookingsResponse>(Endpoints.BOOKINGS.BASE, {
            params: { pageIndex, pageSize },
        });
        return response.data;
    },

    // Get my bookings (user)
    getMyBookings: async (params?: MyBookingsQuery): Promise<BookingsResponse> => {
        const response = await axios.get<BookingsResponse>(Endpoints.BOOKINGS.MY, { params });
        return response.data;
    },

    // Get booking by ID
    getBookingById: async (id: string): Promise<BookingDetail> => {
        const response = await axios.get<BookingDetail>(Endpoints.BOOKINGS.DETAIL(id));
        return response.data;
    },

    // Create a new booking
    createBooking: async (data: CreateBookingRequest): Promise<CreateBookingResponse> => {
        const response = await axios.post<CreateBookingResponse>(Endpoints.BOOKINGS.BASE, data);
        return response.data;
    },

    // Cancel a booking
    cancelBooking: async (id: string): Promise<void> => {
        const response = await axios.post(Endpoints.BOOKINGS.CANCEL(id));
        return response.data;
    },

    // Complete a booking
    completeBooking: async (id: string): Promise<void> => {
        const response = await axios.post(Endpoints.BOOKINGS.COMPLETE(id));
        return response.data;
    },

    // Request refund
    requestRefund: async (id: string): Promise<void> => {
        const response = await axios.post(Endpoints.BOOKINGS.REQUEST_REFUND(id));
        return response.data;
    },

    // Approve refund (admin/manager)
    approveRefund: async (id: string): Promise<void> => {
        const response = await axios.post(Endpoints.BOOKINGS.APPROVE_REFUND(id));
        return response.data;
    },

    // Check-in
    checkIn: async (id: string): Promise<void> => {
        const response = await axios.post(Endpoints.BOOKINGS.CHECK_IN(id));
        return response.data;
    },

    // Payment callback from VNPay
    handlePaymentCallback: async (params: Record<string, string>): Promise<PaymentCallbackResponse> => {
        const response = await axios.get<PaymentCallbackResponse>(Endpoints.BOOKINGS.CALLBACK, { params });
        return response.data;
    },
};

export default bookingsApi;
