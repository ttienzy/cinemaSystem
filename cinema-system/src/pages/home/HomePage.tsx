import React, { useEffect, useState } from 'react';
import HeroCarousel from '../../components/HeroCarousel';
import QuickBooking from '../../components/QuickBooking';
import MovieGrid from '../../components/MovieGrid';
import CinemaSection from '../../components/CinemaSection';
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import {
    clearError,
    enableLoading,
    getMoviesAndCinemasInfo,
    getMoviesComingSoon,
    unenableLoading
} from "../../store/slices/movieSlice";
import type { SearchParams } from '../../types/movie.types';
import { getCinemas } from '../../store/slices/cinemaSlice';



const HomePage: React.FC = () => {
    const dispatch = useAppDispatch();
    const {
        moviesComingSoon,
        movieFeature,
        movies,
        cinemas,
        statistic,
        loading
    } = useAppSelector(state => state.movie);

    const [currentSearch, setCurrentSearch] = useState<SearchParams | null>(null);

    useEffect(() => {
        // Load initial data when component mounts
        const loadInitialData = async () => {
            dispatch(clearError());
            dispatch(enableLoading());

            try {
                await Promise.all([
                    dispatch(getMoviesComingSoon()),
                    dispatch(getMoviesAndCinemasInfo()),
                    dispatch(getCinemas())
                ]);
            } catch (error) {
                console.error('Failed to load initial data:', error);
            } finally {
                dispatch(unenableLoading());
            }
        }
        loadInitialData();
    }, [dispatch]);

    const handleSearch = (searchParams: SearchParams) => {
        setCurrentSearch(searchParams);

        // Simulate search loading (replace with actual API call result handling)
        setTimeout(() => {
        }, 1000);

        console.log('Search initiated with params:', searchParams);
    };

    const getMovieGridTitle = () => {

        if (currentSearch) {
            const cinemaName = cinemas.find(c =>
                c.cinemaId.toString() === currentSearch.cinemaId
            )?.cinemaName;
            const movieName = movies.find(m =>
                m.movieId.toString() === currentSearch.movieId
            )?.title;

            return `Suất Chiếu - ${movieName} tại ${cinemaName}`;
        }

        return "Kết Quả Tìm Kiếm";
    };


    // Debug log
    console.log({
        moviesComingSoon,
        movieFeature,
        movies,
        cinemas,
        statistic,
        currentSearch,
    });

    return (
        <>
            {/* Hero Carousel - Featured Movies */}
            <HeroCarousel />

            {/* Quick Booking Widget */}
            <div className="container mx-auto">
                <QuickBooking onSearch={handleSearch} />
            </div>

            {/* Movie Grid - Shows search results or default movies */}
            <MovieGrid
                title={getMovieGridTitle()}
                isLoading={loading}
            />

            {/* Cinema Section */}
            <CinemaSection />

        </>
    );
};

export default HomePage;