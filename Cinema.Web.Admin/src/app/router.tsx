import { Navigate, createBrowserRouter } from 'react-router-dom';
import { ProtectedRoute } from '../shared/auth/ProtectedRoute';
import AdminLayout from '../layouts/AdminLayout';
import LoginPage from '../pages/auth/LoginPage';
import UnauthorizedPage from '../pages/auth/UnauthorizedPage';
import DashboardPage from '../pages/dashboard/DashboardPage';
import MoviesPage from '../pages/movies/MoviesPage';
import CinemasPage from '../pages/cinemas/CinemasPage';
import ShowtimesPage from '../pages/showtimes/ShowtimesPage';
import TicketsPage from '../pages/tickets/TicketsPage';

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/unauthorized',
    element: <UnauthorizedPage />,
  },
  {
    element: <ProtectedRoute roles={['Admin']} />,
    children: [
      {
        path: '/',
        element: <AdminLayout />,
        children: [
          { index: true, element: <DashboardPage /> },
          { path: 'movies', element: <MoviesPage /> },
          { path: 'cinemas', element: <CinemasPage /> },
          { path: 'showtimes', element: <ShowtimesPage /> },
          { path: 'tickets', element: <TicketsPage /> },
        ],
      },
    ],
  },
  {
    path: '*',
    element: <Navigate to="/" replace />,
  },
]);
