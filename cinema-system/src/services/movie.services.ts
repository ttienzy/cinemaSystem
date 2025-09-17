import { api } from "../configs/api";


export const movieServices = {
    getMoviesComingSoon: async () => {
        const response = await api.get('/movies/coming-soon');
        return response.data;
    },
    getMoviesAndCinemasInfo: async () => {
        const response = await api.get('/movies/movie-and-cinema-info');
        return response.data;
    },
    getMoviesFeature: async (cinemaId: string, movieId: string, showDate: string) => {
        const response = await api.get('/showtimes/detail', {
            params: {
                cinemaId: cinemaId,
                movieId: movieId,
                showDate: showDate
            }
        });
        return response.data;
    },
    getMoviesListFeature: async (cinemaId: string, showDate: string) => {
        const response = await api.get('/showtimes', {
            params: {
                cinemaId: cinemaId,
                movieId: '00000000-0000-0000-0000-000000000000',
                showDate: showDate
            }
        });
        return response.data;
    },
    getMovieDetails: async (movieId: string) => {
        const response = await api.get('/movies/' + movieId);
        return response.data;
    },
    getMoviesSection: async () => {
        const response = await api.get('/movies/section');
        return response.data;
    },
}