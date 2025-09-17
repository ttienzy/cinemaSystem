import React, { useState, useEffect } from 'react';
import { MapPin, Phone, Mail, User, Monitor, Armchair, Clock, Calendar } from 'lucide-react';
import { useParams } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { getCinemaDetails } from '../../store/slices/cinemaSlice';

// Interfaces


const CinemaDetailsPage: React.FC = () => {
    const [selectedScreenType, setSelectedScreenType] = useState<string>('All');
    const { id } = useParams<{ id: string }>();
    if (id === undefined) return <div>Loading...</div>;
    const dispatch = useAppDispatch();
    const { cinemaDetails, loading, error } = useAppSelector((state) => state.cinema);

    // Mock data - replace with actual API call
    useEffect(() => {
        // Simulate API call
        if (!id) return;
        setTimeout(() => {
            dispatch(getCinemaDetails(id));
        }, 1000);
    }, [id, dispatch]);

    const getStatusColor = (status: string) => {
        switch (status) {
            case 'Active':
                return 'bg-green-600 text-white';
            case 'Inactive':
                return 'bg-red-600 text-white';
            case 'Under Maintenance':
                return 'bg-yellow-600 text-white';
            default:
                return 'bg-gray-600 text-white';
        }
    };

    const getScreenTypeColor = (type: string) => {
        switch (type) {
            case 'IMAX':
                return 'bg-purple-600 text-white';
            case '4DX':
                return 'bg-blue-600 text-white';
            case 'VIP':
                return 'bg-amber-600 text-white';
            case 'Premium':
                return 'bg-emerald-600 text-white';
            case 'Standard':
                return 'bg-gray-600 text-white';
            default:
                return 'bg-gray-600 text-white';
        }
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString('vi-VN', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-gray-900 flex items-center justify-center">
                <div className="text-white text-xl">Đang tải thông tin rạp phim...</div>
            </div>
        );
    }
    console.log(cinemaDetails);
    if (!cinemaDetails || error) {
        return (
            <div className="min-h-screen bg-gray-900 flex items-center justify-center">
                <div className="text-white text-xl">Không tìm thấy thông tin rạp phim</div>
            </div>
        );
    }

    const { cinema, screens } = cinemaDetails;

    // Filter screens based on selected type
    const filteredScreens = selectedScreenType === 'All'
        ? screens
        : screens.filter(screen => screen.type === selectedScreenType);

    // Statistics
    const totalSeats = screens.reduce((sum, screen) => sum + screen.seatCount, 0);
    const activeScreens = screens.filter(screen => screen.status === 'Active').length;
    const screenTypes = [...new Set(screens.map(screen => screen.type))];

    return (
        <div className="min-h-screen bg-gray-900 text-white">
            {/* Hero Section */}
            <div className="relative h-80 overflow-hidden">
                {cinema.image && (
                    <img
                        src={cinema.image}
                        alt={cinema.cinemaName}
                        className="w-full h-full object-cover"
                    />
                )}
                <div className="absolute inset-0 bg-black bg-opacity-60"></div>
                <div className="absolute inset-0 flex items-center">
                    <div className="container mx-auto px-4">
                        <div className="max-w-3xl">
                            <h1 className="text-4xl md:text-6xl font-bold mb-4">{cinema.cinemaName}</h1>
                            <div className="flex items-center gap-2 mb-4">
                                <span className={`px-4 py-2 rounded-full text-sm font-semibold ${getStatusColor(cinema.status)}`}>
                                    {cinema.status}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div className="container mx-auto px-4 py-8">
                <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
                    {/* Main Content */}
                    <div className="lg:col-span-3">
                        {/* Cinema Overview Stats */}
                        <section className="mb-8">
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                <div className="bg-gray-800 rounded-lg p-6 text-center">
                                    <Monitor className="w-8 h-8 mx-auto mb-2 text-blue-400" />
                                    <div className="text-2xl font-bold">{screens.length}</div>
                                    <div className="text-gray-400">Tổng phòng chiếu</div>
                                </div>
                                <div className="bg-gray-800 rounded-lg p-6 text-center">
                                    <Armchair className="w-8 h-8 mx-auto mb-2 text-green-400" />
                                    <div className="text-2xl font-bold">{totalSeats.toLocaleString()}</div>
                                    <div className="text-gray-400">Tổng ghế</div>
                                </div>
                                <div className="bg-gray-800 rounded-lg p-6 text-center">
                                    <Clock className="w-8 h-8 mx-auto mb-2 text-purple-400" />
                                    <div className="text-2xl font-bold">{activeScreens}</div>
                                    <div className="text-gray-400">Phòng hoạt động</div>
                                </div>
                            </div>
                        </section>

                        {/* Screen Type Filter */}
                        <section className="mb-6">
                            <div className="flex flex-wrap gap-3">
                                <button
                                    onClick={() => setSelectedScreenType('All')}
                                    className={`px-4 py-2 rounded-lg font-medium transition-colors ${selectedScreenType === 'All'
                                        ? 'bg-blue-600 text-white'
                                        : 'bg-gray-700 text-gray-300 hover:bg-gray-600'
                                        }`}
                                >
                                    Tất cả ({screens.length})
                                </button>
                                {screenTypes.map(type => (
                                    <button
                                        key={type}
                                        onClick={() => setSelectedScreenType(type)}
                                        className={`px-4 py-2 rounded-lg font-medium transition-colors ${selectedScreenType === type
                                            ? 'bg-blue-600 text-white'
                                            : 'bg-gray-700 text-gray-300 hover:bg-gray-600'
                                            }`}
                                    >
                                        {type} ({screens.filter(s => s.type === type).length})
                                    </button>
                                ))}
                            </div>
                        </section>

                        {/* Screens List */}
                        <section>
                            <h2 className="text-2xl font-bold mb-6">Danh sách phòng chiếu</h2>
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                {filteredScreens.map(screen => (
                                    <div key={screen.id} className="bg-gray-800 rounded-lg p-6 hover:bg-gray-750 transition-colors">
                                        <div className="flex justify-between items-start mb-4">
                                            <h3 className="text-xl font-semibold">{screen.screenName}</h3>
                                            <span className={`px-2 py-1 rounded text-xs font-medium ${getStatusColor(screen.status)}`}>
                                                {screen.status}
                                            </span>
                                        </div>

                                        <div className="flex items-center justify-between mb-3">
                                            <span className={`px-3 py-1 rounded-full text-sm font-medium ${getScreenTypeColor(screen.type)}`}>
                                                {screen.type}
                                            </span>
                                            <div className="flex items-center gap-2 text-gray-300">
                                                <Armchair className="w-4 h-4" />
                                                <span className="font-semibold">{screen.seatCount} ghế</span>
                                            </div>
                                        </div>

                                    </div>
                                ))}
                            </div>

                            {filteredScreens.length === 0 && (
                                <div className="text-center py-12 text-gray-400">
                                    <Monitor className="w-16 h-16 mx-auto mb-4 opacity-50" />
                                    <p>Không có phòng chiếu nào với loại đã chọn</p>
                                </div>
                            )}
                        </section>
                    </div>

                    {/* Sidebar */}
                    <div className="space-y-6">
                        {/* Cinema Information */}
                        <section className="bg-gray-800 rounded-lg p-6">
                            <h3 className="text-xl font-bold mb-4">Thông tin rạp</h3>
                            <div className="space-y-4">
                                <div className="flex items-start gap-3">
                                    <MapPin className="w-5 h-5 text-red-400 mt-1 flex-shrink-0" />
                                    <div>
                                        <div className="font-medium mb-1">Địa chỉ</div>
                                        <div className="text-gray-300 text-sm">{cinema.address}</div>
                                    </div>
                                </div>

                                {cinema.phone && (
                                    <div className="flex items-center gap-3">
                                        <Phone className="w-5 h-5 text-green-400" />
                                        <div>
                                            <div className="font-medium mb-1">Điện thoại</div>
                                            <div className="text-gray-300 text-sm">{cinema.phone}</div>
                                        </div>
                                    </div>
                                )}

                                {cinema.email && (
                                    <div className="flex items-center gap-3">
                                        <Mail className="w-5 h-5 text-blue-400" />
                                        <div>
                                            <div className="font-medium mb-1">Email</div>
                                            <div className="text-gray-300 text-sm">{cinema.email}</div>
                                        </div>
                                    </div>
                                )}

                                <div className="flex items-center gap-3">
                                    <User className="w-5 h-5 text-yellow-400" />
                                    <div>
                                        <div className="font-medium mb-1">Quản lý</div>
                                        <div className="text-gray-300 text-sm">{cinema.managerName}</div>
                                    </div>
                                </div>
                            </div>
                        </section>

                        {/* Screen Type Summary */}
                        <section className="bg-gray-800 rounded-lg p-6">
                            <h3 className="text-xl font-bold mb-4">Loại phòng chiếu</h3>
                            <div className="space-y-3">
                                {screenTypes.map(type => {
                                    const typeScreens = screens.filter(s => s.type === type);
                                    const typeSeats = typeScreens.reduce((sum, s) => sum + s.seatCount, 0);
                                    return (
                                        <div key={type} className="flex justify-between items-center">
                                            <span className={`px-2 py-1 rounded text-xs font-medium ${getScreenTypeColor(type)}`}>
                                                {type}
                                            </span>
                                            <div className="text-right">
                                                <div className="font-semibold">{typeScreens.length} phòng</div>
                                                <div className="text-gray-400 text-sm">{typeSeats} ghế</div>
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        </section>

                        {/* System Information */}
                        <section className="bg-gray-800 rounded-lg p-6">
                            <h3 className="text-xl font-bold mb-4">Thông tin hệ thống</h3>
                            <div className="space-y-3 text-sm">
                            </div>
                        </section>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CinemaDetailsPage;