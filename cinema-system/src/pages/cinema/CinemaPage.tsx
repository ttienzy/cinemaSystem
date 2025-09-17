import React, { use, useEffect, useState } from 'react';
import { MapPin, Phone, ScreenShare } from 'lucide-react';
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { useNavigate } from 'react-router-dom';
import type { Cinema } from '../../types/cinema.types';
import { getCinemas } from '../../store/slices/cinemaSlice';



const CinemaPage: React.FC = () => {
    const [searchTerm, setSearchTerm] = useState('');
    const { cinemas } = useAppSelector(state => state.cinema);
    const navigate = useNavigate();
    const dispatch = useAppDispatch();

    useEffect(() => {
        dispatch(getCinemas());
    }, [dispatch]);

    const filteredCinemas = cinemas.filter(cinema =>
        cinema.cinemaName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        cinema.address.toLowerCase().includes(searchTerm.toLowerCase())
    );

    const handleCinemaClick = (cinema: Cinema) => {
        // You can replace this with actual navigation or modal opening
        navigate(`/cinemas/detail/${cinema.cinemaId}`);
    };

    return (
        <div className="min-h-screen bg-gray-50 py-8 px-4">
            <div className="max-w-7xl mx-auto">
                {/* Header */}
                <div className="mb-8">
                    <h1 className="text-3xl font-bold text-gray-900 mb-2">Quản Lý Rạp Phim</h1>
                    <p className="text-gray-600">Danh sách các rạp phim</p>
                </div>

                {/* Search */}
                <div className="bg-white rounded-lg shadow-sm p-6 mb-8">
                    <div className="relative max-w-xl">
                        <MapPin className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                        <input
                            type="text"
                            placeholder="Tìm kiếm rạp phim theo tên hoặc địa chỉ..."
                            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                    </div>

                    {/* Results count */}
                    <div className="mt-4 text-sm text-gray-600">
                        Hiển thị {filteredCinemas.length} trong tổng số {cinemas.length} rạp
                    </div>
                </div>

                {/* Cinemas Grid */}
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {filteredCinemas.map((cinema) => (
                        <div
                            key={cinema.cinemaId}
                            className="bg-white rounded-xl shadow-md overflow-hidden hover:shadow-xl transition-shadow duration-300 cursor-pointer"
                            onClick={() => handleCinemaClick(cinema)}
                        >
                            {/* Image */}
                            <img
                                src={cinema.image}
                                alt={cinema.cinemaName}
                                className="w-full h-48 object-cover"
                            />

                            {/* Cinema Info */}
                            <div className="p-5">
                                <h3 className="text-xl font-semibold text-gray-900 mb-2">
                                    {cinema.cinemaName}
                                </h3>

                                <div className="space-y-2 text-sm text-gray-500">
                                    {/* Address */}
                                    <div className="flex items-start">
                                        <MapPin className="w-4 h-4 mr-2 mt-1 flex-shrink-0" />
                                        <span>{cinema.address}</span>
                                    </div>

                                    {/* Phone */}
                                    <div className="flex items-center">
                                        <Phone className="w-4 h-4 mr-2" />
                                        {cinema.phone}
                                    </div>

                                    {/* Screens */}
                                    <div className="flex items-center">
                                        <ScreenShare className="w-4 h-4 mr-2" />
                                        {cinema.screens} phòng chiếu
                                    </div>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>

                {/* Empty State */}
                {filteredCinemas.length === 0 && (
                    <div className="text-center py-12">
                        <div className="text-gray-500 text-lg mb-2">Không tìm thấy rạp phim nào</div>
                        <div className="text-gray-400 text-sm">Thử thay đổi từ khóa tìm kiếm</div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default CinemaPage;