import { useState, useEffect } from "react";
import { ChevronLeft, ChevronRight, Play, Star } from "lucide-react";
import { Button } from "./ui/Button";
import { useAppSelector } from "../hooks/redux";
import type { MovieComingSoon } from "../types/movie.types";
import { useNavigate } from "react-router-dom";



interface HeroCarouselProps {
  featuredMovies?: MovieComingSoon[];
}

const HeroCarousel = ({ featuredMovies = [] }: HeroCarouselProps) => {
  const [currentSlide, setCurrentSlide] = useState<number>(0);

  const navigate = useNavigate();

  const { moviesComingSoon } = useAppSelector(state => state.movie);

  // Mock data nếu không có dữ liệu từ API

  const movies = featuredMovies.length > 0 ? featuredMovies : moviesComingSoon;

  useEffect(() => {
    const timer = setInterval(() => {
      setCurrentSlide((prev) => (prev + 1) % movies.length);
    }, 5000);

    return () => clearInterval(timer);
  }, [movies.length]);

  const nextSlide = () => {
    setCurrentSlide((prev) => (prev + 1) % movies.length);
  };

  const prevSlide = () => {
    setCurrentSlide((prev) => (prev - 1 + movies.length) % movies.length);
  };

  if (movies.length === 0) return null;

  const currentMovie = movies[currentSlide];

  return (
    <div className="relative h-[70vh] overflow-hidden">
      {/* Background Image */}
      <div
        className="absolute inset-0 bg-cover bg-center transition-all duration-1000"
        style={{ backgroundImage: `url(https://via.placeholder.com/1920x1080/dc2626/ffffff?text=Spider-Man+Backdrop)` }}
      >
        <div className="absolute inset-0 bg-black bg-opacity-50" />
      </div>

      {/* Content */}
      <div className="relative z-10 container mx-auto px-4 h-full flex items-center">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 items-center w-full">
          {/* Movie Info */}
          <div className="text-white space-y-6">
            <div className="space-y-2">
              <div className="flex items-center space-x-4 text-sm">
                <span className="bg-red-600 px-3 py-1 rounded-full">
                  Phim Nổi Bật
                </span>
                <span>{currentMovie.duration} phút</span>
              </div>
              <h1 className="text-4xl lg:text-6xl font-bold leading-tight">
                {currentMovie.title}
              </h1>
              <p className="text-lg text-gray-300">{currentMovie.genres}</p>
            </div>

            <p className="text-lg leading-relaxed max-w-2xl">
              {currentMovie.description}
            </p>

            <div className="flex flex-col sm:flex-row gap-4">
              <Button
                size="lg"
                className="bg-red-600 hover:bg-red-700 text-white"
              >
                <Play className="h-5 w-5 mr-2" />
                Xem Trailer
              </Button>
              <Button
                size="lg"
                variant="outline"
                className="text-white border-white hover:bg-white hover:text-black"
                onClick={() => navigate('/movies/detail/' + currentMovie.movieId)}
              >
                Chi Tiết
              </Button>
            </div>
          </div>

          {/* Movie Poster */}
          <div className="hidden lg:flex justify-center">
            <div className="relative">
              <img
                src={currentMovie.postUrl}
                alt={currentMovie.title}
                className="w-80 h-auto rounded-lg shadow-2xl transform hover:scale-105 transition-transform duration-300"
              />
              <div className="absolute inset-0 bg-gradient-to-t from-black/20 to-transparent rounded-lg" />
            </div>
          </div>
        </div>
      </div>

      {/* Navigation Arrows */}
      <button
        title="Previous Slide"
        onClick={prevSlide}
        className="absolute left-4 top-1/2 transform -translate-y-1/2 z-20 bg-black bg-opacity-50 hover:bg-opacity-75 text-white p-2 rounded-full transition-all duration-200"
      >
        <ChevronLeft className="h-6 w-6" />
      </button>
      <button
        title="Next Slide"
        onClick={nextSlide}
        className="absolute right-4 top-1/2 transform -translate-y-1/2 z-20 bg-black bg-opacity-50 hover:bg-opacity-75 text-white p-2 rounded-full transition-all duration-200"
      >
        <ChevronRight className="h-6 w-6" />
      </button>

      {/* Dots Indicator */}
      <div className="absolute bottom-6 left-1/2 transform -translate-x-1/2 z-20 flex space-x-2">
        {movies.map((_, index) => (
          <button
            title="Select Slide"
            key={index}
            onClick={() => setCurrentSlide(index)}
            className={`w-3 h-3 rounded-full transition-all duration-200 ${index === currentSlide ? "bg-white" : "bg-white bg-opacity-50"
              }`}
          />
        ))}
      </div>
    </div>
  );
};

export default HeroCarousel;
