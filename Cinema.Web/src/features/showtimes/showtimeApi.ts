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

export const showtimeApi = {
  getShowtimesByMovie(movieId: string) {
    return axiosClient.get<never, ApiResponse<Showtime[]>>(`/api/v1/showtimes/movie/${movieId}`);
  },
};
