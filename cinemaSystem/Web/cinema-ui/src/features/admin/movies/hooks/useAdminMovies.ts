// Admin Movies Hooks - React Query hooks for admin movie management
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { adminMoviesApi } from '../api/adminMoviesApi';
import type { CreateMovieRequest } from '../api/adminMoviesApi';
import { movieKeys } from '../../../movies/hooks/useMovies';

// Query keys for admin movies
const adminMovieKeys = {
    all: ['admin', 'movies'] as const,
    list: () => [...adminMovieKeys.all, 'list'] as const,
    detail: (id: string) => [...adminMovieKeys.all, 'detail', id] as const,
};

// Hook: Get all admin movies
export const useAdminMovies = () => {
    return useQuery({
        queryKey: adminMovieKeys.list(),
        queryFn: () => adminMoviesApi.getAllMovies(),
        staleTime: 5 * 60 * 1000, // 5 minutes
    });
};

export const useCreateMovie = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (data: CreateMovieRequest) => adminMoviesApi.createMovie(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: movieKeys.all });
        },
    });
};

export const useUpdateMovie = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ id, data }: { id: string; data: Omit<CreateMovieRequest, 'id'> }) =>
            adminMoviesApi.updateMovie(id, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: movieKeys.all });
        },
    });
};

export const useDeleteMovie = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: string) => adminMoviesApi.deleteMovie(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: movieKeys.all });
        },
    });
};

export default {
    useAdminMovies,
    useCreateMovie,
    useUpdateMovie,
    useDeleteMovie,
};
