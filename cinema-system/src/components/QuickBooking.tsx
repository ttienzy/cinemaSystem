import { useState } from "react";
import { MapPin, Film, Calendar, Search } from "lucide-react";
import { Button } from "./ui/Button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/Select";
import { useAppSelector, useAppDispatch } from "../hooks/redux";
import { getMoviesFeature } from "../store/slices/movieSlice";
import type { SearchParams } from "../types/movie.types";

interface DateOption {
  value: string;
  label: string;
}

interface QuickBookingProps {
  onSearch?: (searchParams: SearchParams) => void;
}

const QuickBooking = ({ onSearch }: QuickBookingProps) => {
  const dispatch = useAppDispatch();
  const [selectedCinema, setSelectedCinema] = useState<string>("");
  const [selectedMovie, setSelectedMovie] = useState<string>("");
  const [selectedDate, setSelectedDate] = useState<string>("");

  const { cinemas, movies, statistic, loading } = useAppSelector(state => state.movie);

  // Generate next 7 days
  function toLocalMidnightISOString(date: Date): string {
    // Đặt giờ về 00:00:00 local
    date.setHours(0, 0, 0, 0);

    // Lấy offset múi giờ (phút) và chuyển sang ms
    const offset = date.getTimezoneOffset() * 60000;

    // Trừ offset để ra local time, không bị lệch UTC
    const localMidnight = new Date(date.getTime() - offset);

    // Trả về yyyy-MM-ddTHH:mm:ss
    return localMidnight.toISOString().slice(0, 19);
  }

  const dates: DateOption[] = Array.from({ length: 7 }, (_, i) => {
    const date = new Date();
    date.setDate(date.getDate() + i);

    return {
      value: toLocalMidnightISOString(date), // ví dụ: "2025-09-05T00:00:00"
      label:
        i === 0
          ? "Hôm nay"
          : i === 1
            ? "Ngày mai"
            : date.toLocaleDateString("vi-VN", {
              weekday: "short",
              day: "2-digit",
              month: "2-digit",
            }),
    };
  });



  const handleSearch = async () => {
    if (!selectedCinema || !selectedMovie || !selectedDate) {
      alert("Vui lòng chọn đầy đủ thông tin!");
      return;
    }

    const searchParams = {
      cinemaId: selectedCinema,
      movieId: selectedMovie,
      showDate: selectedDate,
    };

    try {
      // Call the search API
      await dispatch(getMoviesFeature(searchParams));
      console.log(searchParams);
      // Call the callback if provided
      if (onSearch) {
        onSearch(searchParams);
      }

      console.log("Searching for showtimes:", searchParams);
    } catch (error) {
      console.error("Search failed:", error);
      alert("Có lỗi xảy ra khi tìm kiếm. Vui lòng thử lại!");
    }
  };


  return (
    <div className="bg-white shadow-lg rounded-lg p-6 mx-4 -mt-16 relative z-20">
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-gray-800 mb-2">Đặt Vé Nhanh</h2>
        <p className="text-gray-600">
          Chọn rạp, phim và ngày để tìm suất chiếu
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        {/* Cinema Selection */}
        <div className="space-y-2">
          <label className="flex items-center text-sm font-medium text-gray-700">
            <MapPin className="h-4 w-4 mr-2 text-red-600" />
            Chọn Rạp
          </label>
          <Select
            value={selectedCinema}
            onValueChange={setSelectedCinema}
            disabled={loading}
          >
            <SelectTrigger>
              <SelectValue placeholder="Chọn rạp chiếu" />
            </SelectTrigger>
            <SelectContent>
              {cinemas.map((cinema) => (
                <SelectItem key={cinema.cinemaId} value={cinema.cinemaId.toString()}>
                  <div>
                    <div className="font-medium">{cinema.cinemaName}</div>
                    <div className="text-xs text-gray-500">
                      {cinema.address}
                    </div>
                  </div>
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {/* Movie Selection */}
        <div className="space-y-2">
          <label className="flex items-center text-sm font-medium text-gray-700">
            <Film className="h-4 w-4 mr-2 text-red-600" />
            Chọn Phim
          </label>
          <Select
            value={selectedMovie}
            onValueChange={setSelectedMovie}
            disabled={loading}
          >
            <SelectTrigger>
              <SelectValue placeholder="Chọn phim" />
            </SelectTrigger>
            <SelectContent>
              {movies.map((movie) => (
                <SelectItem key={movie.movieId} value={movie.movieId.toString()}>
                  {movie.title}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {/* Date Selection */}
        <div className="space-y-2">
          <label className="flex items-center text-sm font-medium text-gray-700">
            <Calendar className="h-4 w-4 mr-2 text-red-600" />
            Chọn Ngày
          </label>
          <Select
            value={selectedDate}
            onValueChange={setSelectedDate}
            disabled={loading}
          >
            <SelectTrigger>
              <SelectValue placeholder="Chọn ngày" />
            </SelectTrigger>
            <SelectContent>
              {dates.map((date) => (
                <SelectItem key={date.value} value={date.value}>
                  {date.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {/* Search Button */}
        <div className="space-y-2">
          <label className="text-sm font-medium text-transparent">Action</label>
          <div className="flex gap-2">
            <Button
              onClick={handleSearch}
              disabled={loading}
              className="flex-1 bg-red-600 hover:bg-red-700 text-white disabled:opacity-50"
              size="lg"
            >
              {loading ? (
                <>
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                  Đang tìm...
                </>
              ) : (
                <>
                  <Search className="h-4 w-4 mr-2" />
                  Tìm Suất Chiếu
                </>
              )}
            </Button>
          </div>
        </div>
      </div>

      {/* Search Status */}
      {(selectedCinema || selectedMovie || selectedDate) && (
        <div className="mt-4 p-3 bg-blue-50 rounded-lg border border-blue-200">
          <div className="text-sm text-blue-800">
            <strong>Tìm kiếm:</strong>
            {selectedCinema && (
              <span className="ml-2 text-blue-600">
                Rạp: {cinemas.find(c => c.cinemaId.toString() === selectedCinema)?.cinemaName}
              </span>
            )}
            {selectedMovie && (
              <span className="ml-2 text-blue-600">
                | Phim: {movies.find(m => m.movieId.toString() === selectedMovie)?.title}
              </span>
            )}
            {selectedDate && (
              <span className="ml-2 text-blue-600">
                | Ngày: {dates.find(d => d.value === selectedDate)?.label}
              </span>
            )}
          </div>
        </div>
      )}

      {/* Quick Stats */}
      <div className="mt-6 pt-6 border-t border-gray-200">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
          <div>
            <div className="text-2xl font-bold text-red-600">{statistic.totalCinemas || 0}+</div>
            <div className="text-sm text-gray-600">Rạp Chiếu</div>
          </div>
          <div>
            <div className="text-2xl font-bold text-red-600">{statistic.totalMovies || 0}+</div>
            <div className="text-sm text-gray-600">Phim Hot</div>
          </div>
          <div>
            <div className="text-2xl font-bold text-red-600">{statistic.totalUsers || 0}+</div>
            <div className="text-sm text-gray-600">Khách Hàng</div>
          </div>
          <div>
            <div className="text-2xl font-bold text-red-600">24/7</div>
            <div className="text-sm text-gray-600">Hỗ Trợ</div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default QuickBooking;