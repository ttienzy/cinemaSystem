import axiosClient from '../../shared/api/axiosClient';
import type { ApiResponse, PaginatedResponse } from '../../shared/types/api';

export interface Genre {
  id: string;
  name: string;
}

export interface Movie {
  id: string;
  title: string;
  description?: string | null;
  duration: number;
  language?: string | null;
  releaseDate: string;
  posterUrl?: string | null;
  status: string;
  genres: Genre[];
  createdAt: string;
}

export interface MovieDetail extends Movie {
  showtimes: ShowtimeSummary[];
}

export interface ShowtimeSummary {
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

export const movieApi = {
  getMovies(pageNumber = 1, pageSize = 100) {
    return axiosClient.get<never, ApiResponse<PaginatedResponse<Movie>>>('/api/v1/movies', {
      params: { pageNumber, pageSize },
    });
  },
  getMovieById(id: string) {
    return axiosClient.get<never, ApiResponse<MovieDetail>>(`/api/v1/movies/${id}`);
  },
};
