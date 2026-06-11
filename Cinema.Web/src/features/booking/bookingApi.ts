import axiosClient from '../../shared/api/axiosClient';
import type { ApiResponse } from '../../shared/types/api';

export type SeatStatus = 0 | 1 | 2 | 3;

export interface SeatStatusDto {
  seatId: string;
  row: string;
  number: number;
  price: number;
  status: SeatStatus;
  lockedBy?: string | null;
  lockedUntil?: string | null;
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
}

export interface SeatLockResult {
  success: boolean;
  showtimeId: string;
  seatIds: string[];
  lockedUntil?: string | null;
  message?: string | null;
}

export interface CreateBookingRequest {
  showtimeId: string;
  seatIds: string[];
  contactEmail: string;
  contactPhone: string;
  contactName: string;
}

export interface BookingSeatDto {
  seatId: string;
  row: string;
  number: number;
  seatType: string;
  price: number;
}

export interface ShowtimeDetailsDto {
  movieTitle?: string | null;
  cinemaName?: string | null;
  cinemaHallName?: string | null;
  startTime?: string | null;
}

export interface BookingResponse {
  bookingId: string;
  userId: string;
  showtimeId: string;
  status: number | string;
  totalPrice: number;
  bookingDate: string;
  expiresAt?: string | null;
  seats: BookingSeatDto[];
  showtimeDetails?: ShowtimeDetailsDto | null;
  paymentId?: string | null;
  checkoutUrl?: string | null;
}

export interface PaymentLookupResponse {
  id: string;
  bookingId: string;
  orderInvoiceNumber: string;
  amount: number;
  currency: string;
  status: number | string;
  expiresAt?: string | null;
  customerEmail: string;
  customerPhone: string;
  customerName: string;
}

export const bookingApi = {
  getSeatAvailability(showtimeId: string) {
    return axiosClient.get<never, ApiResponse<SeatAvailabilityResponse>>(`/api/v1/showtimes/${showtimeId}/seats`);
  },
  lockSeats(showtimeId: string, data: LockSeatsRequest) {
    return axiosClient.post<never, ApiResponse<SeatLockResult>>(`/api/v1/showtimes/${showtimeId}/seats/lock`, data);
  },
  unlockSeats(showtimeId: string, data: LockSeatsRequest) {
    return axiosClient.post<never, ApiResponse<boolean>>(`/api/v1/showtimes/${showtimeId}/seats/unlock`, data);
  },
  createBooking(data: CreateBookingRequest) {
    return axiosClient.post<never, ApiResponse<BookingResponse>>('/api/v1/bookings', data);
  },
  getBookingById(bookingId: string) {
    return axiosClient.get<never, ApiResponse<BookingResponse>>(`/api/v1/bookings/${bookingId}`);
  },
  async getPaymentByBookingId(bookingId: string): Promise<PaymentLookupResponse | null> {
    try {
      const response = await axiosClient.get<never, ApiResponse<PaymentLookupResponse>>(
        `/api/v1/payments/booking/${bookingId}`,
      );
      return response.data;
    } catch (error: unknown) {
      const status = (error as { response?: { status?: number } }).response?.status;
      if (status === 404) return null;
      throw error;
    }
  },
  cancelBooking(bookingId: string, cancellationReason?: string) {
    return axiosClient.put<never, ApiResponse<boolean>>(`/api/v1/bookings/${bookingId}/cancel`, {
      cancellationReason,
    });
  },
};
