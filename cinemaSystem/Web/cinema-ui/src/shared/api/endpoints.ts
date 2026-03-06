// API Endpoints - Centralized URL constants
// Based on Backend API at https://localhost:7227
const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7227';

export const Endpoints = {
    // ==========================================
    // AUTH (Public)
    // ==========================================
    AUTH: {
        REGISTER: `${API_BASE_URL}/api/auth/register`,
        LOGIN: `${API_BASE_URL}/api/auth/login`,
        REFRESH: `${API_BASE_URL}/api/auth/refresh`,
        LOGOUT: `${API_BASE_URL}/api/auth/logout`,
    },

    // ==========================================
    // IDENTITY (Public + Auth)
    // ==========================================
    IDENTITY: {
        PROFILE: (userId: string) => `${API_BASE_URL}/api/identity/profile/${userId}`,
        CHANGE_PASSWORD: `${API_BASE_URL}/api/identity/change-password`,
        FORGOT_PASSWORD: `${API_BASE_URL}/api/identity/forgot-password-with-otp`,
        VERIFY_RESET_OTP: `${API_BASE_URL}/api/identity/verify-reset-otp`,
        RESET_PASSWORD: `${API_BASE_URL}/api/identity/reset-password-with-otp`,
    },

    // ==========================================
    // PROFILE (Customer - Auth required)
    // ==========================================
    PROFILE: {
        BASE: `${API_BASE_URL}/api/profile`,
        CHANGE_PASSWORD: `${API_BASE_URL}/api/profile/change-password`,
    },

    // ==========================================
    // MOVIES (Public)
    // ==========================================
    MOVIES: {
        BASE: `${API_BASE_URL}/api/movies`,
        NOW_SHOWING: `${API_BASE_URL}/api/movies/now-showing`,
        COMING_SOON: `${API_BASE_URL}/api/movies/coming-soon`,
        DETAIL: (id: string) => `${API_BASE_URL}/api/movies/${id}`,
    },

    // ==========================================
    // ADMIN - MOVIES (Admin only)
    // ==========================================
    ADMIN_MOVIES: {
        CREATE: `${API_BASE_URL}/api/admin/movies`,
        DETAIL: (id: string) => `${API_BASE_URL}/api/admin/movies/${id}`,
    },

    // ==========================================
    // SHOWTIMES (Public)
    // ==========================================
    SHOWTIMES: {
        BY_MOVIE: (movieId: string, date?: string) =>
            `${API_BASE_URL}/api/showtimes/movie/${movieId}${date ? `?date=${date}` : ''}`,
        BY_CINEMA: (cinemaId: string, date?: string) =>
            `${API_BASE_URL}/api/showtimes/cinema/${cinemaId}${date ? `?date=${date}` : ''}`,
        SEATING_PLAN: (showtimeId: string) => `${API_BASE_URL}/api/showtimes/${showtimeId}/seating-plan`,
    },

    // ==========================================
    // MANAGER - SHOWTIMES (Manager only)
    // ==========================================
    MANAGER_SHOWTIMES: {
        BULK: `${API_BASE_URL}/api/manager/showtimes/bulk`,
        DETAIL: (showtimeId: string) => `${API_BASE_URL}/api/manager/showtimes/detail/${showtimeId}`,
        BY_CINEMA: (cinemaId: string, date?: string) =>
            `${API_BASE_URL}/api/manager/showtimes/${cinemaId}${date ? `?date=${date}` : ''}`,
        CONFIRM: (showtimeId: string) => `${API_BASE_URL}/api/manager/showtimes/${showtimeId}/confirm`,
    },

    // ==========================================
    // CINEMAS (Public)
    // ==========================================
    CINEMAS: {
        BASE: `${API_BASE_URL}/api/cinemas`,
        SCREEN_SEAT_BLOCK: (screenId: string, seatId: string) =>
            `${API_BASE_URL}/api/cinemas/screens/${screenId}/seats/${seatId}/block`,
        SCREEN_SEAT_LINK: (screenId: string, seatId: string) =>
            `${API_BASE_URL}/api/cinemas/screens/${screenId}/seats/${seatId}/link`,
        SCREEN_SEAT_UNLINK: (screenId: string, seatId: string) =>
            `${API_BASE_URL}/api/cinemas/screens/${screenId}/seats/${seatId}/unlink`,
    },

    // ==========================================
    // ADMIN - CINEMAS (Admin only)
    // ==========================================
    ADMIN_CINEMAS: {
        CREATE: `${API_BASE_URL}/api/admin/cinemas`,
        DETAIL: (id: string) => `${API_BASE_URL}/api/admin/cinemas/${id}`,
        SCREENS: (cinemaId: string) => `${API_BASE_URL}/api/admin/cinemas/${cinemaId}/screens`,
        SCREEN: (cinemaId: string, screenId: string) =>
            `${API_BASE_URL}/api/admin/cinemas/${cinemaId}/screens/${screenId}`,
        SEATS_BULK: (screenId: string) =>
            `${API_BASE_URL}/api/admin/cinemas/screens/${screenId}/seats/bulk`,
        SEAT: (screenId: string, seatId: string) =>
            `${API_BASE_URL}/api/admin/cinemas/screens/${screenId}/seats/${seatId}`,
    },

    // ==========================================
    // BOOKINGS (Customer - Auth required)
    // ==========================================
    BOOKINGS: {
        BASE: `${API_BASE_URL}/api/bookings`,
        MY: `${API_BASE_URL}/api/bookings/my`,
        DETAIL: (id: string) => `${API_BASE_URL}/api/bookings/${id}`,
        CANCEL: (id: string) => `${API_BASE_URL}/api/bookings/${id}/cancel`,
        COMPLETE: (id: string) => `${API_BASE_URL}/api/bookings/${id}/complete`,
        REQUEST_REFUND: (id: string) => `${API_BASE_URL}/api/bookings/${id}/request-refund`,
        APPROVE_REFUND: (id: string) => `${API_BASE_URL}/api/bookings/${id}/approve-refund`,
        CHECK_IN: (id: string) => `${API_BASE_URL}/api/bookings/${id}/check-in`,
        CHECK_IN_QR: `${API_BASE_URL}/api/bookings/check-in`,
        CALLBACK: `${API_BASE_URL}/api/bookings/callback`,
    },

    // ==========================================
    // MANAGER - BOOKINGS (Manager/Admin)
    // ==========================================
    MANAGER_BOOKINGS: {
        BASE: (cinemaId: string, status?: string, date?: string, page = 1, pageSize = 20) =>
            `${API_BASE_URL}/api/manager/bookings?cinemaId=${cinemaId}${status ? `&status=${status}` : ''}${date ? `&date=${date}` : ''}&page=${page}&pageSize=${pageSize}`,
        TODAY: (cinemaId: string) => `${API_BASE_URL}/api/manager/bookings/today?cinemaId=${cinemaId}`,
        REFUND_REQUESTS: (cinemaId: string) => `${API_BASE_URL}/api/manager/bookings/refund-requests?cinemaId=${cinemaId}`,
        APPROVE_REFUND: (id: string) => `${API_BASE_URL}/api/manager/bookings/${id}/approve-refund`,
    },

    // ==========================================
    // GENRES (Public)
    // ==========================================
    GENRES: {
        BASE: `${API_BASE_URL}/api/genres`,
    },

    // ==========================================
    // SEAT TYPES (Public)
    // ==========================================
    SEAT_TYPES: {
        BASE: `${API_BASE_URL}/api/seat-types`,
    },

    // ==========================================
    // PROMOTIONS (Public + Admin)
    // ==========================================
    PROMOTIONS: {
        BASE: `${API_BASE_URL}/api/promotions`,
        VALIDATE: (code: string, orderTotal: number) =>
            `${API_BASE_URL}/api/promotions/validate?code=${code}&orderTotal=${orderTotal}`,
    },

    // ==========================================
    // ADMIN - PROMOTIONS (Admin/Manager)
    // ==========================================
    ADMIN_PROMOTIONS: {
        BASE: `${API_BASE_URL}/api/admin/promotions`,
        DETAIL: (id: string) => `${API_BASE_URL}/api/admin/promotions/${id}`,
        ACTIVATE: (id: string) => `${API_BASE_URL}/api/admin/promotions/${id}/activate`,
        DEACTIVATE: (id: string) => `${API_BASE_URL}/api/admin/promotions/${id}/deactivate`,
    },

    // ==========================================
    // ADMIN - DASHBOARD (Admin only)
    // ==========================================
    ADMIN_DASHBOARD: {
        STATS: (cinemaId?: string) =>
            `${API_BASE_URL}/api/admin/dashboard/stats${cinemaId ? `?cinemaId=${cinemaId}` : ''}`,
        REVENUE: (from: string, to: string, cinemaId?: string, groupBy = 'day') =>
            `${API_BASE_URL}/api/admin/dashboard/revenue?from=${from}&to=${to}&groupBy=${groupBy}${cinemaId ? `&cinemaId=${cinemaId}` : ''}`,
        TOP_MOVIES: (limit = 10, from?: string, to?: string) =>
            `${API_BASE_URL}/api/admin/dashboard/top-movies?limit=${limit}${from ? `&from=${from}` : ''}${to ? `&to=${to}` : ''}`,
    },

    // ==========================================
    // MANAGER - DASHBOARD (Manager only)
    // ==========================================
    MANAGER_DASHBOARD: {
        STATS: (cinemaId: string) =>
            `${API_BASE_URL}/api/manager/dashboard/stats?cinemaId=${cinemaId}`,
        REVENUE: (cinemaId: string, from: string, to: string, groupBy = 'day') =>
            `${API_BASE_URL}/api/manager/dashboard/revenue?cinemaId=${cinemaId}&from=${from}&to=${to}&groupBy=${groupBy}`,
        TOP_MOVIES: (cinemaId: string, limit = 10, from?: string, to?: string) =>
            `${API_BASE_URL}/api/manager/dashboard/top-movies?cinemaId=${cinemaId}&limit=${limit}${from ? `&from=${from}` : ''}${to ? `&to=${to}` : ''}`,
    },

    // ==========================================
    // ADMIN - USERS (Admin only)
    // ==========================================
    ADMIN_USERS: {
        BASE: `${API_BASE_URL}/api/admin/users`,
        DETAIL: (userId: string) => `${API_BASE_URL}/api/admin/users/${userId}`,
        LOCK: (userId: string) => `${API_BASE_URL}/api/admin/users/${userId}/lock`,
        UNLOCK: (userId: string) => `${API_BASE_URL}/api/admin/users/${userId}/unlock`,
        STAFF: `${API_BASE_URL}/api/admin/users/staff`,
        ROLE: (userId: string) => `${API_BASE_URL}/api/admin/users/${userId}/role`,
    },

    // ==========================================
    // ADMIN - TIME SLOTS (Admin only)
    // ==========================================
    ADMIN_TIMESLOTS: {
        BASE: `${API_BASE_URL}/api/admin/time-slots`,
        DETAIL: (id: string) => `${API_BASE_URL}/api/admin/time-slots/${id}`,
    },

    // ==========================================
    // ADMIN - EQUIPMENT (Admin only)
    // ==========================================
    ADMIN_EQUIPMENT: {
        BASE: `${API_BASE_URL}/api/admin/equipment`,
        DETAIL: (id: string) => `${API_BASE_URL}/api/admin/equipment/${id}`,
    },

    // ==========================================
    // ADMIN - ROLES (Admin only)
    // ==========================================
    ADMIN_ROLES: {
        BASE: `${API_BASE_URL}/api/admin/roles`,
        LIST: `${API_BASE_URL}/api/admin/roles/roles`,
        DETAIL: (roleId: string) => `${API_BASE_URL}/api/admin/roles/${roleId}`,
    },

    // ==========================================
    // ADMIN - PRICING TIERS (Admin only)
    // ==========================================
    ADMIN_PRICING_TIERS: {
        BASE: `${API_BASE_URL}/api/admin/pricing-tiers`,
        DETAIL: (id: string) => `${API_BASE_URL}/api/admin/pricing-tiers/${id}`,
    },

    // ==========================================
    // MANAGER - STAFF (Manager/Admin)
    // ==========================================
    MANAGER_STAFF: {
        BASE: (cinemaId: string, page = 1, pageSize = 20) =>
            `${API_BASE_URL}/api/manager/staff?cinemaId=${cinemaId}&page=${page}&pageSize=${pageSize}`,
        DETAIL: (id: string) => `${API_BASE_URL}/api/manager/staff/${id}`,
    },

    // ==========================================
    // MANAGER - SHIFTS (Manager/Admin)
    // ==========================================
    MANAGER_SHIFTS: {
        BASE: (cinemaId: string) => `${API_BASE_URL}/api/manager/shifts?cinemaId=${cinemaId}`,
        DETAIL: (id: string) => `${API_BASE_URL}/api/manager/shifts/${id}`,
    },
} as const;

export type EndpointKeys = typeof Endpoints;
