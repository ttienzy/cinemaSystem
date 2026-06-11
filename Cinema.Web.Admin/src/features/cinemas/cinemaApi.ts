import axiosClient from '../../shared/api/axiosClient';
import type { ApiResponse, PaginatedResponse } from '../../shared/types/api';

export interface Cinema {
  id: string;
  name: string;
  address: string;
  city?: string | null;
  status: string;
  totalHalls: number;
  totalSeats: number;
  createdAt: string;
}

export interface CinemaHall {
  id: string;
  cinemaId: string;
  name: string;
  totalSeats: number;
  seatMapConfigured: boolean;
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

export interface CinemaOverviewParams {
  search?: string;
  city?: string;
  status?: string;
  pageNumber: number;
  pageSize: number;
}

export const cinemaStatuses = ['Active', 'Inactive'] as const;

export const cinemaApi = {
  getCinemas(pageNumber = 1, pageSize = 100) {
    return axiosClient.get<never, ApiResponse<PaginatedResponse<Cinema>>>('/api/v1/cinemas', {
      params: { pageNumber, pageSize },
    });
  },
  getOverview(params: CinemaOverviewParams) {
    return axiosClient.get<never, ApiResponse<PaginatedResponse<CinemaAdminOverview>>>('/api/v1/cinemas/admin/overview', {
      params,
    });
  },
  getSummary() {
    return axiosClient.get<never, ApiResponse<CinemaAdminSummary>>('/api/v1/cinemas/admin/summary');
  },
  createCinema(data: { name: string; address: string; city?: string }) {
    return axiosClient.post<never, ApiResponse<Cinema>>('/api/v1/cinemas', data);
  },
  updateCinema(id: string, data: { name: string; address: string; city?: string }) {
    return axiosClient.put<never, ApiResponse<Cinema>>(`/api/v1/cinemas/${id}`, data);
  },
  deleteCinema(id: string) {
    return axiosClient.delete<never, ApiResponse<boolean>>(`/api/v1/cinemas/${id}`);
  },
  getHallsByCinema(cinemaId: string) {
    return axiosClient.get<never, ApiResponse<CinemaHall[]>>(`/api/v1/cinema-halls/cinema/${cinemaId}`);
  },
  createHall(data: { cinemaId: string; name: string }) {
    return axiosClient.post<never, ApiResponse<CinemaHall>>('/api/v1/cinema-halls', data);
  },
  updateHall(id: string, data: { name: string }) {
    return axiosClient.put<never, ApiResponse<CinemaHall>>(`/api/v1/cinema-halls/${id}`, data);
  },
  deleteHall(id: string) {
    return axiosClient.delete<never, ApiResponse<boolean>>(`/api/v1/cinema-halls/${id}`);
  },
  getSeatsByHall(hallId: string) {
    return axiosClient.get<never, ApiResponse<Seat[]>>(`/api/v1/seats/hall/${hallId}`);
  },
  createSeat(data: { cinemaHallId: string; row: string; number: number }) {
    return axiosClient.post<never, ApiResponse<Seat>>('/api/v1/seats', data);
  },
  bulkCreateSeats(data: { cinemaHallId: string; seats: Array<{ row: string; number: number }> }) {
    return axiosClient.post<never, ApiResponse<Seat[]>>('/api/v1/seats/bulk', data);
  },
  updateSeat(id: string, data: { row: string; number: number }) {
    return axiosClient.put<never, ApiResponse<Seat>>(`/api/v1/seats/${id}`, data);
  },
  deleteSeat(id: string) {
    return axiosClient.delete<never, ApiResponse<boolean>>(`/api/v1/seats/${id}`);
  },
  bulkDeleteSeats(seatIds: string[]) {
    return axiosClient.post<never, ApiResponse<boolean>>('/api/v1/seats/bulk-delete', seatIds);
  },
};
