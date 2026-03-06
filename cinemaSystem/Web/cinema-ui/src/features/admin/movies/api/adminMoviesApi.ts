// Admin Movies API - CRUD operations for movies
import axios from '../../../../shared/api/axios.instance';
import { Endpoints } from '../../../../shared/api/endpoints';

export interface CreateMovieRequest {
    title: string;
    originalTitle?: string;
    posterUrl: string;
    backdropUrl?: string;
    trailerUrl?: string;
    synopsis: string;
    director: string;
    cast: string[];
    duration: number;
    releaseDate: string;
    endDate?: string;
    rating?: string;
    genreIds: string[];
    languages: string[];
    subtitles: string[];
}

export interface UpdateMovieRequest extends CreateMovieRequest {
    id: string;
}

export interface MovieListItem {
    id: string;
    title: string;
    posterUrl: string;
    duration: number;
    releaseDate: string;
    isNowShowing: boolean;
    isComingSoon: boolean;
    status: string;
}

export interface AdminMovieListResponse {
    data: MovieListItem[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
}

export const adminMoviesApi = {
    // Get all movies (admin)
    getAllMovies: async (): Promise<AdminMovieListResponse> => {
        // Use public movies endpoint with admin access
        const response = await axios.get(Endpoints.MOVIES.BASE);
        return response.data;
    },

    // Create a new movie
    createMovie: async (data: CreateMovieRequest): Promise<{ id: string }> => {
        const response = await axios.post(Endpoints.ADMIN_MOVIES.CREATE, data);
        return response.data;
    },

    // Update an existing movie
    updateMovie: async (id: string, data: Omit<CreateMovieRequest, 'id'>): Promise<void> => {
        const response = await axios.put(Endpoints.ADMIN_MOVIES.DETAIL(id), data);
        return response.data;
    },

    // Delete a movie
    deleteMovie: async (id: string): Promise<void> => {
        const response = await axios.delete(Endpoints.ADMIN_MOVIES.DETAIL(id));
        return response.data;
    },
};

export default adminMoviesApi;
