import axiosClient from '../../../api/axiosClient';
import axios from 'axios';
import type { ApiResponse } from '../../../types/api';
import { getAccessToken } from '../../../utils/tokenStorage';
import { getApiGatewayBaseUrl } from '../../../utils/apiConfig';

export interface SeatStatusDto {
  seatId: string;
  row: string;
  number: number;
  seatType: string;
  price: number;
  status: 0 | 1 | 2 | 3; // 0=Available, 1=Locked, 2=Booked, 3=Unavailable
  lockedBy?: string;
  lockedUntil?: string;
}

export interface SeatAvailabilityResponse {
  showtimeId: string;
  cinemaHallId: string;
  cinemaHallName: string;
  seats: SeatStatusDto[];
  summary: {
    totalSeats: number;
    availableSeats: number;
    lockedSeats: number;
    bookedSeats: number;
  };
}

export interface LockSeatsRequest {
  showtimeId: string;
  seatIds: string[];
  // userId removed - extracted from JWT token by backend
}

export interface CreateBookingRequest {
  // userId removed - extracted from JWT token by backend
  showtimeId: string;
  seatIds: string[];
  contactEmail: string;
  contactPhone: string;
  contactName: string;
}

export interface CancelBookingRequest {
  userId: string;
  cancellationReason?: string;
}

export interface BookingResponse {
  bookingId: string;
  userId: string;
  showtimeId: string;
  status: string;
  totalPrice: number;
  bookingDate: string;
  expiresAt?: string;
  seats: BookingSeatDto[];
  showtimeDetails?: ShowtimeDetailsDto;
  // Payment integration
  paymentId?: string;
  checkoutUrl?: string;
}

export interface PaymentLookupResponse {
  id: string;
  paymentId?: string;
  bookingId: string;
  orderInvoiceNumber: string;
  amount: number;
  status: number | string;
  expiresAt?: string;
}

export interface BookingSeatDto {
  seatId: string;
  row: string;
  number: number;
  seatType: string;
  price: number;
}

export interface ShowtimeDetailsDto {
  movieTitle?: string;
  cinemaName?: string;
  cinemaHallName?: string;
  startTime?: string;
}

export const bookingApi = {
  getSeatAvailability: async (showtimeId: string): Promise<ApiResponse<SeatAvailabilityResponse>> => {
    return axiosClient.get(`/api/showtimes/${showtimeId}/seats`);
  },

  lockSeats: async (showtimeId: string, data: LockSeatsRequest): Promise<ApiResponse<any>> => {
    return axiosClient.post(`/api/showtimes/${showtimeId}/seats/lock`, data);
  },

  unlockSeats: async (showtimeId: string, data: { showtimeId: string; seatIds: string[] }): Promise<ApiResponse<any>> => {
    return axiosClient.post(`/api/showtimes/${showtimeId}/seats/unlock`, data);
  },

  createBooking: async (data: CreateBookingRequest): Promise<ApiResponse<BookingResponse>> => {
    return axiosClient.post('/api/bookings', data);
  },

  getBookingById: async (bookingId: string): Promise<ApiResponse<BookingResponse>> => {
    return axiosClient.get(`/api/bookings/${bookingId}`);
  },

  getPaymentByBookingId: async (bookingId: string): Promise<PaymentLookupResponse | null> => {
    try {
      const token = getAccessToken();
      const response = await axios.get<ApiResponse<PaymentLookupResponse>>(
        `${getApiGatewayBaseUrl()}/api/payments/booking/${bookingId}`,
        {
          headers: token ? { Authorization: `Bearer ${token}` } : undefined,
          timeout: 10000,
        }
      );

      return response.data?.success ? response.data.data : null;
    } catch (error: any) {
      if (error?.response?.status === 404) {
        return null;
      }

      throw error;
    }
  },

  cancelBooking: async (bookingId: string, data: CancelBookingRequest): Promise<ApiResponse<unknown>> => {
    return axiosClient.put(`/api/bookings/${bookingId}/cancel`, data);
  }
};
