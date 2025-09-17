import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';
import { LoginPage } from '../pages/auth/LoginPage';
import { RegisterPage } from '../pages/auth/RegisterPage';
import { UnauthorizedPage } from '../pages/auth/UnauthorizedPage';
import HomePage from '../pages/home/HomePage';
import MainLayout from '../components/layouts/MainLayout';
import Profile from '../pages/home/Profile';
import SeatSelection from '../pages/seating_plan/SeatSelection';
import PaymentSuccess from '../pages/payment/PaymentSuccess';
import MovieDetailsPage from '../pages/movie/MovieDetailsPage';
import CinemaDetailsPage from '../pages/cinema/CinemaDetailsPage';
import MoviesPage from '../pages/movie/MoviesPage';
import CinemaPage from '../pages/cinema/CinemaPage';
import ContactPage from '../pages/home/ContactPage';

import Dashboard from '../pages/dashboard';
export const AppRoutes: React.FC = () => {
  return (
    <Routes>
      {/* Public routes */}
      <Route path="/login" element={<LoginPage />} />
      {/* <Route path="/register" element={<RegisterPage />} />
      <Route path="/unauthorized" element={<UnauthorizedPage />} />
      <Route path='/' element={<MainLayout><HomePage /></MainLayout>} />
      <Route path='/profile' element={<Profile />} />
      <Route path='/seating-plan/:id' element={<SeatSelection />} />
      <Route path='/payment/success' element={<PaymentSuccess />} />
      <Route path='/movies/detail/:id' element={<MovieDetailsPage />} />
      <Route path='/movies' element={<MoviesPage />} />
      <Route path='/cinemas' element={<CinemaPage />} />
      <Route path='/cinemas/detail/:id' element={<CinemaDetailsPage />} /> */}
      <Route path='/' element={<Dashboard />} />
      <Route path='/contact' element={<ContactPage />} />
      {/* Redirect */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};
