
// HeroCarousel
interface MovieComingSoon {
    movieId: string;
    title: string;
    description: string;
    postUrl: string;
    genres: string[];
    duration: number;
    trailer: string;
}

// QuickBooking
interface CinemaQuery {
    cinemaId: string;
    cinemaName: string;
    address: string;
}
interface MovieQuery {
    movieId: string;
    title: string;
}
interface DateOptionQuery {
    value: string;
    label: string;
}
interface StatisticItem {
    totalCinemas: number;
    totalMovies: number;
    totalUsers: number;
}
interface MovieSection {
    movieId: string;
    title: string;
    durationMinutes: number;
    releaseDate: string;
    description: string;
    posterUrl: string;
    ageRating: string;
}

// MovieGrid
interface Movie {
    title: string;
    postUrl: string;
    description: string;
    genres: string[];
    durationMinutes: number;
    trailer: string;
    ageRating: string;
    cinemaName: string;
    releaseDate: string;
    screeningSlots: ShowtimeBaseResponse[];
}
interface ShowtimeBaseResponse {
    showtimeId: string;
    actualStartTime: string;
    actualEndTime: string;
}
interface SearchParams {
    cinemaId: string;
    movieId: string;
    showDate: string;
}
//----------------------
interface MovieDetail {
    //id: number;
    title: string;
    durationMinutes: number;
    releaseDate: string;
    description: string;
    posterUrl: string;
    status: string;
    createdAt: string;
}

interface MovieCastCrew {
    id: string;
    personName: string;
    roleType: string;
}

interface MovieCertification {
    id: string;
    certificationBody: string;
    rating: string;
    issueDate: string;
}


interface Genre {
    id: string;
    genreName: string;
    description: string;
    isActive: boolean;
}

interface MovieCopyright {
    id: string;
    distributorCompany: string;
    licenseStartDate: string;
    licenseEndDate: string;
    status: string;
}

interface MovieDetailsData {
    movie: MovieDetail;
    castCrews: MovieCastCrew[];
    certifications: MovieCertification[];
    genres: Genre[];
    copyrights: MovieCopyright[];
}

export type { MovieComingSoon, CinemaQuery, ShowtimeBaseResponse, MovieQuery, DateOptionQuery, Movie, StatisticItem, SearchParams, MovieDetailsData, MovieSection };