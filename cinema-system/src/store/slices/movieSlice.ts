import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import type { MovieComingSoon, CinemaQuery, MovieQuery, Movie, StatisticItem, MovieDetailsData, MovieSection } from "../../types/movie.types";
import { movieServices } from "../../services/movie.services";




interface MovieState {
    moviesComingSoon: MovieComingSoon[];
    cinemas: CinemaQuery[];
    movies: MovieQuery[];
    moviesSection: MovieSection[];
    movieFeature: Movie | null;
    moviesListFeature: Movie[];
    statistic: StatisticItem;
    movieDetail: MovieDetailsData;
    loading: boolean;
    error: string | null;
}


const initialState: MovieState = {
    moviesComingSoon: [],
    moviesSection: [],
    cinemas: [],
    movies: [],
    movieFeature: null,
    moviesListFeature: [],
    statistic: { totalCinemas: 0, totalMovies: 0, totalUsers: 0 },
    movieDetail: {
        movie: {
            title: '',
            description: '',
            posterUrl: '',
            durationMinutes: 0,
            releaseDate: '',
            status: '',
            createdAt: ''
        },
        castCrews: [],
        certifications: [],
        genres: [],
        copyrights: []
    },
    loading: false,
    error: null
}

// Async thunk
export const getMoviesComingSoon = createAsyncThunk(
    'movie/getMoviesComingSoon',
    async () => {
        return await movieServices.getMoviesComingSoon();
    }
)

export const getMoviesAndCinemasInfo = createAsyncThunk(
    'movie/getMoviesAndCinemasInfo',
    async () => {
        return await movieServices.getMoviesAndCinemasInfo();
    }
)
export const getMovieDetails = createAsyncThunk(
    'movie/getMovieDetails',
    async (movieId: string) => {
        return await movieServices.getMovieDetails(movieId);
    }
)

export const getMoviesFeature = createAsyncThunk(
    'movie/getMoviesFeature',
    async (
        params: { cinemaId: string; movieId: string; showDate: string }
    ) => {
        const { cinemaId, movieId, showDate } = params;
        return await movieServices.getMoviesFeature(cinemaId, movieId, showDate);
    }
)
export const getMoviesSection = createAsyncThunk(
    'movie/getMoviesSection',
    async () => {
        return await movieServices.getMoviesSection();
    }
)
export const getMoviesListFeature = createAsyncThunk(
    'movie/getMoviesListFeature',
    async (
        params: { cinemaId: string; showDate: string }
    ) => (
        await movieServices.getMoviesListFeature(
            params.cinemaId,
            params.showDate
        )
    )
)

export const movieSlice = createSlice({
    name: 'movie',
    initialState,
    reducers: {
        clearError: (state) => {
            state.error = null;
        },
        enableLoading: (state) => {
            state.loading = true
        },
        unenableLoading: (state) => {
            state.loading = false;
        }
    },
    extraReducers: (builder) => {
        // get movies coming soon
        builder.addCase(getMoviesComingSoon.fulfilled, (state, action) => {
            state.moviesComingSoon = action.payload;
        });
        builder.addCase(getMoviesComingSoon.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Get movies comingsoon failed';
        })
        // get movies base and cinemas base
        builder.addCase(getMoviesAndCinemasInfo.fulfilled, (state, action) => {
            state.cinemas = action.payload.cinemaBaseResponse;
            state.movies = action.payload.movieBaseResponse;
            state.statistic = action.payload.statisticItemResponse;
        });
        builder.addCase(getMoviesAndCinemasInfo.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Get movies or cinemas failed';
        })
        // get movies feature
        builder.addCase(getMoviesFeature.fulfilled, (state, action) => {
            state.movieFeature = action.payload;
        });
        builder.addCase(getMoviesFeature.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Get feature failed';
        })
        // get movie details
        builder.addCase(getMovieDetails.fulfilled, (state, action) => {
            state.movieDetail = action.payload;
        });
        builder.addCase(getMovieDetails.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Get movie details failed';
        })
        // get movies section
        builder.addCase(getMoviesSection.fulfilled, (state, action) => {
            state.moviesSection = action.payload;
        });
        builder.addCase(getMoviesSection.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Get movies section failed';
        })
        // get movies list feature
        builder.addCase(getMoviesListFeature.fulfilled, (state, action) => {
            state.moviesListFeature = action.payload;
        });
        builder.addCase(getMoviesListFeature.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Get movies list feature failed';
        })
    }
});

export const { clearError, enableLoading, unenableLoading } = movieSlice.actions;
export default movieSlice.reducer;