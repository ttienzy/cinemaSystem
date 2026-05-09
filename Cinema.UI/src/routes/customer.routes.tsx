import type { RouteObject } from 'react-router-dom';
import ClientLayout from '../layouts/ClientLayout';
import { lazy, Suspense } from 'react';
import { Spin } from 'antd';
import { ProtectedRoute } from '../components/auth/ProtectedRoute';
import { PublicRoute } from '../components/auth/PublicRoute';

const Home = lazy(() => import('../pages/customer/Home'));
const MovieDetails = lazy(() => import('../pages/customer/MovieDetails'));
const SeatSelectionPage = lazy(() => import('../pages/customer/Booking/SeatSelectionPage'));
const CheckoutPage = lazy(() => import('../pages/customer/Booking/CheckoutPage'));
const SuccessPage = lazy(() => import('../pages/customer/Booking/SuccessPage'));
const PaymentSuccessPage = lazy(() => import('../pages/customer/Payment/PaymentSuccessPage'));
const PaymentErrorPage = lazy(() => import('../pages/customer/Payment/PaymentErrorPage'));
const PaymentCancelPage = lazy(() => import('../pages/customer/Payment/PaymentCancelPage'));
const Login = lazy(() => import('../pages/auth/Login'));
const Register = lazy(() => import('../pages/auth/Register'));
const Unauthorized = lazy(() => import('../pages/auth/Unauthorized'));

const customerRoutes: RouteObject[] = [
  {
    path: '/',
    element: <ClientLayout />,
    children: [
      {
        index: true,
        element: (
          <Suspense fallback={<Spin size="large" className="global-spinner" />}>
            <Home />
          </Suspense>
        ),
      },
      {
        path: 'movies/:id',
        element: (
          <Suspense fallback={<Spin size="large" className="global-spinner" />}>
            <MovieDetails />
          </Suspense>
        ),
      },
      // Protected routes - Yêu cầu đăng nhập
      {
        element: <ProtectedRoute />,
        children: [
          {
            path: 'booking/:showtimeId',
            element: (
              <Suspense fallback={<Spin size="large" className="global-spinner" />}>
                <SeatSelectionPage />
              </Suspense>
            ),
          },
          {
            path: 'checkout',
            element: (
              <Suspense fallback={<Spin size="large" className="global-spinner" />}>
                <CheckoutPage />
              </Suspense>
            ),
          },
          {
            path: 'success/:bookingId',
            element: (
              <Suspense fallback={<Spin size="large" className="global-spinner" />}>
                <SuccessPage />
              </Suspense>
            ),
          },
        ],
      },
      // Payment callback routes - Public (SePay redirects here)
      {
        path: 'payment/success',
        element: (
          <Suspense fallback={<Spin size="large" className="global-spinner" />}>
            <PaymentSuccessPage />
          </Suspense>
        ),
      },
      {
        path: 'payment/error',
        element: (
          <Suspense fallback={<Spin size="large" className="global-spinner" />}>
            <PaymentErrorPage />
          </Suspense>
        ),
      },
      {
        path: 'payment/cancel',
        element: (
          <Suspense fallback={<Spin size="large" className="global-spinner" />}>
            <PaymentCancelPage />
          </Suspense>
        ),
      },
    ],
  },
  {
    path: '/login',
    element: (
      <PublicRoute>
        <Suspense fallback={<Spin size="large" className="global-spinner" />}>
          <Login />
        </Suspense>
      </PublicRoute>
    ),
  },
  {
    path: '/register',
    element: (
      <PublicRoute>
        <Suspense fallback={<Spin size="large" className="global-spinner" />}>
          <Register />
        </Suspense>
      </PublicRoute>
    ),
  },
  {
    path: '/unauthorized',
    element: (
      <Suspense fallback={<Spin size="large" className="global-spinner" />}>
        <Unauthorized />
      </Suspense>
    ),
  },
];

export default customerRoutes;
