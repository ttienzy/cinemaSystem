// ============================================================
// Ticket Operations API — Quản lý vé tại quầy (Staff/Admin)
// ============================================================

import axiosClient from '../../../api/axiosClient';
import type { ApiResponse, PaginatedResponse } from '../../../types/api';

// ---- DTOs matching backend TicketOperationResponse ----

export interface BookingSeatDto {
  seatId: string;
  row: string;
  number: number;
  seatType: string;
  price: number;
}

export interface ShowtimeDetailsDto {
  showtimeId: string;
  movieTitle: string;
  startTime: string;
  endTime: string;
  cinemaName: string;
  cinemaHallName: string;
}

export interface TicketOperationResponse {
  bookingId: string;
  ticketCode: string;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  bookingStatus: number; // enum from backend
  paymentStatus: number | null;
  operationalStatus: string; // 'Paid' | 'CheckedIn' | 'Cancelled' | 'Expired' | 'Refunded' | ...
  canCheckIn: boolean;
  totalPrice: number;
  bookingDate: string;
  paidAt: string | null;
  checkedInAt: string | null;
  seats: BookingSeatDto[];
  showtimeDetails: ShowtimeDetailsDto | null;
}

// ---- API Calls ----

export const ticketOperationsApi = {
  /**
   * Tìm kiếm vé theo mã vé, email, hoặc SĐT.
   * Endpoint: GET /api/bookings/operations/tickets?q=&pageNumber=&pageSize=
   */
  searchTickets: async (
    query: string,
    pageNumber: number = 1,
    pageSize: number = 20
  ): Promise<ApiResponse<PaginatedResponse<TicketOperationResponse>>> => {
    return axiosClient.get('/api/bookings/operations/tickets', {
      params: { q: query, pageNumber, pageSize },
    });
  },

  /**
   * Check-in vé tại quầy.
   * Endpoint: PUT /api/bookings/operations/tickets/{bookingId}/check-in
   */
  checkInTicket: async (
    bookingId: string
  ): Promise<ApiResponse<TicketOperationResponse>> => {
    return axiosClient.put(`/api/bookings/operations/tickets/${bookingId}/check-in`);
  },
};
