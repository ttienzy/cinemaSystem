// Showtimes API - Based on Backend API endpoints
import axios from '../../../shared/api/axios.instance';
import { Endpoints } from '../../../shared/api/endpoints';
import type { Showtime, SeatingPlan, ShowtimeSearchParams } from '../types/showtime.types';

export interface ShowtimeListResponse {
    items: Showtime[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
}

export const showtimesApi = {
    // Get showtimes with filters
    // Note: Backend doesn't have a base list endpoint
    // Use BY_MOVIE or BY_CINEMA instead based on your needs
    getShowtimes: async (params?: ShowtimeSearchParams): Promise<ShowtimeListResponse> => {
        // If movieId is provided, use BY_MOVIE endpoint
        if (params?.movieId) {
            const response = await axios.get<ShowtimeListResponse>(
                Endpoints.SHOWTIMES.BY_MOVIE(params.movieId, params.date)
            );
            return response.data;
        }
        // If cinemaId is provided, use BY_CINEMA endpoint
        if (params?.cinemaId) {
            const response = await axios.get<ShowtimeListResponse>(
                Endpoints.SHOWTIMES.BY_CINEMA(params.cinemaId, params.date)
            );
            return response.data;
        }
        // Fallback: return empty list if no filter provided
        // Backend requires either movieId or cinemaId
        return {
            items: [],
            totalCount: 0,
            pageIndex: 1,
            pageSize: 20,
            totalPages: 0,
        };
    },

    // Get showtimes by movie
    getShowtimesByMovie: async (
        movieId: string,
        date?: string
    ): Promise<ShowtimeListResponse> => {
        const response = await axios.get<ShowtimeListResponse>(
            Endpoints.SHOWTIMES.BY_MOVIE(movieId, date)
        );
        return response.data;
    },

    // Get showtimes by cinema
    getShowtimesByCinema: async (
        cinemaId: string,
        date?: string
    ): Promise<ShowtimeListResponse> => {
        const response = await axios.get<ShowtimeListResponse>(
            Endpoints.SHOWTIMES.BY_CINEMA(cinemaId, date)
        );
        return response.data;
    },

    // Get seating plan for a showtime
    getSeatingPlan: async (showtimeId: string): Promise<SeatingPlan> => {
        const response = await axios.get<SeatingPlan>(
            Endpoints.SHOWTIMES.SEATING_PLAN(showtimeId)
        );
        return response.data;
    },
};

export default showtimesApi;
