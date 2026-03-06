// Showtime Types - Based on Backend API

export interface Showtime {
    id: string;
    movieId: string;
    movieTitle?: string;
    cinemaId: string;
    cinemaName?: string;
    screenId: string;
    screenName?: string;
    startTime: string; // ISO datetime
    endTime: string; // ISO datetime
    basePrice: number;
    status: ShowtimeStatus;
    availableSeats: number;
    totalSeats: number;
    isSpecial: boolean;
}

export type ShowtimeStatus = 'Scheduled' | 'Ongoing' | 'Completed' | 'Cancelled';

// Showtime with full details
export interface ShowtimeDetail extends Showtime {
    movie?: {
        id: string;
        title: string;
        posterUrl: string;
        duration: number;
    };
    cinema?: {
        id: string;
        name: string;
        address: string;
        district?: string;
        city?: string;
    };
    screen?: {
        id: string;
        name: string;
        capacity: number;
        seatTypes: SeatType[];
    };
}

// Seating Plan
export interface SeatingPlan {
    showtimeId: string;
    screenId: string;
    screenName: string;
    seats: Seat[];
    seatTypes: SeatType[];
    bookedSeats: BookedSeat[];
    lockedSeats: LockedSeat[];
}

export interface Seat {
    id: string;
    row: string;
    column: number;
    seatTypeId: string;
    seatTypeName?: string;
    seatNumber: string;
    isBlocked: boolean;
    isCoupleSeat: boolean;
    linkedSeatId?: string;
}

export interface SeatType {
    id: string;
    name: string;
    code: string;
    basePrice: number;
    description?: string;
    color?: string;
}

export interface BookedSeat {
    seatId: string;
    bookingId: string;
}

export interface LockedSeat {
    seatId: string;
    lockedUntil: string; // ISO datetime
}

// Showtime Search Params
export interface ShowtimeSearchParams {
    movieId?: string;
    cinemaId?: string;
    date?: string;
    fromDate?: string;
    toDate?: string;
    pageIndex?: number;
    pageSize?: number;
}
