import { useState } from "react";
import {
  MapPin,
  Phone,
} from "lucide-react";
import type { Cinema } from "../types/cinema.types";
import { useAppSelector } from "../hooks/redux";
import { useNavigate } from "react-router-dom";

interface CinemaSectionProps {
  cinemas?: Cinema[];
}

const CinemaSection = ({ cinemas = [] }: CinemaSectionProps) => {
  const [selectedCity, setSelectedCity] = useState<string>("all");

  const navigate = useNavigate();

  const { cinemas: mockCinemas } = useAppSelector(state => state.cinema);
  const displayCinemas = cinemas.length > 0 ? cinemas : mockCinemas;

  const cities = ["all", "Ho Chi Minh", "Ha Noi", "Da Nang"];
  const cityLabels: { [key: string]: string } = {
    all: "Tất cả thành phố",
    "Ho Chi Minh": "TP. Hồ Chí Minh",
    "Ha Noi": "Hà Nội",
    "Da Nang": "Đà Nẵng",
  };

  // Filter cinemas based on address for city filtering
  const filteredCinemas = displayCinemas.filter((cinema) => {
    if (selectedCity === "all") return true;
    // Simple city filtering based on address content
    if (selectedCity === "Ho Chi Minh") {
      return cinema.address.includes("TP.HCM") || cinema.address.includes("Hồ Chí Minh");
    }
    if (selectedCity === "Ha Noi") {
      return cinema.address.includes("Hà Nội");
    }
    if (selectedCity === "Da Nang") {
      return cinema.address.includes("Đà Nẵng");
    }
    return true;
  });

  return (
    <section className="py-16 bg-gray-50">
      <div className="container mx-auto px-4">
        {/* Section Header */}
        <div className="text-center mb-12">
          <h2 className="text-3xl font-bold text-gray-800 mb-4">
            Hệ Thống Rạp
          </h2>
          <p className="text-gray-600 max-w-2xl mx-auto">
            Khám phá hệ thống rạp chiếu phim hiện đại với công nghệ tiên tiến và
            dịch vụ tốt nhất
          </p>
        </div>

        {/* City Filter */}
        <div className="flex justify-center mb-8">
          <div className="flex flex-wrap gap-2">
            {cities.map((city) => (
              <button
                key={city}
                onClick={() => setSelectedCity(city)}
                className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${selectedCity === city
                  ? "bg-red-600 text-white"
                  : "bg-white text-red-600 border border-red-600 hover:bg-red-600 hover:text-white"
                  }`}
              >
                {cityLabels[city]}
              </button>
            ))}
          </div>
        </div>

        {/* Cinemas Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredCinemas.map((cinema) => (
            <div
              key={cinema.cinemaId}
              className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-xl transition-all duration-300 transform hover:-translate-y-1"
            >
              {/* Cinema Image */}
              <div className="relative h-48 overflow-hidden">
                <img
                  src={cinema.image}
                  alt={cinema.cinemaName}
                  className="w-full h-full object-cover hover:scale-110 transition-transform duration-300"
                />
                <div className="absolute bottom-4 left-4">
                  <span className="bg-red-600 text-white px-3 py-1 rounded-full text-sm font-medium">
                    {cinema.screens} phòng chiếu
                  </span>
                </div>
              </div>

              {/* Cinema Info */}
              <div className="p-6">
                <h3 className="text-xl font-bold text-gray-800 mb-3">
                  {cinema.cinemaName}
                </h3>

                <div className="space-y-2 mb-4">
                  <div className="flex items-start space-x-2 text-gray-600">
                    <MapPin className="h-4 w-4 mt-1 flex-shrink-0" />
                    <span className="text-sm">{cinema.address}</span>
                  </div>
                  <div className="flex items-center space-x-2 text-gray-600">
                    <Phone className="h-4 w-4" />
                    <span className="text-sm">{cinema.phone}</span>
                  </div>
                </div>

                {/* Action Buttons */}
                <div className="flex space-x-2">
                  {/* <button className="flex-1 bg-red-600 hover:bg-red-700 text-white py-2 px-4 rounded-md text-sm font-medium transition-colors">
                    Xem Suất Chiếu
                  </button> */}
                  <button

                    className="px-4 py-2 border border-red-600 text-red-600 hover:bg-red-600 hover:text-white rounded-md text-sm font-medium transition-colors"
                    onClick={() => navigate(`/cinemas/detail/${cinema.cinemaId}`)}>
                    Chi Tiết
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Map Section */}
        <div className="mt-12 bg-white rounded-lg shadow-lg overflow-hidden">
          <div className="p-6">
            <h3 className="text-xl font-bold text-gray-800 mb-4">
              Bản Đồ Hệ Thống Rạp
            </h3>
            <div className="bg-gray-200 h-64 rounded-lg flex items-center justify-center">
              <div className="text-center text-gray-500">
                <MapPin className="h-12 w-12 mx-auto mb-2" />
                <p>Bản đồ tương tác sẽ được hiển thị tại đây</p>
                <p className="text-sm">Tích hợp Google Maps hoặc Mapbox</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default CinemaSection;