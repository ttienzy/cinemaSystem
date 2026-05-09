// ============================================================
// Showtime Timeline API — Dữ liệu cho Timeline View
// ============================================================

import axiosClient from '../../../api/axiosClient';
import type { ApiResponse } from '../../../types/api';

// ---- DTOs matching backend ShowtimeTimelineDto ----

export interface TimelineShowtimeDto {
  id: string;
  movieId: string;
  movieTitle: string;
  start: string;  // ISO datetime
  end: string;    // ISO datetime
  cleaningEnd: string;
  durationMinutes: number;
  cleaningBufferMinutes: number;
  price: number;
  totalSeats: number;
  bookedSeats: number;
  occupancyRate: number; // 0-100
  hasBookings: boolean;
  canReschedule: boolean;
}

export interface TimelineRoomDto {
  roomId: string;
  roomName: string;
  totalSeats: number;
  showtimes: TimelineShowtimeDto[];
}

export interface ShowtimeTimelineDto {
  cinemaId: string;
  cinemaName: string;
  date: string;
  timelineStart: string;
  timelineEnd: string;
  cleaningBufferMinutes: number;
  rooms: TimelineRoomDto[];
}

// ---- Validate Slot DTOs ----

export interface ValidateSlotRequest {
  movieId: string;
  cinemaHallId: string;
  startTime: string;
  excludeShowtimeId?: string;
}

export interface ShowtimeConflictItem {
  showtimeId: string;
  movieId: string;
  movieTitle: string;
  startTime: string;
  endTime: string;
  cleaningEndTime: string;
}

export interface SlotValidationResponse {
  isAvailable: boolean;
  movieId: string;
  cinemaHallId: string;
  proposedStartTime: string;
  proposedEndTime: string;
  proposedCleaningEnd: string;
  cleaningBufferMinutes: number;
  conflicts: ShowtimeConflictItem[];
}

// ---- API Calls ----

export const showtimeTimelineApi = {
  /**
   * Lấy timeline theo cụm rạp + ngày.
   * GET /api/showtimes/timeline?cinemaId={id}&date=2026-05-02
   */
  getTimeline: async (
    cinemaId: string,
    date: string
  ): Promise<ApiResponse<ShowtimeTimelineDto>> => {
    return axiosClient.get('/api/showtimes/timeline', {
      params: { cinemaId, date },
    });
  },

  /**
   * Validate xem slot mới có bị conflict không.
   * POST /api/showtimes/validate-slot
   */
  validateSlot: async (
    data: ValidateSlotRequest
  ): Promise<ApiResponse<SlotValidationResponse>> => {
    return axiosClient.post('/api/showtimes/validate-slot', data);
  },
};
