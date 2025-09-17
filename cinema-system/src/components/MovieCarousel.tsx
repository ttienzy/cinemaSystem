import { useState, useRef } from "react";
import { ChevronLeft, ChevronRight, Calendar, Bell, Star } from "lucide-react";
import { Button } from "./ui/Button";
import { Badge } from "./ui/Badge";

interface Movie {
  id: number;
  title: string;
  poster: string;
  rating: number;
  genre: string[];
  releaseDate: string;
  ageRating: string;
  description: string;
  isComingSoon: boolean;
}

interface MovieCarouselProps {
  title: string;
  movies?: Movie[];
}

const MovieCarousel = ({ title, movies = [] }: MovieCarouselProps) => {
  const scrollRef = useRef<HTMLDivElement>(null);
  const [canScrollLeft, setCanScrollLeft] = useState<boolean>(false);
  const [canScrollRight, setCanScrollRight] = useState<boolean>(true);

  // Mock data nếu không có dữ liệu từ API
  const mockMovies: Movie[] = [
    {
      id: 1,
      title: "Avatar: The Way of Water",
      poster: "https://via.placeholder.com/300x450/0ea5e9/ffffff?text=Avatar+2",
      rating: 8.1,
      genre: ["Khoa Học Viễn Tưởng", "Phiêu Lưu"],
      releaseDate: "2024-12-16",
      ageRating: "T13",
      description:
        "Jake Sully và gia đình tiếp tục cuộc phiêu lưu trên Pandora...",
      isComingSoon: true,
    },
    {
      id: 2,
      title: "Black Panther: Wakanda Forever",
      poster:
        "https://via.placeholder.com/300x450/7c3aed/ffffff?text=Black+Panther",
      rating: 7.9,
      genre: ["Hành Động", "Chính Kịch"],
      releaseDate: "2024-11-11",
      ageRating: "T13",
      description:
        "Wakanda phải đối mặt với thử thách mới sau cái chết của T'Challa...",
      isComingSoon: true,
    },
    {
      id: 3,
      title: "Fast X",
      poster: "https://via.placeholder.com/300x450/dc2626/ffffff?text=Fast+X",
      rating: 7.2,
      genre: ["Hành Động", "Tội Phạm"],
      releaseDate: "2024-05-19",
      ageRating: "T16",
      description:
        "Dom Toretto và gia đình đối mặt với kẻ thù nguy hiểm nhất...",
      isComingSoon: true,
    },
    {
      id: 4,
      title: "Guardians of the Galaxy Vol. 3",
      poster:
        "https://via.placeholder.com/300x450/059669/ffffff?text=Guardians+3",
      rating: 8.3,
      genre: ["Hành Động", "Hài"],
      releaseDate: "2024-05-05",
      ageRating: "T13",
      description: "Cuộc phiêu lưu cuối cùng của đội Guardians...",
      isComingSoon: true,
    },
    {
      id: 5,
      title: "The Flash",
      poster:
        "https://via.placeholder.com/300x450/eab308/ffffff?text=The+Flash",
      rating: 6.8,
      genre: ["Hành Động", "Khoa Học Viễn Tưởng"],
      releaseDate: "2024-06-16",
      ageRating: "T13",
      description: "Barry Allen du hành thời gian để cứu mẹ mình...",
      isComingSoon: true,
    },
    {
      id: 6,
      title: "Indiana Jones 5",
      poster:
        "https://via.placeholder.com/300x450/92400e/ffffff?text=Indiana+Jones",
      rating: 7.5,
      genre: ["Hành Động", "Phiêu Lưu"],
      releaseDate: "2024-06-30",
      ageRating: "T13",
      description:
        "Cuộc phiêu lưu cuối cùng của nhà khảo cổ học huyền thoại...",
      isComingSoon: true,
    },
  ];

  const displayMovies = movies.length > 0 ? movies : mockMovies;

  const scroll = (direction: "left" | "right") => {
    if (scrollRef.current) {
      const scrollAmount = 320; // width of one card + gap
      const newScrollLeft =
        scrollRef.current.scrollLeft +
        (direction === "left" ? -scrollAmount : scrollAmount);

      scrollRef.current.scrollTo({
        left: newScrollLeft,
        behavior: "smooth",
      });
    }
  };

  const handleScroll = () => {
    if (scrollRef.current) {
      const { scrollLeft, scrollWidth, clientWidth } = scrollRef.current;
      setCanScrollLeft(scrollLeft > 0);
      setCanScrollRight(scrollLeft < scrollWidth - clientWidth - 10);
    }
  };

  const formatReleaseDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString("vi-VN", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    });
  };

  const getDaysUntilRelease = (dateString: string) => {
    const releaseDate = new Date(dateString);
    const today = new Date();
    const diffTime = releaseDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays > 0 ? diffDays : 0;
  };

  return (
    <section className="py-16 bg-white">
      <div className="container mx-auto px-4">
        {/* Section Header */}
        <div className="flex items-center justify-between mb-8">
          <div>
            <h2 className="text-3xl font-bold text-gray-800 mb-2">{title}</h2>
            <p className="text-gray-600">
              Những bộ phim đáng mong chờ sắp ra mắt
            </p>
          </div>

          {/* Navigation Buttons */}
          <div className="hidden md:flex space-x-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => scroll("left")}
              disabled={!canScrollLeft}
              className="border-red-600 text-red-600 hover:bg-red-600 hover:text-white disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => scroll("right")}
              disabled={!canScrollRight}
              className="border-red-600 text-red-600 hover:bg-red-600 hover:text-white disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>

        {/* Movies Carousel */}
        <div className="relative">
          <div
            ref={scrollRef}
            onScroll={handleScroll}
            className="flex space-x-6 overflow-x-auto scrollbar-hide pb-4"
            style={{ scrollbarWidth: "none", msOverflowStyle: "none" }}
          >
            {displayMovies.map((movie) => (
              <div
                key={movie.id}
                className="flex-none w-80 bg-white rounded-lg shadow-lg overflow-hidden hover:shadow-xl transition-all duration-300 transform hover:-translate-y-2"
              >
                <div className="flex">
                  {/* Movie Poster */}
                  <div className="relative w-32 h-48 flex-shrink-0">
                    <img
                      src={movie.poster}
                      alt={movie.title}
                      className="w-full h-full object-cover"
                    />

                    {/* Age Rating Badge */}
                    <Badge
                      className={`absolute top-2 left-2 ${movie.ageRating === "P"
                          ? "bg-green-500"
                          : movie.ageRating === "T13"
                            ? "bg-yellow-500"
                            : movie.ageRating === "T16"
                              ? "bg-orange-500"
                              : "bg-red-500"
                        }`}
                    >
                      {movie.ageRating}
                    </Badge>

                    {/* Coming Soon Badge */}
                    <Badge className="absolute bottom-2 left-2 bg-red-600">
                      Sắp Chiếu
                    </Badge>
                  </div>

                  {/* Movie Info */}
                  <div className="flex-1 p-4 flex flex-col justify-between">
                    <div>
                      <h3 className="font-bold text-lg text-gray-800 mb-2 line-clamp-2">
                        {movie.title}
                      </h3>

                      <div className="flex items-center space-x-1 mb-2">
                        <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                        <span className="text-sm font-medium">
                          {movie.rating}
                        </span>
                        <span className="text-gray-400">•</span>
                        <span className="text-sm text-gray-500">Dự kiến</span>
                      </div>

                      <div className="flex flex-wrap gap-1 mb-3">
                        {movie.genre.slice(0, 2).map((g) => (
                          <Badge
                            key={g}
                            variant="secondary"
                            className="text-xs"
                          >
                            {g}
                          </Badge>
                        ))}
                      </div>

                      <p className="text-gray-600 text-sm mb-3 line-clamp-2">
                        {movie.description}
                      </p>
                    </div>

                    <div>
                      {/* Release Date */}
                      <div className="flex items-center space-x-2 mb-3 text-sm text-gray-600">
                        <Calendar className="h-4 w-4" />
                        <span>{formatReleaseDate(movie.releaseDate)}</span>
                      </div>

                      {/* Days Until Release */}
                      <div className="bg-red-50 rounded-lg p-2 mb-3">
                        <div className="text-center">
                          <div className="text-2xl font-bold text-red-600">
                            {getDaysUntilRelease(movie.releaseDate)}
                          </div>
                          <div className="text-xs text-red-600">ngày nữa</div>
                        </div>
                      </div>

                      {/* Action Button */}
                      <Button
                        size="sm"
                        className="w-full bg-red-600 hover:bg-red-700 text-white"
                      >
                        <Bell className="h-4 w-4 mr-2" />
                        Đặt Thông Báo
                      </Button>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Mobile Navigation Buttons */}
          <div className="flex md:hidden justify-center space-x-2 mt-4">
            <Button
              variant="outline"
              size="sm"
              onClick={() => scroll("left")}
              disabled={!canScrollLeft}
              className="border-red-600 text-red-600 hover:bg-red-600 hover:text-white disabled:opacity-50"
            >
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => scroll("right")}
              disabled={!canScrollRight}
              className="border-red-600 text-red-600 hover:bg-red-600 hover:text-white disabled:opacity-50"
            >
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>

        {/* View All Button */}
        <div className="text-center mt-8">
          <Button
            variant="outline"
            size="lg"
            className="border-red-600 text-red-600 hover:bg-red-600 hover:text-white"
          >
            Xem Tất Cả Phim Sắp Chiếu
          </Button>
        </div>
      </div>

      <style>{`
        .scrollbar-hide {
          -ms-overflow-style: none;
          scrollbar-width: none;
        }
        .scrollbar-hide::-webkit-scrollbar {
          display: none;
        }
        .line-clamp-1 {
          display: -webkit-box;
          -webkit-line-clamp: 1;
          -webkit-box-orient: vertical;
          overflow: hidden;
        }
        .line-clamp-2 {
          display: -webkit-box;
          -webkit-line-clamp: 2;
          -webkit-box-orient: vertical;
          overflow: hidden;
        }
      `}</style>
    </section>
  );
};

export default MovieCarousel;
