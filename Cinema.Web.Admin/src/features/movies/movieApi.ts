import axiosClient from '../../shared/api/axiosClient';
import type { ApiResponse, PaginatedResponse } from '../../shared/types/api';

export interface GenreDto {
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
  genres: GenreDto[];
  createdAt: string;
}

export interface MovieAdminListItem extends Movie {
  totalShowtimes: number;
  upcomingShowtimesCount: number;
  nextShowtimeAt?: string | null;
  lastShowtimeAt?: string | null;
}

export interface MovieAdminSummary {
  totalMovies: number;
  showingMovies: number;
  comingSoonMovies: number;
  archivedMovies: number;
}

export interface MovieAdminListParams {
  search?: string;
  status?: string;
  genreId?: string;
  pageNumber: number;
  pageSize: number;
}

export const movieStatuses = ['Showing', 'ComingSoon', 'Archived'] as const;

export const movieApi = {
  getMovies(pageNumber = 1, pageSize = 100) {
    return axiosClient.get<never, ApiResponse<PaginatedResponse<Movie>>>('/api/v1/movies', {
      params: { pageNumber, pageSize },
    });
  },
  getGenres() {
    return axiosClient.get<never, ApiResponse<GenreDto[]>>('/api/v1/genres');
  },
  getAdminList(params: MovieAdminListParams) {
    return axiosClient.get<never, ApiResponse<PaginatedResponse<MovieAdminListItem>>>('/api/v1/movies/admin/list', {
      params,
    });
  },
  getAdminSummary() {
    return axiosClient.get<never, ApiResponse<MovieAdminSummary>>('/api/v1/movies/admin/summary');
  },
  createMovie(formData: FormData) {
    return axiosClient.post<never, ApiResponse<Movie>>('/api/v1/movies', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  updateMovie(id: string, formData: FormData) {
    return axiosClient.put<never, ApiResponse<Movie>>(`/api/v1/movies/${id}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  deleteMovie(id: string) {
    return axiosClient.delete<never, ApiResponse<boolean>>(`/api/v1/movies/${id}`);
  },
};
