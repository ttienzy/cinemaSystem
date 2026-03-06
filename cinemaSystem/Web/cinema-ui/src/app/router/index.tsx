import { createBrowserRouter } from 'react-router-dom';
import { Suspense, lazy } from 'react';
import PageLoader from '../../shared/components/common/PageLoader';

// Lazy load pages
const HomePage = lazy(() => import('../../pages/home'));
const LoginPage = lazy(() => import('../../pages/auth/login'));
const RegisterPage = lazy(() => import('../../pages/auth/register'));
const MovieListPage = lazy(() => import('../../pages/movies'));
const MovieDetailPage = lazy(() => import('../../pages/movies/[id]'));
const CinemaListPage = lazy(() => import('../../pages/cinemas'));
const BookingPage = lazy(() => import('../../pages/booking'));
const BookingSuccessPage = lazy(() => import('../../pages/booking/success'));
const BookingFailedPage = lazy(() => import('../../pages/booking/failed'));
const ProfilePage = lazy(() => import('../../pages/profile'));
const BookingHistoryPage = lazy(() => import('../../pages/profile/history'));
const BookingDetailPage = lazy(() => import('../../pages/profile/[id]'));

// Admin Pages
const AdminDashboardPage = lazy(() => import('../../pages/admin'));
const AdminMoviesPage = lazy(() => import('../../pages/admin/movies'));
const AdminCinemasPage = lazy(() => import('../../pages/admin/cinemas'));
const AdminShowtimesPage = lazy(() => import('../../pages/admin/showtimes'));
const AdminBookingsPage = lazy(() => import('../../pages/admin/bookings'));
const AdminUsersPage = lazy(() => import('../../pages/admin/users'));

// Error Pages
const Error404Page = lazy(() => import('../../pages/errors/404'));
const Error403Page = lazy(() => import('../../pages/errors/403'));

// Layouts
import AppLayout from '../../shared/components/layout/AppLayout';
import AuthLayout from '../../shared/components/layout/AuthLayout';
import AdminLayout from '../../shared/components/layout/AdminLayout';
import ProtectedRoute from '../../shared/components/common/ProtectedRoute';
import PublicRoute from '../../shared/components/common/PublicRoute';

// Router configuration
export const router = createBrowserRouter([
    // Auth routes (Login, Register)
    {
        element: <AuthLayout />,
        children: [
            {
                path: '/login',
                element: (
                    <PublicRoute>
                        <Suspense fallback={<PageLoader />}>
                            <LoginPage />
                        </Suspense>
                    </PublicRoute>
                ),
            },
            {
                path: '/register',
                element: (
                    <PublicRoute>
                        <Suspense fallback={<PageLoader />}>
                            <RegisterPage />
                        </Suspense>
                    </PublicRoute>
                ),
            },
        ],
    },

    // Admin routes (role-based access control)
    // Dashboard - Admin only
    {
        path: '/admin',
        element: (
            <ProtectedRoute allowedRoles={['Admin']}>
                <AdminLayout />
            </ProtectedRoute>
        ),
        children: [
            {
                path: '',
                element: (
                    <Suspense fallback={<PageLoader />}>
                        <AdminDashboardPage />
                    </Suspense>
                ),
            },
        ],
    },
    // Movies management - Admin only
    {
        element: (
            <ProtectedRoute allowedRoles={['Admin']}>
                <AdminLayout />
            </ProtectedRoute>
        ),
        children: [
            {
                path: '/admin/movies',
                element: (
                    <Suspense fallback={<PageLoader />}>
                        <AdminMoviesPage />
                    </Suspense>
                ),
            },
        ],
    },
    // Cinemas management - Admin only
    {
        element: (
            <ProtectedRoute allowedRoles={['Admin']}>
                <AdminLayout />
            </ProtectedRoute>
        ),
        children: [
            {
                path: '/admin/cinemas',
                element: (
                    <Suspense fallback={<PageLoader />}>
                        <AdminCinemasPage />
                    </Suspense>
                ),
            },
        ],
    },
    // Showtimes management - Admin/Manager
    {
        element: (
            <ProtectedRoute allowedRoles={['Admin', 'Manager']}>
                <AdminLayout />
            </ProtectedRoute>
        ),
        children: [
            {
                path: '/admin/showtimes',
                element: (
                    <Suspense fallback={<PageLoader />}>
                        <AdminShowtimesPage />
                    </Suspense>
                ),
            },
        ],
    },
    // Bookings management - Admin/Manager
    {
        element: (
            <ProtectedRoute allowedRoles={['Admin', 'Manager']}>
                <AdminLayout />
            </ProtectedRoute>
        ),
        children: [
            {
                path: '/admin/bookings',
                element: (
                    <Suspense fallback={<PageLoader />}>
                        <AdminBookingsPage />
                    </Suspense>
                ),
            },
        ],
    },
    // Users management - Admin only
    {
        element: (
            <ProtectedRoute allowedRoles={['Admin']}>
                <AdminLayout />
            </ProtectedRoute>
        ),
        children: [
            {
                path: '/admin/users',
                element: (
                    <Suspense fallback={<PageLoader />}>
                        <AdminUsersPage />
                    </Suspense>
                ),
            },
        ],
    },

    // App routes (Home, Movies, Booking, Profile)
    {
        element: <AppLayout />,
        children: [
            {
                path: '/',
                element: (
                    <Suspense fallback={<PageLoader />}>
                        <HomePage />
                    </Suspense>
                ),
            },
            {
                path: '/movies',
                element: (
                    <Suspense fallback={<PageLoader />}>
                        <MovieListPage />
                    </Suspense>
                ),
            },
            {
                path: '/movies/:id',
                element: (
                    <Suspense fallback={<PageLoader />}>
                        <MovieDetailPage />
                    </Suspense>
                ),
            },
            {
                path: '/cinemas',
                element: (
                    <Suspense fallback={<PageLoader />}>
                        <CinemaListPage />
                    </Suspense>
                ),
            },
            {
                path: '/booking',
                element: (
                    <ProtectedRoute>
                        <Suspense fallback={<PageLoader />}>
                            <BookingPage />
                        </Suspense>
                    </ProtectedRoute>
                ),
            },
            {
                path: '/booking/success',
                element: (
                    <ProtectedRoute>
                        <Suspense fallback={<PageLoader />}>
                            <BookingSuccessPage />
                        </Suspense>
                    </ProtectedRoute>
                ),
            },
            {
                path: '/booking/failed',
                element: (
                    <ProtectedRoute>
                        <Suspense fallback={<PageLoader />}>
                            <BookingFailedPage />
                        </Suspense>
                    </ProtectedRoute>
                ),
            },
            {
                path: '/profile',
                element: (
                    <ProtectedRoute>
                        <Suspense fallback={<PageLoader />}>
                            <ProfilePage />
                        </Suspense>
                    </ProtectedRoute>
                ),
            },
            {
                path: '/profile/history',
                element: (
                    <ProtectedRoute>
                        <Suspense fallback={<PageLoader />}>
                            <BookingHistoryPage />
                        </Suspense>
                    </ProtectedRoute>
                ),
            },
            {
                path: '/profile/booking/:id',
                element: (
                    <ProtectedRoute>
                        <Suspense fallback={<PageLoader />}>
                            <BookingDetailPage />
                        </Suspense>
                    </ProtectedRoute>
                ),
            },
        ],
    },

    // Error pages
    {
        path: '/403',
        element: (
            <Suspense fallback={<PageLoader />}>
                <Error403Page />
            </Suspense>
        ),
    },
    {
        path: '*',
        element: (
            <Suspense fallback={<PageLoader />}>
                <Error404Page />
            </Suspense>
        ),
    },
]);

export default router;
