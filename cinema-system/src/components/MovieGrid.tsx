import { Clock, Play } from "lucide-react";
import { Button } from "./ui/Button";
import { Badge } from "./ui/Badge";
import { useAppSelector } from "../hooks/redux";
import { useNavigate } from "react-router-dom";
import { useAppDispatch } from "../hooks/redux";
import { getShowtimeSeatingPlan } from '../store/slices/showtimeSlice';
import { useSignalR } from "../contexts/SignalRContext";
interface MovieGridProps {
  title: string;
  isLoading?: boolean;
}

// Skeleton component for loading state
const MovieSkeleton = () => (
  <div className="bg-white rounded-lg shadow-md overflow-hidden animate-pulse">
    {/* Poster skeleton */}
    <div className="w-full h-64 bg-gray-300"></div>

    {/* Content skeleton */}
    <div className="p-4">
      <div className="h-6 bg-gray-300 rounded mb-2"></div>
      <div className="flex items-center justify-between mb-2">
        <div className="h-4 bg-gray-300 rounded w-20"></div>
      </div>
      <div className="flex gap-2 mb-3">
        <div className="h-6 bg-gray-300 rounded w-16"></div>
        <div className="h-6 bg-gray-300 rounded w-20"></div>
      </div>
      <div className="space-y-2 mb-4">
        <div className="h-4 bg-gray-300 rounded"></div>
        <div className="h-4 bg-gray-300 rounded w-3/4"></div>
      </div>
      <div className="h-10 bg-gray-300 rounded"></div>
    </div>
  </div>
);

// Empty state component
const EmptyState = () => (
  <div className="col-span-full text-center py-16">
    <div className="mx-auto w-24 h-24 bg-gray-200 rounded-full flex items-center justify-center mb-4">
      <Play className="h-12 w-12 text-gray-400" />
    </div>
    <h3 className="text-xl font-medium text-gray-800 mb-2">
      Chưa có dữ liệu phim
    </h3>
    <p className="text-gray-600 mb-4">
      Vui lòng chọn rạp, phim và ngày để tìm suất chiếu
    </p>
  </div>
);

const MovieGrid = ({ title, isLoading = false }: MovieGridProps) => {
  const dispatch = useAppDispatch();
  const { movieFeature } = useAppSelector(state => state.movie);
  const navigate = useNavigate();

  interface ShowtimeBaseResponse {
    showtimeId: string;
    actualStartTime: string;
    actualEndTime: string;
  }

  const formatShowtimes = (screeningSlots: ShowtimeBaseResponse[]) => {
    if (!screeningSlots || screeningSlots.length === 0) return [];

    return screeningSlots.map(slot => {
      const start = new Date(slot.actualStartTime).toLocaleTimeString("vi-VN", {
        hour: "2-digit",
        minute: "2-digit",
        hour12: false, // hiển thị 24h
      });

      const end = new Date(slot.actualEndTime).toLocaleTimeString("vi-VN", {
        hour: "2-digit",
        minute: "2-digit",
        hour12: false,
      });

      return {
        id: slot.showtimeId,
        time: `${start} - ${end}`, // ghép giờ bắt đầu và kết thúc
        available: true,
      };
    });
  };


  const handleShowtimeClick = async (showtimeId: string) => {
    await dispatch(getShowtimeSeatingPlan(showtimeId));
    navigate(`/seating-plan/${showtimeId}`);
  }

  return (
    <section className="py-16 bg-gray-50">
      <div className="container mx-auto px-4">
        {/* Section Header */}
        <div className="mb-8">
          <h2 className="text-3xl font-bold text-gray-800 mb-2">{title}</h2>
          <p className="text-gray-600">
            {isLoading
              ? "Đang tìm kiếm suất chiếu..."
              : (movieFeature)
                ? `Xuất chiếu của bộ phim ${movieFeature.title} tại rạp ${movieFeature.cinemaName}`
                : "Chọn rạp, phim và ngày để xem suất chiếu"
            }
          </p>
        </div>

        {/* Movies Grid */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {isLoading ? (
            // Show skeleton loading
            Array.from({ length: 8 }).map((_, index) => (
              <MovieSkeleton key={index} />
            ))
          ) : !movieFeature ? (
            // Show empty state
            <EmptyState />
          ) : (
            <div
              key={`${movieFeature.title}`}
              className="group bg-white rounded-lg shadow-md overflow-hidden hover:shadow-xl transition-all duration-300 transform hover:-translate-y-2"
            >
              {/* Movie Poster */}
              <div className="relative overflow-hidden">
                <img
                  src={movieFeature.postUrl}
                  alt={movieFeature.title}
                  className="w-full h-64 object-cover group-hover:scale-110 group-hover:brightness-75 transition-all duration-300"
                  //className="w-full h-64 object-cover group-hover:scale-110 transition-transform duration-300"
                  onError={(e) => {
                    const target = e.currentTarget;
                    // Kiểm tra để chắc chắn không lặp lại nếu ảnh fallback cũng lỗi
                    if (target.src.endsWith('/fallback.png')) {
                      return;
                    }
                    // Dùng đường dẫn tuyệt đối từ thư mục public
                    target.src = '/fallback.png';
                  }}
                />

                {/* Overlay */}
                <div className="absolute inset-0 transition-all duration-300 flex items-center justify-center">
                  <Button
                    size="sm"
                    className="opacity-0 group-hover:opacity-100 transform translate-y-4 group-hover:translate-y-0 transition-all duration-300 bg-red-600 hover:bg-red-700"
                    onClick={() => window.open(movieFeature.trailer, '_blank')}
                  >
                    <Play className="h-4 w-4 mr-2" />
                    Trailer
                  </Button>
                </div>

                {/* Age Rating Badge */}
                {movieFeature.ageRating && (
                  <Badge
                    className={`absolute top-2 left-2 ${movieFeature.ageRating === "P"
                      ? "bg-green-500"
                      : movieFeature.ageRating === "T13"
                        ? "bg-yellow-500"
                        : movieFeature.ageRating === "T16"
                          ? "bg-orange-500"
                          : "bg-red-500"
                      }`}
                  >
                    {movieFeature.ageRating}
                  </Badge>
                )}
              </div>

              {/* Movie Info */}
              <div className="p-4">
                <h3 className="font-bold text-lg text-gray-800 mb-2 line-clamp-1">
                  {movieFeature.title}
                </h3>

                <div className="flex items-center justify-between mb-2">
                  <div className="flex items-center space-x-1 text-gray-500">
                    <Clock className="h-4 w-4" />
                    <span className="text-sm">{movieFeature.durationMinutes} phút</span>
                  </div>
                  <div className="text-sm text-gray-500">
                    {new Date(movieFeature.releaseDate).toLocaleDateString('vi-VN')}
                  </div>
                </div>

                <div className="flex flex-wrap gap-1 mb-3">
                  {movieFeature.genres?.slice(0, 2).map((genre, idx) => (
                    <Badge key={idx} variant="secondary" className="text-xs">
                      {genre}
                    </Badge>
                  ))}
                </div>

                <p className="text-gray-600 text-sm mb-4 line-clamp-2">
                  {movieFeature.description}
                </p>

                {/* Showtimes */}
                {movieFeature.screeningSlots && movieFeature.screeningSlots.length > 0 && (
                  <div className="mb-4">
                    <p className="text-sm font-medium text-gray-700 mb-2">
                      Suất chiếu:
                    </p>
                    <div className="flex flex-wrap gap-2">
                      {formatShowtimes(movieFeature.screeningSlots).slice(0, 4).map((showtime, idx) => (
                        <span
                          key={idx}
                          className={`px-2 py-1 text-xs rounded border transition-colors 
                            ${"bg-gray-100 hover:bg-red-50 hover:border-red-300 cursor-pointer"}`}
                          onClick={() => handleShowtimeClick(showtime.id)}
                        >
                          {showtime.time}
                        </span>
                      ))}
                      {movieFeature.screeningSlots.length > 4 && (
                        <span className="px-2 py-1 text-xs text-gray-500">
                          +{movieFeature.screeningSlots.length - 4} suất khác
                        </span>
                      )}
                    </div>
                  </div>
                )}
              </div>
            </div>
          )
          }
        </div>

        {/* Load More Button - Only show when there are movies */}
        {!isLoading && movieFeature && (
          <div className="text-center mt-8">
            <Button
              variant="outline"
              size="lg"
              className="border-red-600 text-red-600 hover:bg-red-600 hover:text-white"
            >
              Xem Thêm Phim
            </Button>
          </div>
        )}
      </div>
    </section>
  );
};

export default MovieGrid;