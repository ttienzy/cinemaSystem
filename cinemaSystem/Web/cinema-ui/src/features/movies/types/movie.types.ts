// Movie Types - Based on Backend API
import type { Showtime } from '../../showtime/types/showtime.types';
export interface Movie {
    id: string;
    title: string;
    originalTitle?: string;
    posterUrl: string;
    backdropUrl?: string;
    trailerUrl?: string;
    synopsis: string;
    director: string;
    cast: string[];
    duration: number; // in minutes
    releaseDate: string; // ISO date
    endDate?: string; // ISO date
    rating?: string; // e.g., "PG-13", "C18"
    isNowShowing: boolean;
    isComingSoon: boolean;
    genres: Genre[];
    languages: string[];
    subtitles: string[];
    averageRating?: number;
    totalReviews: number;
    status: MovieStatus;
    createdAt: string;
    updatedAt: string;
}

export type MovieStatus = 'Draft' | 'Published' | 'Archived';

export interface Genre {
    id: string;
    name: string;
    slug: string;
}

// Movie List Response
export interface MovieListResponse {
    items: Movie[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
}

// Movie Detail Response (extends Movie with additional info)
export interface MovieDetailResponse extends Movie {
    // Add any additional fields specific to detail view
    showtimes?: Showtime[];
}

// Search/Filter Movies
export interface MovieSearchParams {
    pageIndex?: number;
    pageSize?: number;
    keyword?: string;
    genreId?: string;
    status?: MovieStatus;
    fromDate?: string;
    toDate?: string;
}

// Admin Movie Management
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

export interface UpdateMovieRequest extends Partial<CreateMovieRequest> {
    id: string;
}

// For Home Page - Separate Now Showing and Coming Soon
export interface MovieCategory {
    movies: Movie[];
    totalCount: number;
}
