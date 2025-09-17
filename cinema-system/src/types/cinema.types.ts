interface Cinema {
    cinemaId: string;
    cinemaName: string;
    address: string;
    phone: string;
    image: string;
    screens: number;
}
//---------------------
interface CinemaDetails {
    cinemaName: string;
    address: string;
    phone?: string;
    email?: string;
    image?: string;
    managerName: string;
    status: string;
}
interface Screen {
    id: string;
    screenName: string;
    type: string;
    status: string;
    seatCount: number;
}
interface CinemaDetailsData {
    cinema: CinemaDetails;
    screens: Screen[];
}

export type { Cinema, CinemaDetails, Screen, CinemaDetailsData };