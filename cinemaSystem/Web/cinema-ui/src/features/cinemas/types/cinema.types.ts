// Cinema Types - Based on Backend API

export interface Cinema {
    id: string;
    name: string;
    address: string;
    district?: string;
    city?: string;
    phone?: string;
    email?: string;
    imageUrl?: string;
    totalScreens: number;
    isActive: boolean;
    createdAt: string;
    updatedAt: string;
}

export interface CinemaDetail extends Cinema {
    screens: Screen[];
    facilities?: string[];
    parkingInfo?: string;
    openingHours?: string;
}

export interface Screen {
    id: string;
    cinemaId: string;
    name: string;
    type: ScreenType;
    capacity: number;
    rows: number;
    columns: number;
    isActive: boolean;
    seats?: Seat[];
}

export type ScreenType = 'Standard' | 'IMAX' | '4DX' | 'VIP' | 'GOLD' | 'COUPLE';

export interface Seat {
    id: string;
    screenId: string;
    row: string;
    column: number;
    seatNumber: string;
    seatTypeId: string;
    seatType?: SeatType;
    isBlocked: boolean;
    isCoupleSeat: boolean;
    linkedSeatId?: string;
    positionX?: number;
    positionY?: number;
}

export interface SeatType {
    id: string;
    name: string;
    code: string;
    basePrice: number;
    description?: string;
    color?: string;
}

// Cinema List Response
export interface CinemaListResponse {
    items: Cinema[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
}

// Admin Cinema Management
export interface CreateCinemaRequest {
    name: string;
    address: string;
    district?: string;
    city?: string;
    phone?: string;
    email?: string;
    imageUrl?: string;
}

export interface UpdateCinemaRequest extends Partial<CreateCinemaRequest> {
    id: string;
}

// Screen Management
export interface CreateScreenRequest {
    cinemaId: string;
    name: string;
    type: ScreenType;
    capacity: number;
    rows: number;
    columns: number;
}

export interface UpdateScreenRequest extends Partial<CreateScreenRequest> {
    id: string;
}
