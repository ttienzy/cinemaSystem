import React, { useEffect, useState, useMemo, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';

// --- Component Imports ---
import HeroCarousel from '../../components/HeroCarousel';
import QuickBooking from '../../components/QuickBooking';
import MovieGrid from '../../components/MovieGrid';
import CinemaSection from '../../components/CinemaSection';

// --- Redux & Utils ---
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { getMoviesAndCinemasInfo, getMoviesComingSoon } from "../../store/slices/movieSlice";
import { getCinemas } from '../../store/slices/cinemaSlice';
import { decodeTokenAndGetUser } from '../../utils/decodeTokenAndGetUser';

// --- Type Imports ---
import type { SearchParams } from '../../types/movie.types';

// =================================================================
// HOME PAGE COMPONENT
// =================================================================
const HomePage: React.FC = () => {
    // --- HOOKS ---
    const dispatch = useAppDispatch();
    const navigate = useNavigate();

    // --- REDUX STATE ---
    const { movies, cinemas, loading } = useAppSelector(state => state.movie);

    // --- LOCAL STATE ---
    const [currentSearch, setCurrentSearch] = useState<SearchParams | null>(null);

    // --- CAUSE #1 FIX: USER REDIRECTION LOGIC ---
    // This effect runs ONLY ONCE after the initial render, because of the empty dependency array [].
    // This is the correct place for side effects like checking authentication and redirecting.
    useEffect(() => {
        const userFromToken = decodeTokenAndGetUser(localStorage.getItem('accessToken'));
        console.log('Decoded User from Token:', userFromToken);

        if (userFromToken) {
            const { role } = userFromToken;
            if (role.includes('Admin')) {
                navigate('/dashboard/admin', { replace: true });
            } else if (role.includes('Manager')) {
                navigate('/dashboard/manager', { replace: true });
            } else if (role.includes('Employee')) {
                navigate('/dashboard/employee', { replace: true });
            }
            // If the user has a token but none of the above roles, they stay here.
        }
        // If there's no token, the user is not logged in and should stay on the homepage.
        // The initial 'navigate('/')' was redundant and potentially problematic.
    }, [navigate]); // navigate is stable and won't cause re-runs.

    // --- DATA FETCHING EFFECT ---
    // This effect also runs only once on mount to load initial page data.
    useEffect(() => {
        dispatch(getMoviesComingSoon());
        dispatch(getMoviesAndCinemasInfo());
        dispatch(getCinemas());
    }, [dispatch]); // dispatch is stable and won't cause re-runs.

    // --- CAUSE #2 FIX: MEMOIZED DERIVED STATE (TITLE) ---
    // useMemo will only re-calculate the title when its dependencies change.
    // This prevents running expensive '.find()' operations on every single render.
    const movieGridTitle = useMemo(() => {
        if (currentSearch) {
            const cinemaName = cinemas.find(c => c.cinemaId.toString() === currentSearch.cinemaId)?.cinemaName || '...';
            const movieName = movies.find(m => m.movieId.toString() === currentSearch.movieId)?.title || '...';
            return `Showing results for "${movieName}" at ${cinemaName}`;
        }
        return "Now Showing"; // A more descriptive default title.
    }, [currentSearch, cinemas, movies]);

    // --- CAUSE #3 FIX: MEMOIZED EVENT HANDLER ---
    // useCallback ensures that the handleSearch function reference does not change on re-renders.
    // This is important for performance if QuickBooking is a memoized component.
    const handleSearch = useCallback((searchParams: SearchParams) => {
        setCurrentSearch(searchParams);
        console.log('Search initiated with params:', searchParams);
        // Any logic related to the search would go here.
    }, []); // No dependencies, so this function is created only once.

    // --- RENDER ---
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
                title={movieGridTitle}
                isLoading={loading}
            />

            {/* Cinema Section */}
            <CinemaSection />
        </>
    );
};

export default HomePage;