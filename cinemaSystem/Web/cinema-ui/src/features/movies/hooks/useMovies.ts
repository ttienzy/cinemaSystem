// Movies Hooks - React Query hooks for movies
import { useQuery } from '@tanstack/react-query';
import { moviesApi } from '../api/moviesApi';
import type { MovieSearchParams } from '../types/movie.types';

// Query keys
export const movieKeys = {
    all: ['movies'] as const,
    lists: () => [...movieKeys.all, 'list'] as const,
    list: (params: MovieSearchParams) => [...movieKeys.lists(), params] as const,
    details: () => [...movieKeys.all, 'detail'] as const,
    detail: (id: string) => [...movieKeys.details(), id] as const,
    nowShowing: (pageIndex: number, pageSize: number) =>
        [...movieKeys.all, 'nowShowing', pageIndex, pageSize] as const,
    comingSoon: (pageIndex: number, pageSize: number) =>
        [...movieKeys.all, 'comingSoon', pageIndex, pageSize] as const,
};

// Hook: Get genres for movie filtering
export const useGenres = () => {
    return useQuery({
        queryKey: ['genres'],
        queryFn: () => moviesApi.getGenres(),
        staleTime: 60 * 60 * 1000, // 1 hour - genres don't change often
    });
};

// Hook: Get movies currently showing
export const useNowShowingMovies = (pageIndex = 1, pageSize = 20) => {
    return useQuery({
        queryKey: movieKeys.nowShowing(pageIndex, pageSize),
        queryFn: () => moviesApi.getNowShowing(pageIndex, pageSize),
        staleTime: 5 * 60 * 1000, // 5 minutes
    });
};

// Hook: Get upcoming movies
export const useComingSoonMovies = (pageIndex = 1, pageSize = 20) => {
    return useQuery({
        queryKey: movieKeys.comingSoon(pageIndex, pageSize),
        queryFn: () => moviesApi.getComingSoon(pageIndex, pageSize),
        staleTime: 5 * 60 * 1000,
    });
};

// Hook: Get movie by ID
export const useMovie = (id: string) => {
    return useQuery({
        queryKey: movieKeys.detail(id),
        queryFn: () => moviesApi.getMovieById(id),
        enabled: !!id,
        staleTime: 5 * 60 * 1000,
    });
};

// Hook: Get all movies with filters
export const useMovies = (params?: MovieSearchParams) => {
    return useQuery({
        queryKey: movieKeys.list(params || {}),
        queryFn: () => moviesApi.getMovies(params),
        staleTime: 5 * 60 * 1000,
    });
};

// Hook: Search movies
export const useSearchMovies = (keyword: string, pageIndex = 1, pageSize = 20) => {
    return useQuery({
        queryKey: [...movieKeys.lists(), 'search', keyword, pageIndex, pageSize],
        queryFn: () => moviesApi.searchMovies(keyword, pageIndex, pageSize),
        enabled: keyword.length > 0,
        staleTime: 5 * 60 * 1000,
    });
};

export default {
    useNowShowingMovies,
    useComingSoonMovies,
    useMovie,
    useMovies,
    useSearchMovies,
    useGenres,
};
