interface BookingCheckedInResponse {
    bookingCode: string;
    bookingTime: string;
    movieTitle: string;
    screenName: string;
    seatsList: string[];
    totalAmount: number;
    totalTickets: number;
    cinemaName: string;
    actualStartTime: string;
    actualEndTime: string;
    status: string;
    isCheckedIn: boolean;
}

export type { BookingCheckedInResponse };