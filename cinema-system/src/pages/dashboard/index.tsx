// src/pages/dashboard/index.tsx
import React, { useState } from 'react';
import Sidebar from '../../components/layouts/Sidebar';
import Header from '../../components/layouts/HeaderMg';
// Import all content components
import EmployeeDashboard from './employee/Dashboard';
import EmployeeNewSale from './employee/NewSale';
import EmployeeSalesHistory from './employee/SalesHistory';
import EmployeeCashPayment from './employee/CashPayment';
import EmployeeCardPayment from './employee/CardPayment';
import EmployeeCheckin from './employee/Checkin';
import EmployeeSchedule from './employee/Schedule';
import EmployeeEquipmentReport from './employee/EquipmentReport';

import ManagerDashboard from './manager/Dashboard';
import ManagerCreateShowtime from './manager/CreateShowtime';
import ManagerManageShowtimes from './manager/ManageShowtimes';
import ManagerShowtimePricing from './manager/ShowtimePricing';
import ManagerStaffList from './manager/StaffList';
import ManagerShiftAssignment from './manager/ShiftAssignment';
import ManagerStaffPerformance from './manager/StaffPerformance';
import ManagerInventoryList from './manager/InventoryList';
import ManagerStockControl from './manager/StockControl';
import ManagerRestock from './manager/Restock';
import ManagerEquipmentList from './manager/EquipmentList';
import ManagerMaintenanceSchedule from './manager/MaintenanceSchedule';
import ManagerMaintenanceHistory from './manager/MaintenanceHistory';
import ManagerDailyRevenue from './manager/DailyRevenue';
import ManagerMonthlyReport from './manager/MonthlyReport';
import ManagerMoviePerformance from './manager/MoviePerformance';

import AdminDashboard from './admin/Dashboard';
import AdminCinemaList from './admin/CinemaList';
import AdminCinemaCreate from './admin/CinemaCreate';
import AdminCinemaSettings from './admin/CinemaSettings';
import AdminScreenManagement from './admin/ScreenManagement';
import AdminSeatLayout from './admin/SeatLayout';
import AdminSeatTypes from './admin/SeatTypes';
import AdminMovieList from './admin/MovieList';
import AdminMovieCreate from './admin/MovieCreate';
import AdminMovieGenres from './admin/MovieGenres';
import AdminMovieCastCrew from './admin/MovieCastCrew';
import AdminPricingTiers from './admin/PricingTiers';
import AdminSeatPricing from './admin/SeatPricing';
import AdminTimeSlots from './admin/TimeSlots';
import AdminOverallRevenue from './admin/OverallRevenue';
import AdminCinemaPerformance from './admin/CinemaPerformance';
import AdminSystemAnalytics from './admin/SystemAnalytics';
import AdminGeneralSettings from './admin/GeneralSettings';
import AdminUserRoles from './admin/UserRoles';
import AdminSystemBackup from './admin/SystemBackup';
import { menuItems } from '../../types/dashboard.types';



const Dashboard = () => {
    const [activeRole, setActiveRole] = useState('employee');
    const [activeItem, setActiveItem] = useState('dashboard');
    const [expandedSections, setExpandedSections] = useState<Record<string, boolean>>({});

    const toggleSection = (sectionId: string) => {
        setExpandedSections(prev => ({
            ...prev,
            [sectionId]: !prev[sectionId]
        }));
    };

    const getContentComponent = () => {
        if (activeRole === 'employee') {
            switch (activeItem) {
                case 'dashboard': return <EmployeeDashboard />;
                case 'new-sale': return <EmployeeNewSale />;
                case 'sales-history': return <EmployeeSalesHistory />;
                case 'cash-payment': return <EmployeeCashPayment />;
                case 'card-payment': return <EmployeeCardPayment />;
                case 'checkin': return <EmployeeCheckin />;
                case 'schedule': return <EmployeeSchedule />;
                case 'equipment-report': return <EmployeeEquipmentReport />;
                default: return <div>Nội dung không tìm thấy</div>;
            }
        } else if (activeRole === 'manager') {
            switch (activeItem) {
                case 'dashboard': return <ManagerDashboard />;
                case 'create-showtime': return <ManagerCreateShowtime />;
                case 'manage-showtimes': return <ManagerManageShowtimes />;
                case 'showtime-pricing': return <ManagerShowtimePricing />;
                case 'staff-list': return <ManagerStaffList />;
                case 'shift-assignment': return <ManagerShiftAssignment />;
                case 'staff-performance': return <ManagerStaffPerformance />;
                case 'inventory-list': return <ManagerInventoryList />;
                case 'stock-control': return <ManagerStockControl />;
                case 'restock': return <ManagerRestock />;
                case 'equipment-list': return <ManagerEquipmentList />;
                case 'maintenance-schedule': return <ManagerMaintenanceSchedule />;
                case 'maintenance-history': return <ManagerMaintenanceHistory />;
                case 'daily-revenue': return <ManagerDailyRevenue />;
                case 'monthly-report': return <ManagerMonthlyReport />;
                case 'movie-performance': return <ManagerMoviePerformance />;
                default: return <div>Nội dung không tìm thấy</div>;
            }
        } else if (activeRole === 'admin') {
            switch (activeItem) {
                case 'dashboard': return <AdminDashboard />;
                case 'cinema-list': return <AdminCinemaList />;
                case 'cinema-create': return <AdminCinemaCreate />;
                case 'cinema-settings': return <AdminCinemaSettings />;
                case 'screen-management': return <AdminScreenManagement />;
                case 'seat-layout': return <AdminSeatLayout />;
                case 'seat-types': return <AdminSeatTypes />;
                case 'movie-list': return <AdminMovieList />;
                case 'movie-create': return <AdminMovieCreate />;
                case 'movie-genres': return <AdminMovieGenres />;
                case 'movie-cast-crew': return <AdminMovieCastCrew />;
                case 'pricing-tiers': return <AdminPricingTiers />;
                case 'seat-pricing': return <AdminSeatPricing />;
                case 'time-slots': return <AdminTimeSlots />;
                case 'overall-revenue': return <AdminOverallRevenue />;
                case 'cinema-performance': return <AdminCinemaPerformance />;
                case 'system-analytics': return <AdminSystemAnalytics />;
                case 'general-settings': return <AdminGeneralSettings />;
                case 'user-roles': return <AdminUserRoles />;
                case 'system-backup': return <AdminSystemBackup />;
                default: return <div>Nội dung không tìm thấy</div>;
            }
        }
        return <div>Nội dung không tìm thấy</div>;
    };

    return (
        <div className="flex h-screen bg-gray-100">
            <Sidebar
                activeRole={activeRole}
                activeItem={activeItem}
                expandedSections={expandedSections}
                setActiveItem={setActiveItem}
                toggleSection={toggleSection}
                setActiveRole={setActiveRole}
                setExpandedSections={setExpandedSections}
            />
            <div className="flex-1 flex flex-col">
                <Header activeRole={activeRole} activeItem={activeItem} menuItems={menuItems} />
                {/* Chỗ này thêm overflow-y-auto */}
                <div className="flex-1 p-6 bg-gray-50 overflow-y-auto">
                    {getContentComponent()}
                </div>
            </div>
        </div>

    );
};

export default Dashboard;