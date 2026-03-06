// Cinemas API - Based on Backend API endpoints
import axios from '../../../shared/api/axios.instance';
import { Endpoints } from '../../../shared/api/endpoints';
import type { Cinema, CinemaDetail } from '../types/cinema.types';

export interface CinemasResponse {
    items: Cinema[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
}

export const cinemasApi = {
    // Get all cinemas
    getCinemas: async (pageIndex = 1, pageSize = 20): Promise<CinemasResponse> => {
        const response = await axios.get<CinemasResponse>(Endpoints.CINEMAS.BASE, {
            params: { pageIndex, pageSize },
        });
        return response.data;
    },

    // Get cinema details by ID
    getCinemaById: async (id: string): Promise<CinemaDetail> => {
        const response = await axios.get<CinemaDetail>(`${Endpoints.CINEMAS.BASE}/${id}`);
        return response.data;
    },

    // Block a seat in a screen
    blockSeat: async (screenId: string, seatId: string): Promise<void> => {
        const response = await axios.post(Endpoints.CINEMAS.SCREEN_SEAT_BLOCK(screenId, seatId));
        return response.data;
    },

    // Link seats (for couple seats)
    linkSeat: async (screenId: string, seatId: string, linkedSeatId: string): Promise<void> => {
        const response = await axios.post(Endpoints.CINEMAS.SCREEN_SEAT_LINK(screenId, seatId), {
            linkedSeatId,
        });
        return response.data;
    },

    // Unlink seats
    unlinkSeat: async (screenId: string, seatId: string): Promise<void> => {
        const response = await axios.post(Endpoints.CINEMAS.SCREEN_SEAT_UNLINK(screenId, seatId));
        return response.data;
    },
};

export default cinemasApi;
