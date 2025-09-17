import { Clock } from "lucide-react";
import type { Movie } from "../../types/movie.types";

interface MovieCardProps {
    movie: Movie;
    onShowtimeClick: (showtimeId: string) => void;
}

const MovieCard: React.FC<MovieCardProps> = ({ movie, onShowtimeClick }) => {
    return (
        <div className="border border-gray-200 rounded-lg p-4">
            <div className="flex space-x-4">
                <img src={movie.postUrl} alt={movie.title} className="w-24 h-32 object-cover rounded" />
                <div className="flex-1">
                    <h4 className="font-semibold text-lg">{movie.title}</h4>
                    <p className="text-sm text-gray-600">{movie.description}</p>
                    {/* <div className="text-xs text-gray-500 mt-1">
                        Thể loại: {'Không cơ sở'}
                    </div> */}
                    <div className="text-xs text-gray-500">Thời lượng: {movie.durationMinutes} phút</div>
                    <div className="text-xs text-gray-500">
                        Trailer: <a href={movie.trailer} className="text-blue-500">Xem</a>
                    </div>
                    <div className="text-xs text-gray-500">Độ tuổi: {movie.ageRating}</div>
                    <div className="text-xs text-gray-500">Rạp: {movie.cinemaName}</div>
                </div>
            </div>
            <div className="mt-4">
                <h5 className="font-medium text-sm mb-2 flex items-center">
                    <Clock className="w-4 h-4 mr-1" />
                    Suất chiếu:
                </h5>
                <div className="flex flex-wrap gap-2">
                    {movie.screeningSlots.map(slot => (
                        <button
                            key={slot.showtimeId}
                            onClick={() => onShowtimeClick(slot.showtimeId)}
                            className="px-3 py-1 bg-green-100 text-green-800 rounded hover:bg-green-200 text-sm"
                        >
                            {slot.actualStartTime} - {slot.actualEndTime}
                        </button>
                    ))}
                </div>
            </div>
        </div>
    );
};

export default MovieCard;