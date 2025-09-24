import React, { useState, useEffect } from 'react';
import { MapPin, Phone, Mail, User, Monitor, Armchair, Clock, Calendar } from 'lucide-react';
import { useParams } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { getCinemaDetails } from '../../store/slices/cinemaSlice';

const CinemaDetailsPage: React.FC = () => {
    const [selectedScreenType, setSelectedScreenType] = useState<string>('All');
    const { id } = useParams<{ id: string }>();
    if (id === undefined) return <div>Loading...</div>;
    const dispatch = useAppDispatch();
    const { cinemaDetails, loading, error } = useAppSelector((state) => state.cinema);

    useEffect(() => {
        if (!id) return;
        setTimeout(() => {
            dispatch(getCinemaDetails(id));
        }, 1000);
    }, [id, dispatch]);

    const getStatusColor = (status: string) => {
        switch (status) {
            case 'Active':
                return 'bg-green-50 text-green-700 border-green-200';
            case 'Inactive':
                return 'bg-red-50 text-red-700 border-red-200';
            case 'Under Maintenance':
                return 'bg-yellow-50 text-yellow-700 border-yellow-200';
            default:
                return 'bg-gray-50 text-gray-700 border-gray-200';
        }
    };

    const getScreenTypeColor = (type: string) => {
        switch (type) {
            case 'IMAX':
                return 'bg-purple-50 text-purple-700 border-purple-200';
            case '4DX':
                return 'bg-blue-50 text-blue-700 border-blue-200';
            case 'VIP':
                return 'bg-amber-50 text-amber-700 border-amber-200';
            case 'Premium':
                return 'bg-emerald-50 text-emerald-700 border-emerald-200';
            case 'Standard':
                return 'bg-gray-50 text-gray-700 border-gray-200';
            default:
                return 'bg-gray-50 text-gray-700 border-gray-200';
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
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-gray-700 text-xl">Đang tải thông tin rạp phim...</div>
            </div>
        );
    }

    if (!cinemaDetails || error) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-gray-700 text-xl">Không tìm thấy thông tin rạp phim</div>
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
        <div className="min-h-screen bg-gray-50">
            {/* Hero Section */}
            <div className="bg-white border-b border-gray-200">
                <div className="container mx-auto px-6 py-12">
                    {cinema.image && (
                        <div className="mb-8">
                            <img
                                src={cinema.image}
                                alt={cinema.cinemaName}
                                className="w-full h-64 object-cover rounded-xl border border-gray-200 shadow-lg"
                            />
                        </div>
                    )}
                    <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                        <div>
                            <h1 className="text-4xl lg:text-5xl font-bold text-gray-900 mb-4">{cinema.cinemaName}</h1>
                            <div className="flex items-center gap-2">
                                <span className={`px-4 py-2 rounded-full text-sm font-medium border ${getStatusColor(cinema.status)}`}>
                                    {cinema.status}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div className="container mx-auto px-6 py-12">
                <div className="grid grid-cols-1 lg:grid-cols-4 gap-12">
                    {/* Main Content */}
                    <div className="lg:col-span-3">
                        {/* Cinema Overview Stats */}
                        <section className="mb-12">
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                                <div className="bg-white border border-gray-200 rounded-xl p-8 text-center shadow-sm">
                                    <Monitor className="w-8 h-8 mx-auto mb-3 text-blue-500" />
                                    <div className="text-3xl font-bold text-gray-900 mb-1">{screens.length}</div>
                                    <div className="text-gray-600">Tổng phòng chiếu</div>
                                </div>
                                <div className="bg-white border border-gray-200 rounded-xl p-8 text-center shadow-sm">
                                    <Armchair className="w-8 h-8 mx-auto mb-3 text-blue-500" />
                                    <div className="text-3xl font-bold text-gray-900 mb-1">{totalSeats.toLocaleString()}</div>
                                    <div className="text-gray-600">Tổng ghế</div>
                                </div>
                                <div className="bg-white border border-gray-200 rounded-xl p-8 text-center shadow-sm">
                                    <Clock className="w-8 h-8 mx-auto mb-3 text-blue-500" />
                                    <div className="text-3xl font-bold text-gray-900 mb-1">{activeScreens}</div>
                                    <div className="text-gray-600">Phòng hoạt động</div>
                                </div>
                            </div>
                        </section>

                        {/* Screen Type Filter */}
                        <section className="mb-8">
                            <div className="flex flex-wrap gap-3">
                                <button
                                    onClick={() => setSelectedScreenType('All')}
                                    className={`px-4 py-2 rounded-lg font-medium transition-colors border ${selectedScreenType === 'All'
                                            ? 'bg-blue-50 text-blue-700 border-blue-200'
                                            : 'bg-white text-gray-600 border-gray-300 hover:bg-gray-50'
                                        }`}
                                >
                                    Tất cả ({screens.length})
                                </button>
                                {screenTypes.map(type => (
                                    <button
                                        key={type}
                                        onClick={() => setSelectedScreenType(type)}
                                        className={`px-4 py-2 rounded-lg font-medium transition-colors border ${selectedScreenType === type
                                                ? 'bg-blue-50 text-blue-700 border-blue-200'
                                                : 'bg-white text-gray-600 border-gray-300 hover:bg-gray-50'
                                            }`}
                                    >
                                        {type} ({screens.filter(s => s.type === type).length})
                                    </button>
                                ))}
                            </div>
                        </section>

                        {/* Screens List */}
                        <section>
                            <h2 className="text-2xl font-bold text-gray-900 mb-8">Danh sách phòng chiếu</h2>
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                {filteredScreens.map(screen => (
                                    <div key={screen.id} className="bg-white border border-gray-200 rounded-xl p-6 hover:shadow-md transition-shadow">
                                        <div className="flex justify-between items-start mb-4">
                                            <h3 className="text-xl font-semibold text-gray-900">{screen.screenName}</h3>
                                            <span className={`px-3 py-1 rounded-full text-xs font-medium border ${getStatusColor(screen.status)}`}>
                                                {screen.status}
                                            </span>
                                        </div>

                                        <div className="flex items-center justify-between">
                                            <span className={`px-3 py-1 rounded-full text-sm font-medium border ${getScreenTypeColor(screen.type)}`}>
                                                {screen.type}
                                            </span>
                                            <div className="flex items-center gap-2 text-gray-600">
                                                <Armchair className="w-4 h-4" />
                                                <span className="font-semibold">{screen.seatCount} ghế</span>
                                            </div>
                                        </div>
                                    </div>
                                ))}
                            </div>

                            {filteredScreens.length === 0 && (
                                <div className="text-center py-12 text-gray-500">
                                    <Monitor className="w-16 h-16 mx-auto mb-4 opacity-30" />
                                    <p>Không có phòng chiếu nào với loại đã chọn</p>
                                </div>
                            )}
                        </section>
                    </div>

                    {/* Sidebar */}
                    <div className="space-y-8">
                        {/* Cinema Information */}
                        <section className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
                            <h3 className="text-lg font-bold text-gray-900 mb-6">Thông tin rạp</h3>
                            <div className="space-y-6">
                                <div className="flex items-start gap-3">
                                    <MapPin className="w-5 h-5 text-blue-500 mt-1 flex-shrink-0" />
                                    <div>
                                        <div className="font-medium text-gray-900 mb-1">Địa chỉ</div>
                                        <div className="text-gray-600 text-sm leading-relaxed">{cinema.address}</div>
                                    </div>
                                </div>

                                {cinema.phone && (
                                    <div className="flex items-center gap-3">
                                        <Phone className="w-5 h-5 text-blue-500" />
                                        <div>
                                            <div className="font-medium text-gray-900 mb-1">Điện thoại</div>
                                            <div className="text-gray-600 text-sm">{cinema.phone}</div>
                                        </div>
                                    </div>
                                )}

                                {cinema.email && (
                                    <div className="flex items-center gap-3">
                                        <Mail className="w-5 h-5 text-blue-500" />
                                        <div>
                                            <div className="font-medium text-gray-900 mb-1">Email</div>
                                            <div className="text-gray-600 text-sm">{cinema.email}</div>
                                        </div>
                                    </div>
                                )}

                                <div className="flex items-center gap-3">
                                    <User className="w-5 h-5 text-blue-500" />
                                    <div>
                                        <div className="font-medium text-gray-900 mb-1">Quản lý</div>
                                        <div className="text-gray-600 text-sm">{cinema.managerName}</div>
                                    </div>
                                </div>
                            </div>
                        </section>

                        {/* Screen Type Summary */}
                        <section className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
                            <h3 className="text-lg font-bold text-gray-900 mb-6">Loại phòng chiếu</h3>
                            <div className="space-y-4">
                                {screenTypes.map(type => {
                                    const typeScreens = screens.filter(s => s.type === type);
                                    const typeSeats = typeScreens.reduce((sum, s) => sum + s.seatCount, 0);
                                    return (
                                        <div key={type} className="flex justify-between items-center py-2">
                                            <span className={`px-3 py-1 rounded-full text-xs font-medium border ${getScreenTypeColor(type)}`}>
                                                {type}
                                            </span>
                                            <div className="text-right">
                                                <div className="font-semibold text-gray-900">{typeScreens.length} phòng</div>
                                                <div className="text-gray-500 text-sm">{typeSeats} ghế</div>
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        </section>

                        {/* System Information */}
                        <section className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
                            <h3 className="text-lg font-bold text-gray-900 mb-6">Thông tin hệ thống</h3>
                            <div className="space-y-3 text-sm text-gray-600">
                                {/* Add system information here if needed */}
                            </div>
                        </section>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CinemaDetailsPage;