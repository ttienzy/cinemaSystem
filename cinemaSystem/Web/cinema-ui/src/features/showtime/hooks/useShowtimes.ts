// Showtimes Hooks - React Query hooks for showtimes
import { useQuery } from '@tanstack/react-query';
import { showtimesApi } from '../api/showtimesApi';
import type { ShowtimeSearchParams } from '../types/showtime.types';

// Query keys
export const showtimeKeys = {
    all: ['showtimes'] as const,
    lists: () => [...showtimeKeys.all, 'list'] as const,
    list: (params: ShowtimeSearchParams) => [...showtimeKeys.lists(), params] as const,
    details: () => [...showtimeKeys.all, 'detail'] as const,
    detail: (id: string) => [...showtimeKeys.details(), id] as const,
    byMovie: (movieId: string, date?: string) =>
        [...showtimeKeys.all, 'byMovie', movieId, date] as const,
    byCinema: (cinemaId: string, date?: string) =>
        [...showtimeKeys.all, 'byCinema', cinemaId, date] as const,
    seatingPlan: (showtimeId: string) =>
        [...showtimeKeys.all, 'seatingPlan', showtimeId] as const,
};

// Hook: Get showtimes with filters
export const useShowtimes = (params?: ShowtimeSearchParams) => {
    return useQuery({
        queryKey: showtimeKeys.list(params || {}),
        queryFn: () => showtimesApi.getShowtimes(params),
        staleTime: 2 * 60 * 1000, // 2 minutes
    });
};

// Hook: Get showtimes by movie
export const useShowtimesByMovie = (movieId: string, date?: string) => {
    return useQuery({
        queryKey: showtimeKeys.byMovie(movieId, date),
        queryFn: () => showtimesApi.getShowtimesByMovie(movieId, date),
        enabled: !!movieId,
        staleTime: 2 * 60 * 1000,
    });
};

// Hook: Get showtimes by cinema
export const useShowtimesByCinema = (cinemaId: string, date?: string) => {
    return useQuery({
        queryKey: showtimeKeys.byCinema(cinemaId, date),
        queryFn: () => showtimesApi.getShowtimesByCinema(cinemaId, date),
        enabled: !!cinemaId,
        staleTime: 2 * 60 * 1000,
    });
};

// Hook: Get seating plan for a showtime
export const useSeatingPlan = (showtimeId: string) => {
    return useQuery({
        queryKey: showtimeKeys.seatingPlan(showtimeId),
        queryFn: () => showtimesApi.getSeatingPlan(showtimeId),
        enabled: !!showtimeId,
        staleTime: 30 * 1000, // 30 seconds - seating changes frequently
    });
};

export default {
    useShowtimes,
    useShowtimesByMovie,
    useShowtimesByCinema,
    useSeatingPlan,
};
