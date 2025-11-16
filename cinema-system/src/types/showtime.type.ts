
interface ShowtimeInfo {
    id: string;
    showDate: string;
    actualStartTime: string;
    actualEndTime: string;
    movieTitle: string;
    cinemaName: string;
    screenName: string;
}
interface Pricing {
    seatTypeId: string;
    finalPrice: number;
}
interface Seat {
    id: string;
    rowName: string;
    number: number;
    seatTypeId: string;
    seatTypeName: string;
    status: string;
}
interface SeatsSelectedResponse {
    showtimeInfo: ShowtimeInfo;
    pricings: Pricing[];
    seats: Seat[];
}
//----------------------------------------
interface PaymentInfoRequest {
    userId: string;
    showtimeId: string;
    selectedSeats: SelectedSeat[];
}
interface SelectedSeat {
    seatId: string;
    price: number;
}
//----------------------------------------
interface SeatReserved {
    showtimeId: string;
    seatIds: string[];
}
interface UpdateSeatStatusPayload {
    seatIds: string[];
    status: string; // 'Reserved', 'Available', etc.
}

interface Screen {
    screenId: string;
    screenName: string;
    screenType: string;
    seatCapacity: number;
    isActive: boolean;
}

interface Slot {
    slotId: string;
    startTime: string;
    endTime: string;
    slotName: string;
    isPeakTime: boolean;
}

interface PricingTier {
    pricingTierId: string;
    tierName: string;
    multiplier: number;
    description: string;
}

interface Movie {
    movieId: string;
    title: string;
    duration: number;
}
interface Showtime {
    showtimeId: string;
    title: string;
    screenName: string;
    screenType: string;
    showDate: string;
    slotStartTime: string;
    slotEndTime: string;
    actualStartTime: string;
    actualEndTime: string;
    status: string;
    pricingTier: string;
    multiplier: number;
    totalBookings: number;
    avgTicketPrice: number;
}
interface SeatType {
    seatTypeId: string;
    seatTypeName: string;
    multiplier: number;
}
interface ShowtimeSetupDataDto {
    screens: Screen[];
    slots: Slot[];
    pricingTiers: PricingTier[];
    movies: Movie[];
    seatTypes: SeatType[];
}

interface ShowtimePricingInfoRequest {
    seatTypeId: string;
    basePrice: number;
}

interface InputShowtimeDto {
    movieId: string;
    cinemaId: string;
    screenId: string;
    slotId: string;
    pricingTierId: string;
    showDate: string;
    actualStartTime: string;
    actualEndTime: string;
    status: string;
    showtimePricings: ShowtimePricingInfoRequest[];
}

type SeatStatus = 'Available' | 'Reserved' | 'Booked' | 'Unavailable';
export type {
    ShowtimeInfo, Pricing, Seat, SeatsSelectedResponse, PaymentInfoRequest, SelectedSeat, SeatStatus, SeatReserved, UpdateSeatStatusPayload,
    Screen, Slot, PricingTier, Movie, Showtime, ShowtimeSetupDataDto, InputShowtimeDto, ShowtimePricingInfoRequest
};