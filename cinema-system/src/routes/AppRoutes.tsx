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

import ForbiddenError from '../components/errors/ForbiddenError';
import EmployeeDashboard from '../pages/dashboard/employee/Dashboard';
import EmployeeNewSale from '../pages/dashboard/employee/NewSale';
import EmployeeSalesHistory from '../pages/dashboard/employee/SalesHistory';
import EmployeeCardPayment from '../pages/dashboard/employee/CardPayment';
import EmployeeCheckin from '../pages/dashboard/employee/Checkin';
import EmployeeCashPayment from '../pages/dashboard/employee/CashPayment';
import EmployeeSchedule from '../components/concession/ScheduleItem';
import EmployeeEquipmentReport from '../pages/dashboard/employee/EquipmentReport';
import ManagerDashboard from '../pages/dashboard/manager/Dashboard';
import ManagerCreateShowtime from '../pages/dashboard/manager/CreateShowtime';
import ManagerDailyRevenue from '../pages/dashboard/manager/DailyRevenue';
import ManagerEquipmentList from '../pages/dashboard/manager/EquipmentList';
import ManagerInventoryList from '../pages/dashboard/manager/InventoryList';
import ManagerMaintenanceHistory from '../pages/dashboard/manager/MaintenanceHistory';
import ManagerMaintenanceSchedule from '../pages/dashboard/manager/MaintenanceSchedule';
import ManagerManageShowtimes from '../pages/dashboard/manager/ManageShowtimes';
import ManagerMonthlyReport from '../pages/dashboard/manager/MonthlyReport';
import ManagerMoviePerformance from '../pages/dashboard/manager/MoviePerformance';
import ManagerRestock from '../pages/dashboard/manager/Restock';
import ManagerShiftAssignment from '../pages/dashboard/manager/ShiftAssignment';
import ManagerShowtimePricing from '../pages/dashboard/manager/ShowtimePricing';
import ManagerStaffList from '../pages/dashboard/manager/StaffList';
import ManagerStaffPerformance from '../pages/dashboard/manager/StaffPerformance';
import ManagerStockControl from '../pages/dashboard/manager/StockControl';
import AdminDashboard from '../pages/dashboard/admin/Dashboard';
import AdminCinemaList from '../pages/dashboard/admin/CinemaList';
import AdminCinemaCreate from '../pages/dashboard/admin/CinemaCreate';
import AdminCinemaSettings from '../pages/dashboard/admin/CinemaSettings';
import AdminScreenManagement from '../pages/dashboard/admin/ScreenManagement';
import AdminSeatLayout from '../pages/dashboard/admin/SeatLayout';
import AdminMovieList from '../pages/dashboard/admin/MovieList';
import AdminSeatTypes from '../pages/dashboard/admin/SeatTypes';
import AdminMovieCreate from '../pages/dashboard/admin/MovieCreate';
import AdminMovieGenres from '../pages/dashboard/admin/MovieGenres';
import AdminMovieCastCrew from '../pages/dashboard/admin/MovieCastCrew';
import AdminPricingTiers from '../pages/dashboard/admin/PricingTiers';
import AdminTimeSlots from '../pages/dashboard/admin/TimeSlots';
import AdminCinemaPerformance from '../pages/dashboard/admin/CinemaPerformance';
import AdminGeneralSettings from '../pages/dashboard/admin/GeneralSettings';
import AdminOverallRevenue from '../pages/dashboard/admin/OverallRevenue';
import AdminSystemAnalytics from '../pages/dashboard/admin/SystemAnalytics';
import AdminSystemBackup from '../pages/dashboard/admin/SystemBackup';
import AdminUserRoles from '../pages/dashboard/admin/UserRoles';
import DashboardLayout from '../pages/dashboard';
import AdminSeatPricing from '../pages/dashboard/admin/SeatPricing';
export const AppRoutes: React.FC = () => {
  return (
    <Routes>
      {/* Public routes */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />

      {/* User */}
      <Route path='/' element={<MainLayout><HomePage /></MainLayout>} />
      <Route path='/movies' element={<MainLayout><MoviesPage /></MainLayout>} />
      <Route path='/movies/detail/:id' element={<MainLayout><MovieDetailsPage /></MainLayout>} />
      <Route path='/cinemas' element={<MainLayout><CinemaPage /></MainLayout>} />
      <Route path='/cinemas/detail/:id' element={<MainLayout><CinemaDetailsPage /></MainLayout>} />
      <Route path='/seating-plan/:id' element={<SeatSelection />} />
      <Route path='/contact' element={<ContactPage />} />
      <Route path='/profile' element={<ProtectedRoute allowedRoles={['user', 'employee', 'manager', 'admin']}><Profile /></ProtectedRoute>} />
      <Route path='/payment/success' element={<PaymentSuccess />} />

      {/* Employee Dashboard Routes */}
      <Route path="/dashboard/employee" element={
        <ProtectedRoute allowedRoles={['employee', 'manager', 'admin']}>
          <DashboardLayout />
        </ProtectedRoute>
      }>
        <Route index element={<EmployeeDashboard />} />
        <Route path="new-sale" element={
          <ProtectedRoute allowedRoles={['employee', 'manager', 'admin']}>
            <EmployeeNewSale />
          </ProtectedRoute>
        } />
        <Route path="sales-history" element={
          <ProtectedRoute allowedRoles={['employee', 'manager', 'admin']}>
            <EmployeeSalesHistory />
          </ProtectedRoute>
        } />
        <Route path="cash-payment" element={
          <ProtectedRoute allowedRoles={['employee', 'manager', 'admin']}>
            <EmployeeCashPayment />
          </ProtectedRoute>
        } />
        <Route path="card-payment" element={
          <ProtectedRoute allowedRoles={['employee', 'manager', 'admin']}>
            <EmployeeCardPayment />
          </ProtectedRoute>
        } />
        <Route path="checkin" element={
          <ProtectedRoute allowedRoles={['employee', 'manager', 'admin']}>
            <EmployeeCheckin />
          </ProtectedRoute>
        } />
        <Route path="schedule" element={
          <ProtectedRoute allowedRoles={['employee', 'manager', 'admin']}>
            <EmployeeSchedule />
          </ProtectedRoute>
        } />
        <Route path="equipment-report" element={
          <ProtectedRoute allowedRoles={['employee', 'manager', 'admin']}>
            <EmployeeEquipmentReport />
          </ProtectedRoute>
        } />
      </Route>

      {/* Manager Dashboard Routes */}
      <Route path="/dashboard/manager" element={
        <ProtectedRoute allowedRoles={['manager', 'admin']}>
          <DashboardLayout />
        </ProtectedRoute>
      }>
        <Route index element={<ManagerDashboard />} />
        <Route path="create-showtime" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerCreateShowtime />
          </ProtectedRoute>
        } />
        <Route path="manage-showtimes" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerManageShowtimes />
          </ProtectedRoute>
        } />
        <Route path="showtime-pricing" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerShowtimePricing />
          </ProtectedRoute>
        } />
        <Route path="staff-list" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerStaffList />
          </ProtectedRoute>
        } />
        <Route path="shift-assignment" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerShiftAssignment />
          </ProtectedRoute>
        } />
        <Route path="staff-performance" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerStaffPerformance />
          </ProtectedRoute>
        } />
        <Route path="inventory-list" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerInventoryList />
          </ProtectedRoute>
        } />
        <Route path="stock-control" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerStockControl />
          </ProtectedRoute>
        } />
        <Route path="restock" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerRestock />
          </ProtectedRoute>
        } />
        <Route path="equipment-list" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerEquipmentList />
          </ProtectedRoute>
        } />
        <Route path="maintenance-schedule" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerMaintenanceSchedule />
          </ProtectedRoute>
        } />
        <Route path="maintenance-history" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerMaintenanceHistory />
          </ProtectedRoute>
        } />
        <Route path="daily-revenue" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerDailyRevenue />
          </ProtectedRoute>
        } />
        <Route path="monthly-report" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerMonthlyReport />
          </ProtectedRoute>
        } />
        <Route path="movie-performance" element={
          <ProtectedRoute allowedRoles={['manager', 'admin']}>
            <ManagerMoviePerformance />
          </ProtectedRoute>
        } />
      </Route>

      {/* Admin Dashboard Routes */}
      <Route path="/dashboard/admin" element={
        <ProtectedRoute allowedRoles={['admin']}>
          <DashboardLayout />
        </ProtectedRoute>
      }>
        <Route index element={<AdminDashboard />} />
        <Route path="cinema-list" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminCinemaList />
          </ProtectedRoute>
        } />
        <Route path="cinema-create" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminCinemaCreate />
          </ProtectedRoute>
        } />
        <Route path="cinema-settings" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminCinemaSettings />
          </ProtectedRoute>
        } />
        <Route path="screen-management" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminScreenManagement />
          </ProtectedRoute>
        } />
        <Route path="seat-layout" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminSeatLayout />
          </ProtectedRoute>
        } />
        <Route path="seat-types" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminSeatTypes />
          </ProtectedRoute>
        } />
        <Route path="movie-list" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminMovieList />
          </ProtectedRoute>
        } />
        <Route path="movie-create" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminMovieCreate />
          </ProtectedRoute>
        } />
        <Route path="movie-genres" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminMovieGenres />
          </ProtectedRoute>
        } />
        <Route path="movie-cast-crew" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminMovieCastCrew />
          </ProtectedRoute>
        } />
        <Route path="pricing-tiers" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminPricingTiers />
          </ProtectedRoute>
        } />
        <Route path="seat-pricing" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminSeatPricing />
          </ProtectedRoute>
        } />
        <Route path="time-slots" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminTimeSlots />
          </ProtectedRoute>
        } />
        <Route path="overall-revenue" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminOverallRevenue />
          </ProtectedRoute>
        } />
        <Route path="cinema-performance" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminCinemaPerformance />
          </ProtectedRoute>
        } />
        <Route path="system-analytics" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminSystemAnalytics />
          </ProtectedRoute>
        } />
        <Route path="general-settings" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminGeneralSettings />
          </ProtectedRoute>
        } />
        <Route path="user-roles" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminUserRoles />
          </ProtectedRoute>
        } />
        <Route path="system-backup" element={
          <ProtectedRoute allowedRoles={['admin']}>
            <AdminSystemBackup />
          </ProtectedRoute>
        } />
      </Route>
      {/* Redirect */}
      <Route path="*" element={<Navigate to="/" replace />} />
      {/* Redirect to Error page */}
      <Route path="/unauthorized" element={<UnauthorizedPage />} />
      <Route path="/forbidden" element={<ForbiddenError />} />
    </Routes>
  );
};
