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

type SeatStatus = 'Available' | 'Reserved' | 'Booked' | 'Unavailable';
export type { ShowtimeInfo, Pricing, Seat, SeatsSelectedResponse, PaymentInfoRequest, SelectedSeat, SeatStatus, SeatReserved, UpdateSeatStatusPayload };