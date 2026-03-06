// Booking Types - Based on Backend API

export interface Booking {
    id: string;
    bookingCode: string;
    userId: string;
    userName?: string;
    userEmail?: string;
    userPhone?: string;
    showtimeId: string;
    showtime?: {
        id: string;
        movieTitle: string;
        cinemaName: string;
        screenName: string;
        startTime: string;
    };
    seats: BookingSeat[];
    totalAmount: number;
    discountAmount: number;
    finalAmount: number;
    status: BookingStatus;
    paymentMethod?: string;
    paymentStatus: PaymentStatus;
    vnpTransactionNo?: string;
    bookingDate: string;
    expiresAt: string;
    checkedInAt?: string;
    createdAt: string;
    updatedAt: string;
}

export interface BookingSeat {
    seatId: string;
    seatNumber: string;
    row: string;
    column: number;
    seatType: string;
    price: number;
}

export type BookingStatus =
    | 'Pending'
    | 'Confirmed'
    | 'Completed'
    | 'Cancelled'
    | 'Refunded'
    | 'Expired';

export type PaymentStatus =
    | 'Pending'
    | 'Paid'
    | 'Failed'
    | 'Refunded'
    | 'PartiallyRefunded';

// Booking List Response
export interface BookingListResponse {
    items: Booking[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
}

// Create Booking Request
export interface CreateBookingRequest {
    showtimeId: string;
    seatIds: string[];
    promotionCode?: string;
}

// Booking Response (after creation)
export interface CreateBookingResponse {
    booking: Booking;
    paymentUrl?: string; // VNPay URL for payment
    expiresAt: string;
}

// Booking Detail
export interface BookingDetail extends Booking {
    qrCode?: string;
    moviePoster?: string;
    movieDuration?: number;
    cinemaAddress?: string;
}

// My Bookings Query
export interface MyBookingsQuery {
    pageIndex?: number;
    pageSize?: number;
    status?: BookingStatus;
    fromDate?: string;
    toDate?: string;
}

// Payment Callback Response
export interface PaymentCallbackResponse {
    bookingId: string;
    success: boolean;
    message: string;
    transactionNo?: string;
    amount?: number;
    responseCode?: string;
}

// Booking Statistics (for admin)
export interface BookingStats {
    totalBookings: number;
    totalRevenue: number;
    completedBookings: number;
    pendingBookings: number;
    cancelledBookings: number;
    averageTicketPrice: number;
}
