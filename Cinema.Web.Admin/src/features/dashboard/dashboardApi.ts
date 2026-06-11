import axiosClient from '../../shared/api/axiosClient';
import type { ApiResponse } from '../../shared/types/api';
import { getUtcOffsetMinutes } from '../../shared/utils/format';

export interface DashboardHotMovie {
  movieId: string;
  title: string;
  posterUrl: string | null;
  bookingsCount: number;
  ticketsSold: number;
  revenue: number;
}

export interface DashboardKpiSnapshot {
  todayRevenue: number;
  todayTicketsSold: number;
  occupancyRate: number;
  todayShowtimesCount: number;
  showingMoviesCount: number;
  hotMovie: DashboardHotMovie | null;
  generatedAtUtc: string;
  utcOffsetMinutes: number;
}

export interface RevenuePoint {
  date: string;
  label: string;
  revenue: number;
  ticketsSold: number;
  bookingsCount: number;
}

export interface DashboardSummary {
  kpi: DashboardKpiSnapshot;
  revenueChart: {
    weekly: RevenuePoint[];
    monthly: RevenuePoint[];
  };
  topMovies: Array<{
    movieId: string;
    title: string;
    posterUrl: string | null;
    rank: number;
    bookingsCount: number;
    ticketsSold: number;
    revenue: number;
    occupancyRate: number;
    trendDirection: string;
    lastBookingAtUtc: string | null;
  }>;
  recentActivities: Array<{
    bookingId: string;
    showtimeId: string;
    movieId: string;
    movieTitle: string;
    customerName: string;
    amount: number;
    seatsCount: number;
    status: string;
    occurredAtUtc: string;
  }>;
  generatedAtUtc: string;
  utcOffsetMinutes: number;
}

export const dashboardApi = {
  getSummary() {
    return axiosClient.get<never, ApiResponse<DashboardSummary>>('/api/v1/bookings/dashboard/summary', {
      params: { utcOffsetMinutes: getUtcOffsetMinutes() },
    });
  },
  getKpiSnapshot() {
    return axiosClient.get<never, ApiResponse<DashboardKpiSnapshot>>('/api/v1/bookings/dashboard/kpi-snapshot', {
      params: { utcOffsetMinutes: getUtcOffsetMinutes() },
    });
  },
};
