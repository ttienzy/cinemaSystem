import axiosClient from '../../../api/axiosClient';
import type { ApiResponse, PaginatedResponse } from '../../../types/api';

// ---- DTOs ----

export interface Cinema {
  id: string;
  name: string;
  address: string;
  city?: string;
  status: string; // 'Active' | 'Inactive'
  totalHalls: number;
  totalSeats: number;
  createdAt: string;
}

export interface CinemaHall {
  id: string;
  cinemaId: string;
  name: string;
  roomType: string;
  totalSeats: number;
  seatMapConfigured: boolean;
  normalSeatCount: number;
  vipSeatCount: number;
  coupleSeatCount: number;
  createdAt: string;
}

export interface CinemaAdminOverview extends Cinema {
  cinemaHalls: CinemaHall[];
}

export interface CinemaAdminSummary {
  totalCinemas: number;
  activeCinemas: number;
  inactiveCinemas: number;
  totalHalls: number;
  totalSeats: number;
}

export interface Seat {
  id: string;
  cinemaHallId: string;
  row: string;
  number: number;
  displayName: string;
}

// ---- API Calls ----

export const cinemaApi = {
  // Public
  getCinemas: async (pageNumber: number = 1, pageSize: number = 100): Promise<ApiResponse<PaginatedResponse<Cinema>>> => {
    return axiosClient.get('/api/cinemas', { params: { pageNumber, pageSize } });
  },
  getHallsByCinemaId: async (cinemaId: string): Promise<ApiResponse<CinemaHall[]>> => {
    return axiosClient.get(`/api/cinema-halls/cinema/${cinemaId}`);
  },

  // Admin
  getAdminOverview: async (params: {
    search?: string; city?: string; status?: string;
    pageNumber?: number; pageSize?: number;
  }): Promise<ApiResponse<PaginatedResponse<CinemaAdminOverview>>> => {
    return axiosClient.get('/api/cinemas/admin/overview', { params });
  },
  getAdminSummary: async (): Promise<ApiResponse<CinemaAdminSummary>> => {
    return axiosClient.get('/api/cinemas/admin/summary');
  },

  // CRUD - Cinema
  getCinemaById: async (id: string): Promise<ApiResponse<Cinema>> => {
    return axiosClient.get(`/api/cinemas/${id}`);
  },
  createCinema: async (data: { name: string; address: string; city?: string }): Promise<ApiResponse<Cinema>> => {
    return axiosClient.post('/api/cinemas', data);
  },
  updateCinema: async (id: string, data: { name: string; address: string; city?: string }): Promise<ApiResponse<Cinema>> => {
    return axiosClient.put(`/api/cinemas/${id}`, data);
  },
  deleteCinema: async (id: string): Promise<ApiResponse<void>> => {
    return axiosClient.delete(`/api/cinemas/${id}`);
  },

  // CRUD - Cinema Hall
  getHallById: async (id: string): Promise<ApiResponse<CinemaHall>> => {
    return axiosClient.get(`/api/cinema-halls/${id}`);
  },
  createHall: async (data: { cinemaId: string; name: string }): Promise<ApiResponse<CinemaHall>> => {
    return axiosClient.post('/api/cinema-halls', data);
  },
  updateHall: async (id: string, data: { name: string }): Promise<ApiResponse<CinemaHall>> => {
    return axiosClient.put(`/api/cinema-halls/${id}`, data);
  },
  deleteHall: async (id: string): Promise<ApiResponse<void>> => {
    return axiosClient.delete(`/api/cinema-halls/${id}`);
  },

  // CRUD - Seats
  getSeatsByHallId: async (hallId: string): Promise<ApiResponse<Seat[]>> => {
    return axiosClient.get(`/api/seats/hall/${hallId}`);
  },
  createSeat: async (data: { cinemaHallId: string; row: string; number: number }): Promise<ApiResponse<Seat>> => {
    return axiosClient.post('/api/seats', data);
  },
  bulkCreateSeats: async (data: { cinemaHallId: string; seats: { row: string; number: number }[] }): Promise<ApiResponse<Seat[]>> => {
    return axiosClient.post('/api/seats/bulk', data);
  },
  updateSeat: async (id: string, data: { row: string; number: number }): Promise<ApiResponse<Seat>> => {
    return axiosClient.put(`/api/seats/${id}`, data);
  },
  deleteSeat: async (id: string): Promise<ApiResponse<void>> => {
    return axiosClient.delete(`/api/seats/${id}`);
  },
  bulkDeleteSeats: async (seatIds: string[]): Promise<ApiResponse<void>> => {
    return axiosClient.post('/api/seats/bulk-delete', seatIds);
  },
};
