import axiosClient from '../../shared/api/axiosClient';
import type { ApiResponse } from '../../shared/types/api';

export interface Showtime {
  id: string;
  movieId: string;
  movieTitle: string;
  cinemaHallId: string;
  cinemaHallName?: string | null;
  cinemaName?: string | null;
  startTime: string;
  endTime: string;
  price: number;
  durationMinutes: number;
  createdAt?: string;
}

export interface ShowtimeLookupItem {
  showtimeId: string;
  movieId: string;
  movieTitle: string;
  posterUrl?: string | null;
  cinemaHallId: string;
  startTime: string;
  endTime: string;
  price: number;
}

export interface CreateShowtimeRequest {
  movieId: string;
  cinemaHallId: string;
  startTime: string;
  price: number;
}

export const showtimeApi = {
  getUpcoming(count = 50) {
    return axiosClient.get<never, ApiResponse<Showtime[]>>('/api/v1/showtimes/upcoming', {
      params: { count },
    });
  },
  getByMovie(movieId: string) {
    return axiosClient.get<never, ApiResponse<Showtime[]>>(`/api/v1/showtimes/movie/${movieId}`);
  },
  getByCinemaHall(cinemaHallId: string) {
    return axiosClient.get<never, ApiResponse<Showtime[]>>(`/api/v1/showtimes/cinemahall/${cinemaHallId}`);
  },
  getRange(from: string, to: string) {
    return axiosClient.get<never, ApiResponse<ShowtimeLookupItem[]>>('/api/v1/showtimes/range', {
      params: { from, to },
    });
  },
  createShowtime(data: CreateShowtimeRequest) {
    return axiosClient.post<never, ApiResponse<Showtime>>('/api/v1/showtimes', data);
  },
};
