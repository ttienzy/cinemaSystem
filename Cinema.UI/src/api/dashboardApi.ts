// ============================================================
// Dashboard API — Contract cho Admin Command Center
// ============================================================
// Endpoints: Booking.API → /api/bookings/dashboard/*
// SignalR Hub: Booking.API → /hubs/admin-dashboard
// ============================================================

import axiosClient from './axiosClient';
import type { ApiResponse } from '../types/api';

// ---- KPI ----

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
  occupancyRate: number;       // 0-100
  todayShowtimesCount: number;
  showingMoviesCount: number;
  hotMovie: DashboardHotMovie | null;
  generatedAtUtc: string;
  utcOffsetMinutes: number;
}

// ---- Revenue Chart ----

export interface RevenuePoint {
  date: string;
  label: string;
  revenue: number;
  ticketsSold: number;
  bookingsCount: number;
}

export interface RevenueChart {
  weekly: RevenuePoint[];
  monthly: RevenuePoint[];
}

// ---- Top Movies ----

export interface TopMovie {
  movieId: string;
  title: string;
  posterUrl: string | null;
  rank: number;
  bookingsCount: number;
  ticketsSold: number;
  revenue: number;
  occupancyRate: number;
  trendDirection: 'up' | 'down' | 'stable';
  lastBookingAtUtc: string | null;
}

// ---- Recent Activity ----

export interface RecentActivity {
  bookingId: string;
  showtimeId: string;
  movieId: string;
  movieTitle: string;
  customerName: string;
  amount: number;
  seatsCount: number;
  status: string;
  occurredAtUtc: string;
}

// ---- Full Summary ----

export interface DashboardSummary {
  kpi: DashboardKpiSnapshot;
  revenueChart: RevenueChart;
  topMovies: TopMovie[];
  recentActivities: RecentActivity[];
  generatedAtUtc: string;
  utcOffsetMinutes: number;
}

// ---- SignalR NewBooking payload (same as RecentActivity) ----
export type NewBookingPayload = RecentActivity;

// ---- API Calls ----

const UTC_OFFSET = -(new Date().getTimezoneOffset()); // Vietnam = 420

export const dashboardApi = {
  /**
   * Full dashboard summary — gọi 1 lần khi mount.
   * GET /api/bookings/dashboard/summary?utcOffsetMinutes=420
   */
  getSummary: async (): Promise<ApiResponse<DashboardSummary>> => {
    return axiosClient.get('/api/bookings/dashboard/summary', {
      params: { utcOffsetMinutes: UTC_OFFSET },
    });
  },

  /**
   * Lightweight KPI snapshot — polling mỗi 60s.
   * GET /api/bookings/dashboard/kpi-snapshot?utcOffsetMinutes=420
   */
  getKpiSnapshot: async (): Promise<ApiResponse<DashboardKpiSnapshot>> => {
    return axiosClient.get('/api/bookings/dashboard/kpi-snapshot', {
      params: { utcOffsetMinutes: UTC_OFFSET },
    });
  },
};
