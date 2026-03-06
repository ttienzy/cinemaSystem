// Cinemas Hooks - React Query hooks for cinemas
import { useQuery } from '@tanstack/react-query';
import { cinemasApi } from '../api/cinemasApi';

// Query keys
const cinemaKeys = {
    all: ['cinemas'] as const,
    lists: () => [...cinemaKeys.all, 'list'] as const,
    list: (pageIndex: number, pageSize: number) =>
        [...cinemaKeys.lists(), pageIndex, pageSize] as const,
    details: () => [...cinemaKeys.all, 'detail'] as const,
    detail: (id: string) => [...cinemaKeys.details(), id] as const,
};

export { cinemaKeys };

// Hook: Get all cinemas
export const useCinemas = (pageIndex = 1, pageSize = 20) => {
    return useQuery({
        queryKey: cinemaKeys.list(pageIndex, pageSize),
        queryFn: () => cinemasApi.getCinemas(pageIndex, pageSize),
        staleTime: 5 * 60 * 1000, // 5 minutes
    });
};

// Hook: Get cinema by ID
export const useCinema = (id: string) => {
    return useQuery({
        queryKey: cinemaKeys.detail(id),
        queryFn: () => cinemasApi.getCinemaById(id),
        enabled: !!id,
        staleTime: 5 * 60 * 1000,
    });
};

export default {
    useCinemas,
    useCinema,
};
