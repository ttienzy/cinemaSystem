import axiosClient from '../../../api/axiosClient';
import type { ApiResponse, PaginatedResponse } from '../../../types/api';

// ---- DTOs ----

export interface GenreDto {
  id: string;
  name: string;
}

export interface Movie {
  id: string;
  title: string;
  description?: string;
  duration: number;
  language?: string;
  releaseDate: string;
  posterUrl?: string;
  status: string; // 'Showing' | 'ComingSoon' | 'Archived'
  genres: GenreDto[];
  createdAt: string;
}

export interface MovieAdminListItem extends Movie {
  totalShowtimes: number;
  upcomingShowtimesCount: number;
  nextShowtimeAt: string | null;
  lastShowtimeAt: string | null;
}

export interface MovieAdminSummary {
  totalMovies: number;
  showingMovies: number;
  comingSoonMovies: number;
  archivedMovies: number;
}

// ---- API Calls ----

export const movieApi = {
  // Public
  getMovies: async (pageNumber: number, pageSize: number): Promise<ApiResponse<PaginatedResponse<Movie>>> => {
    return axiosClient.get('/api/movies', { params: { pageNumber, pageSize } });
  },
  getMovieById: async (id: string): Promise<ApiResponse<Movie>> => {
    return axiosClient.get(`/api/movies/${id}`);
  },

  // Admin
  getAdminList: async (params: {
    search?: string; status?: string; genreId?: string;
    pageNumber?: number; pageSize?: number;
  }): Promise<ApiResponse<PaginatedResponse<MovieAdminListItem>>> => {
    return axiosClient.get('/api/movies/admin/list', { params });
  },
  getAdminSummary: async (): Promise<ApiResponse<MovieAdminSummary>> => {
    return axiosClient.get('/api/movies/admin/summary');
  },

  // CRUD — multipart/form-data for poster upload
  createMovie: async (data: FormData): Promise<ApiResponse<Movie>> => {
    return axiosClient.post('/api/movies', data, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  updateMovie: async (id: string, data: FormData): Promise<ApiResponse<Movie>> => {
    return axiosClient.put(`/api/movies/${id}`, data, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  deleteMovie: async (id: string): Promise<ApiResponse<void>> => {
    return axiosClient.delete(`/api/movies/${id}`);
  },
};
