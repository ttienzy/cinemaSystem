// Admin Dashboard Types - Based on Backend API

export interface DashboardSummary {
    totalRevenue: number;
    totalBookings: number;
    totalTickets: number;
    averageOccupancyRate: number;
    monthlyRevenue: number;
    monthlyBookings: number;
    todayRevenue: number;
    todayBookings: number;
}

export interface RevenueReport {
    period: string;
    revenue: number;
    bookings: number;
    tickets: number;
}

export interface TopMovie {
    movieId: string;
    movieTitle: string;
    posterUrl?: string;
    totalRevenue: number;
    totalBookings: number;
    totalTickets: number;
}

export interface DashboardStatsParams {
    cinemaId?: string;
}

export interface RevenueReportParams {
    from: string;
    to: string;
    cinemaId?: string;
    groupBy: 'day' | 'week' | 'month';
}

export interface TopMoviesParams {
    limit?: number;
    from?: string;
    to?: string;
    cinemaId?: string;
}
