interface UserInfo {
    userName: string;
    email: string;
    phoneNumber: string;
    role: string[];
    createdAt: string;
}

interface Purchase {
    bookingId: number;
    bookingTime: string;    
    movieTitle: string;
    totalAmount: number;
    totalTickets: number;
    showTime: string;
    cinemaName: string;
    status: string;
}

export type { UserInfo, Purchase };