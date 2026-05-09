import axiosClient from '../../../api/axiosClient';
import type { ApiResponse } from '../../../types/api';

export interface CreateShowtimeRequest {
  movieId: string;
  cinemaHallId: string;
  startTime: string; // ISO string in UTC
  price: number;
}

export interface Showtime {
  id: string;
  movieId: string;
  movieTitle: string;
  cinemaHallId: string;
  cinemaName: string;
  cinemaHallName: string;
  startTime: string;
  endTime: string;
  price: number;
  durationMinutes: number;
  createdAt: string;
}

export const showtimeApi = {
  createShowtime: async (data: CreateShowtimeRequest): Promise<ApiResponse<any>> => {
    return axiosClient.post('/api/showtimes', data);
  },
  getShowtimesByMovie: async (movieId: string): Promise<ApiResponse<Showtime[]>> => {
    return axiosClient.get(`/api/showtimes/movie/${movieId}`);
  }
};
