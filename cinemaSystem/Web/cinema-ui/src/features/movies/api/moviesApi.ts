// Movies API - Based on Backend API endpoints
import axios from '../../../shared/api/axios.instance';
import { Endpoints } from '../../../shared/api/endpoints';
import type { MovieListResponse, MovieDetailResponse, MovieSearchParams } from '../types/movie.types';

export interface Genre {
    id: string;
    name: string;
}

export const moviesApi = {
    // Get all movies with pagination and filters
    getMovies: async (params?: MovieSearchParams): Promise<MovieListResponse> => {
        const response = await axios.get<MovieListResponse>(Endpoints.MOVIES.BASE, { params });
        return response.data;
    },

    // Get movies currently showing
    getNowShowing: async (pageIndex = 1, pageSize = 20): Promise<MovieListResponse> => {
        const response = await axios.get<MovieListResponse>(Endpoints.MOVIES.NOW_SHOWING, {
            params: { pageIndex, pageSize },
        });
        return response.data;
    },

    // Get upcoming movies
    getComingSoon: async (pageIndex = 1, pageSize = 20): Promise<MovieListResponse> => {
        const response = await axios.get<MovieListResponse>(Endpoints.MOVIES.COMING_SOON, {
            params: { pageIndex, pageSize },
        });
        return response.data;
    },

    // Get movie details by ID
    getMovieById: async (id: string): Promise<MovieDetailResponse> => {
        const response = await axios.get<MovieDetailResponse>(Endpoints.MOVIES.DETAIL(id));
        return response.data;
    },

    // Search movies
    searchMovies: async (keyword: string, pageIndex = 1, pageSize = 20): Promise<MovieListResponse> => {
        const response = await axios.get<MovieListResponse>(Endpoints.MOVIES.BASE, {
            params: { keyword, pageIndex, pageSize },
        });
        return response.data;
    },

    // Get all genres
    getGenres: async (): Promise<Genre[]> => {
        const response = await axios.get<Genre[]>(Endpoints.GENRES.BASE);
        return response.data;
    },
};

export default moviesApi;
