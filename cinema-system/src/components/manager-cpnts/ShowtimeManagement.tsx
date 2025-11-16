import React, { useEffect, useMemo } from 'react';
import { Calendar, Clock, Film, MapPin, Users, DollarSign, Edit2, Trash2, Loader2, XCircle, CheckCircle2 } from 'lucide-react';
import type { Showtime } from '../../types/showtime.type';
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { cancelShowtime, confirmShowtime, getShowtimePerformance, updateShowtimeStatus } from '../../store/slices/showtimeSlice';
// import { fetchShowtimes, deleteShowtime } from '../../store/slices/showtimeSlice';

// Giả sử kiểu Showtime của bạn được cập nhật để bao gồm cả status
type ShowtimeWithStatus = Showtime & {
    status: 'Scheduled' | 'Confirmed' | 'Cancelled' | 'Completed';
};


const ShowtimeManagement: React.FC = () => {
    const dispatch = useAppDispatch();

    // Lấy data trực tiếp từ Redux store
    const { showtime, loading, error } = useAppSelector((state) => state.showtime);

    // Fetch data khi component mount
    useEffect(() => {
        // Uncomment khi đã có action
        const cinemaId = localStorage.getItem('cinemaId');
        if (cinemaId != null) {

            dispatch(getShowtimePerformance(cinemaId));
        }
    }, [dispatch]);

    const formatDate = (dateString: string): string => {
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    };

    const formatTime = (timeString: string): string => {
        if (timeString.includes('T')) {
            const date = new Date(timeString);
            return date.toLocaleTimeString('vi-VN', {
                hour: '2-digit',
                minute: '2-digit'
            });
        }
        return timeString;
    };


    const formatCurrency = (amount: number): string => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    };

    const handleEdit = (showtimeId: string): void => {
        console.log('Edit showtime:', showtimeId);
        dispatch(confirmShowtime(showtimeId));
        dispatch(updateShowtimeStatus({ showtimeId, status: 'Confirmed' }));

    };

    const handleDelete = async (showtimeId: string): Promise<void> => {
        if (!window.confirm(`Bạn có chắc muốn hủy suất chiếu này không?`)) {
            return;
        }

        try {
            dispatch(cancelShowtime(showtimeId));
            dispatch(updateShowtimeStatus({ showtimeId, status: 'Cancelled' }));
            alert('Suất chiếu đã được hủy thành công!');
        } catch (err) {
            console.error('Failed to delete showtime:', err);
            alert('Có lỗi xảy ra khi hủy suất chiếu!');
        }
    };

    // Group showtimes by date - sử dụng useMemo để tránh tính toán lại không cần thiết
    const groupedShowtimes = useMemo(() => {
        return (showtime || []).reduce<Record<string, ShowtimeWithStatus[]>>((acc: any, currentShowtime: ShowtimeWithStatus) => {
            const date = formatDate(currentShowtime.showDate);
            if (!acc[date]) {
                acc[date] = [];
            }
            acc[date].push(currentShowtime);
            return acc;
        }, {});
    }, [showtime]);

    // Loading state
    if (loading) {
        return (
            <div className="bg-gray-50 min-h-screen p-6">
                <div className="max-w-7xl mx-auto">
                    <div className="flex items-center justify-center h-64">
                        <div className="text-center">
                            <Loader2 className="w-8 h-8 text-blue-600 animate-spin mx-auto mb-4" />
                            <p className="text-gray-600">Đang tải dữ liệu...</p>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    // Error state
    if (error) {
        return (
            <div className="bg-gray-50 min-h-screen p-6">
                <div className="max-w-7xl mx-auto">
                    <div className="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
                        <p className="text-red-800 font-medium mb-2">Có lỗi xảy ra khi tải dữ liệu</p>
                        <p className="text-red-600 text-sm">{error}</p>
                        <button
                            onClick={() => window.location.reload()}
                            className="mt-4 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
                        >
                            Tải lại trang
                        </button>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="bg-gray-50 min-h-screen p-6">
            <div className="max-w-7xl mx-auto">
                <div className="mb-6">
                    <h1 className="text-2xl font-bold text-gray-900 mb-2">Quản lý Suất Chiếu</h1>
                    <p className="text-gray-600">Chỉnh sửa và hủy các suất chiếu đã lên lịch</p>
                </div>

                {Object.entries(groupedShowtimes).length > 0 ? (
                    Object.entries(groupedShowtimes).map(([date, showtimesForDate]) => (
                        <div key={date} className="mb-8">
                            <div className="flex items-center gap-2 mb-4">
                                <Calendar className="w-5 h-5 text-blue-600" />
                                <h2 className="text-lg font-semibold text-gray-900">{date}</h2>
                                <span className="text-sm text-gray-500">({showtimesForDate.length} suất chiếu)</span>
                            </div>

                            <div className="grid gap-4">
                                {showtimesForDate.map((showtime: Showtime) => (
                                    <div
                                        key={showtime.showtimeId}
                                        className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 hover:shadow-md transition-shadow"
                                    >
                                        <div className="flex items-start justify-between">
                                            <div className="flex-1">
                                                <div className="flex items-start gap-4">
                                                    <div className="flex-1">
                                                        <div className="flex items-center gap-3 mb-3 flex-wrap">
                                                            <Film className="w-5 h-5 text-blue-600" />
                                                            <h3 className="text-lg font-semibold text-gray-900">
                                                                {showtime.title}
                                                            </h3>
                                                            {/* PHẦN CODE MỚI THÊM VÀO */}
                                                            <span
                                                                className={`px-2 py-1 rounded text-xs font-medium ${showtime.status === 'Confirmed'
                                                                    ? 'bg-green-100 text-green-700'
                                                                    : showtime.status === 'Scheduled'
                                                                        ? 'bg-blue-100 text-blue-700'
                                                                        : showtime.status === 'Cancelled'
                                                                            ? 'bg-red-100 text-red-700'
                                                                            : showtime.status === 'Completed'
                                                                                ? 'bg-gray-200 text-gray-800'
                                                                                : 'bg-gray-100 text-gray-700'
                                                                    }`}
                                                            >
                                                                {showtime.status}
                                                            </span>
                                                            {/* KẾT THÚC PHẦN CODE MỚI */}
                                                            <span className={`px-2 py-1 rounded text-xs font-medium ${showtime.screenType === 'IMAX'
                                                                ? 'bg-purple-100 text-purple-700'
                                                                : showtime.screenType === 'VIP'
                                                                    ? 'bg-amber-100 text-amber-700'
                                                                    : 'bg-gray-100 text-gray-700'
                                                                }`}>
                                                                {showtime.screenType}
                                                            </span>
                                                        </div>

                                                        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 text-sm">
                                                            <div className="flex items-center gap-2">
                                                                <MapPin className="w-4 h-4 text-gray-400" />
                                                                <div>
                                                                    <span className="text-gray-500">Phòng:</span>
                                                                    <span className="ml-1 font-medium text-gray-900">
                                                                        {showtime.screenName}
                                                                    </span>
                                                                </div>
                                                            </div>

                                                            <div className="flex items-center gap-2">
                                                                <Clock className="w-4 h-4 text-gray-400" />
                                                                <div>
                                                                    <span className="text-gray-500">Giờ chiếu:</span>
                                                                    <span className="ml-1 font-medium text-gray-900">
                                                                        {formatTime(showtime.actualStartTime)} - {formatTime(showtime.actualEndTime)}
                                                                    </span>
                                                                </div>
                                                            </div>

                                                            <div className="flex items-center gap-2">
                                                                <Users className="w-4 h-4 text-gray-400" />
                                                                <div>
                                                                    <span className="text-gray-500">Đã đặt:</span>
                                                                    <span className="ml-1 font-medium text-gray-900">
                                                                        {showtime.totalBookings} vé
                                                                    </span>
                                                                </div>
                                                            </div>

                                                            <div className="flex items-center gap-2">
                                                                <DollarSign className="w-4 h-4 text-gray-400" />
                                                                <div>
                                                                    <span className="text-gray-500">Giá TB:</span>
                                                                    <span className="ml-1 font-medium text-gray-900">
                                                                        {formatCurrency(showtime.avgTicketPrice)}
                                                                    </span>
                                                                </div>
                                                            </div>
                                                        </div>

                                                        <div className="mt-3 flex items-center gap-4 text-xs text-gray-500">
                                                            <span>Khung giờ: {showtime.slotStartTime} - {showtime.slotEndTime}</span>
                                                            <span>•</span>
                                                            <span>Bậc giá: {showtime.pricingTier}</span>
                                                            <span>•</span>
                                                            <span>Hệ số: x{showtime.multiplier}</span>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>

                                            <div className="flex gap-2 ml-4">
                                                <button
                                                    onClick={() => handleEdit(showtime.showtimeId)}
                                                    className="flex items-center gap-1 px-3 py-2 bg-green-100 text-green-700 hover:bg-green-200 rounded-lg transition-colors"
                                                    title="Xác nhận suất chiếu"
                                                >
                                                    <CheckCircle2 className="w-5 h-5" />
                                                    <span className="text-sm font-medium hidden sm:inline">Xác nhận</span>
                                                </button>
                                                <button
                                                    onClick={() => handleDelete(showtime.showtimeId)}
                                                    className="flex items-center gap-1 px-3 py-2 bg-red-100 text-red-700 hover:bg-red-200 rounded-lg transition-colors"
                                                    title="Hủy suất chiếu"
                                                >
                                                    <XCircle className="w-5 h-5" />
                                                    <span className="text-sm font-medium hidden sm:inline">Hủy</span>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    ))
                ) : (
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-12 text-center">
                        <Calendar className="w-12 h-12 text-gray-400 mx-auto mb-4" />
                        <h3 className="text-lg font-medium text-gray-900 mb-2">
                            Chưa có suất chiếu nào
                        </h3>
                        <p className="text-gray-600">
                            Hiện tại không có suất chiếu nào được lên lịch
                        </p>
                    </div>
                )}
            </div>
        </div>
    );
};

export default ShowtimeManagement;