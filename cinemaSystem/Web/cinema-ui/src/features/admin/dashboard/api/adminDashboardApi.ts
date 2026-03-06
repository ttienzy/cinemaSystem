// Admin Dashboard API - Based on Backend API endpoints
import axios from '../../../../shared/api/axios.instance';
import { Endpoints } from '../../../../shared/api/endpoints';
import type { DashboardSummary, RevenueReport, TopMovie } from '../types/adminDashboard.types';

export const adminDashboardApi = {
    // Get dashboard statistics
    getStats: async (cinemaId?: string): Promise<DashboardSummary> => {
        const response = await axios.get<DashboardSummary>(Endpoints.ADMIN_DASHBOARD.STATS(cinemaId));
        return response.data;
    },

    // Get revenue report
    getRevenueReport: async (
        from: string,
        to: string,
        cinemaId?: string,
        groupBy = 'day'
    ): Promise<RevenueReport[]> => {
        const response = await axios.get<RevenueReport[]>(
            Endpoints.ADMIN_DASHBOARD.REVENUE(from, to, cinemaId, groupBy)
        );
        return response.data;
    },

    // Get top movies
    getTopMovies: async (
        limit = 10,
        from?: string,
        to?: string
    ): Promise<TopMovie[]> => {
        const response = await axios.get<TopMovie[]>(
            Endpoints.ADMIN_DASHBOARD.TOP_MOVIES(limit, from, to)
        );
        return response.data;
    },
};

export default adminDashboardApi;
