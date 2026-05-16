import type { RouteObject } from 'react-router-dom';
import AdminLayout from '../layouts/AdminLayout';
import { lazy, Suspense } from 'react';
import { Spin } from 'antd';
import { ProtectedRoute } from '../components/auth/ProtectedRoute';

const Dashboard = lazy(() => import('../pages/admin/Dashboard'));
const MoviesPage = lazy(() => import('../pages/admin/Movies/MoviesPage'));
const CinemaManagementPage = lazy(() => import('../pages/admin/Cinemas/CinemaManagementPage'));
const CinemaHallManagementPage = lazy(() => import('../pages/admin/Cinemas/CinemaHallManagementPage'));
const SeatMapEditorPage = lazy(() => import('../pages/admin/Cinemas/SeatMapEditorPage'));
const ShowtimesPage = lazy(() => import('../pages/admin/Showtimes/ShowtimesPage'));
const TicketOperationsPage = lazy(() => import('../pages/admin/Tickets/TicketOperationsPage'));

const Fallback = <Spin size="large" className="global-spinner" />;

const adminRoutes: RouteObject[] = [
  {
    path: '/admin',
    element: <ProtectedRoute allowedRoles={['Admin']} />,
    children: [
      {
        path: '',
        element: <AdminLayout />,
        children: [
          { index: true, element: <Suspense fallback={Fallback}><Dashboard /></Suspense> },
          { path: 'movies', element: <Suspense fallback={Fallback}><MoviesPage /></Suspense> },
          { path: 'cinemas', element: <Suspense fallback={Fallback}><CinemaManagementPage /></Suspense> },
          { path: 'cinemas/:cinemaId/halls', element: <Suspense fallback={Fallback}><CinemaHallManagementPage /></Suspense> },
          { path: 'halls/:hallId/seats', element: <Suspense fallback={Fallback}><SeatMapEditorPage /></Suspense> },
          { path: 'showtimes', element: <Suspense fallback={Fallback}><ShowtimesPage /></Suspense> },
          { path: 'tickets', element: <Suspense fallback={Fallback}><TicketOperationsPage /></Suspense> },
        ],
      },
    ],
  },
];

export default adminRoutes;
