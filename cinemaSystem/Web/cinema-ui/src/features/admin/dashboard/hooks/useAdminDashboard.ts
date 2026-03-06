// Admin Dashboard Hooks - React Query hooks for admin dashboard
import { useQuery } from '@tanstack/react-query';
import { adminDashboardApi } from '../api/adminDashboardApi';

// Query keys
const adminDashboardKeys = {
    all: ['adminDashboard'] as const,
    stats: (cinemaId?: string) => [...adminDashboardKeys.all, 'stats', cinemaId] as const,
    revenue: (from: string, to: string, cinemaId?: string, groupBy?: string) =>
        [...adminDashboardKeys.all, 'revenue', from, to, cinemaId, groupBy] as const,
    topMovies: (limit?: number, from?: string, to?: string) =>
        [...adminDashboardKeys.all, 'topMovies', limit, from, to] as const,
};

export { adminDashboardKeys };

// Hook: Get dashboard statistics
export const useAdminDashboardStats = (cinemaId?: string) => {
    return useQuery({
        queryKey: adminDashboardKeys.stats(cinemaId),
        queryFn: () => adminDashboardApi.getStats(cinemaId),
        staleTime: 60 * 1000, // 1 minute
    });
};

// Hook: Get revenue report
export const useAdminRevenueReport = (
    from: string,
    to: string,
    cinemaId?: string,
    groupBy = 'day'
) => {
    return useQuery({
        queryKey: adminDashboardKeys.revenue(from, to, cinemaId, groupBy),
        queryFn: () => adminDashboardApi.getRevenueReport(from, to, cinemaId, groupBy),
        enabled: !!from && !!to,
        staleTime: 60 * 1000,
    });
};

// Hook: Get top movies
export const useAdminTopMovies = (limit = 10, from?: string, to?: string) => {
    return useQuery({
        queryKey: adminDashboardKeys.topMovies(limit, from, to),
        queryFn: () => adminDashboardApi.getTopMovies(limit, from, to),
        staleTime: 60 * 1000,
    });
};

export default {
    useAdminDashboardStats,
    useAdminRevenueReport,
    useAdminTopMovies,
};
