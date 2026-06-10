import { Navigate, createBrowserRouter } from 'react-router-dom';
import { ProtectedRoute } from '../shared/auth/ProtectedRoute';
import ClientLayout from '../layouts/ClientLayout';
import LoginPage from '../pages/auth/LoginPage';
import RegisterPage from '../pages/auth/RegisterPage';
import UnauthorizedPage from '../pages/auth/UnauthorizedPage';
import BookingStatusPage from '../pages/booking/BookingStatusPage';
import BookingSuccessPage from '../pages/booking/BookingSuccessPage';
import CheckoutPage from '../pages/booking/CheckoutPage';
import SeatSelectionPage from '../pages/booking/SeatSelectionPage';
import HomePage from '../pages/home/HomePage';
import MovieDetailPage from '../pages/movies/MovieDetailPage';
import MoviesPage from '../pages/movies/MoviesPage';
import PaymentCancelPage from '../pages/payment/PaymentCancelPage';
import PaymentErrorPage from '../pages/payment/PaymentErrorPage';
import PaymentSuccessPage from '../pages/payment/PaymentSuccessPage';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <ClientLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: 'movies', element: <MoviesPage /> },
      { path: 'movies/:movieId', element: <MovieDetailPage /> },
      { path: 'payment/success', element: <PaymentSuccessPage /> },
      { path: 'payment/cancel', element: <PaymentCancelPage /> },
      { path: 'payment/error', element: <PaymentErrorPage /> },
    ],
  },
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/register',
    element: <RegisterPage />,
  },
  {
    path: '/unauthorized',
    element: <UnauthorizedPage />,
  },
  {
    element: <ProtectedRoute roles={['Customer', 'Admin']} />,
    children: [
      {
        path: '/account',
        element: <ClientLayout />,
        children: [
          { index: true, element: <div className="page-panel">Account area is ready for customer pages.</div> },
        ],
      },
      {
        path: '/booking/:showtimeId',
        element: <ClientLayout />,
        children: [{ index: true, element: <SeatSelectionPage /> }],
      },
      {
        path: '/checkout',
        element: <ClientLayout />,
        children: [{ index: true, element: <CheckoutPage /> }],
      },
      {
        path: '/booking-status/:bookingId',
        element: <ClientLayout />,
        children: [{ index: true, element: <BookingStatusPage /> }],
      },
      {
        path: '/success/:bookingId',
        element: <ClientLayout />,
        children: [{ index: true, element: <BookingSuccessPage /> }],
      },
    ],
  },
  {
    path: '*',
    element: <Navigate to="/" replace />,
  },
]);
