import React, { useState, useEffect } from 'react';
import { Calendar, Clock, Film, MapPin, DollarSign, Monitor, Save, X, AlertCircle, CheckCircle, Trash2, Plus } from 'lucide-react';
import type { Screen, Slot, PricingTier, Movie, ShowtimePricingInfoRequest } from '../../types/showtime.type';
import type { InputShowtimeDto } from '../../types/showtime.type';
import { useAppSelector, useAppDispatch } from '../../hooks/redux';
import { getShowtimeSetUpData, postShowtimeForm } from '../../store/slices/showtimeSlice';
import { formatDateTimeLocal } from '../../utils/format';


interface Notification {
    show: boolean;
    type: 'success' | 'error' | '';
    message: string;
}

// Mock data


const CreateShowtime: React.FC = () => {
    const [formData, setFormData] = useState<InputShowtimeDto>({
        movieId: '',
        cinemaId: '',
        screenId: '',
        slotId: '',
        pricingTierId: '',
        showDate: '',
        actualStartTime: '',
        actualEndTime: '',
        status: 'Scheduled',
        showtimePricings: []
    });

    const [notification, setNotification] = useState<Notification>({
        show: false,
        type: '',
        message: ''
    });

    const dispatch = useAppDispatch();
    const { showtimeSetUpData } = useAppSelector(state => state.showtime);


    // Get cinemaId from state/memory on component mount
    useEffect(() => {
        // Simulating getting cinemaId from state/context
        // In real app, this would come from React Context, Redux, or props
        const storedCinemaId = localStorage.getItem('cinemaId');
        if (storedCinemaId === null) {
            setNotification({
                show: true,
                type: 'error',
                message: 'Cinema ID not found'
            });
            return;
        }
        dispatch(getShowtimeSetUpData(storedCinemaId));
        setFormData(prev => ({ ...prev, cinemaId: storedCinemaId }));
    }, []);

    const formatDateTimeLocal = (date: Date): string => {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        return `${year}-${month}-${day}T${hours}:${minutes}`;
    };

    const handleInputChange = (field: keyof InputShowtimeDto, value: string): void => {
        setFormData(prev => ({ ...prev, [field]: value }));

        // Auto calculate end time based on movie duration
        if (field === 'actualStartTime' && formData.movieId) {
            const movie = showtimeSetUpData?.movies.find(m => m.movieId === formData.movieId);
            if (movie) {
                const startTime = new Date(value);
                const endTime = new Date(startTime.getTime() + movie.duration * 60000);

                console.log('Start:', startTime); // Debug
                console.log('End:', endTime);     // Debug
                console.log('Formatted:', formatDateTimeLocal(endTime)); // Debug

                setFormData(prev => ({ ...prev, actualEndTime: formatDateTimeLocal(endTime) }));
            }
        }
    };
    const handleAddSeatPricing = (): void => {
        const availableSeatTypes = showtimeSetUpData?.seatTypes.filter(
            st => !formData.showtimePricings.some(sp => sp.seatTypeId === st.seatTypeId)
        );

        if (availableSeatTypes && availableSeatTypes.length > 0) {
            setFormData(prev => ({
                ...prev,
                showtimePricings: [
                    ...prev.showtimePricings,
                    { seatTypeId: '', basePrice: 0 }
                ]
            }));
        } else {
            setNotification({
                show: true,
                type: 'error',
                message: 'Đã thêm tất cả các loại ghế!'
            });
            setTimeout(() => setNotification({ show: false, type: '', message: '' }), 3000);
        }
    };

    const handleRemoveSeatPricing = (index: number): void => {
        setFormData(prev => ({
            ...prev,
            showtimePricings: prev.showtimePricings.filter((_, i) => i !== index)
        }));
    };

    const handleSeatPricingChange = (index: number, field: keyof ShowtimePricingInfoRequest, value: string | number): void => {
        setFormData(prev => ({
            ...prev,
            showtimePricings: prev.showtimePricings.map((pricing, i) =>
                i === index ? { ...pricing, [field]: value } : pricing
            )
        }));
    };
    //const activeScreens = showtimeSetUpData?.screens.filter(s => s.isActive);

    const getAvailableSeatTypes = (currentIndex: number) => {
        const selectedIds = formData.showtimePricings
            .map((p, i) => i !== currentIndex ? p.seatTypeId : null)
            .filter(Boolean);
        return showtimeSetUpData?.seatTypes.filter(st => !selectedIds.includes(st.seatTypeId)) || [];
    };

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>): void => {
        e.preventDefault();

        // Validate required fields
        const requiredFields: (keyof InputShowtimeDto)[] = [
            'movieId', 'cinemaId', 'screenId', 'slotId',
            'pricingTierId', 'showDate', 'actualStartTime', 'actualEndTime'
        ];
        const missingFields = requiredFields.filter(field => !formData[field]);

        if (missingFields.length > 0) {
            setNotification({
                show: true,
                type: 'error',
                message: 'Vui lòng điền đầy đủ thông tin bắt buộc!'
            });
            setTimeout(() => setNotification({ show: false, type: '', message: '' }), 3000);
            return;
        }
        const invalidPricing = formData.showtimePricings.some(
            p => !p.seatTypeId || p.basePrice <= 0
        );

        if (invalidPricing) {
            setNotification({
                show: true,
                type: 'error',
                message: 'Vui lòng chọn loại ghế và nhập giá hợp lệ cho tất cả các dòng!'
            });
            setTimeout(() => setNotification({ show: false, type: '', message: '' }), 3000);
            return;
        }


        console.log('Submitting:', formData);
        // Here you would dispatch an action or call a service to submit the form data
        dispatch(postShowtimeForm(formData));

        setNotification({
            show: true,
            type: 'success',
            message: 'Tạo lịch chiếu thành công!'
        });

    };

    const handleReset = (): void => {
        const currentCinemaId = formData.cinemaId;
        setFormData({
            movieId: '',
            cinemaId: currentCinemaId,
            screenId: '',
            slotId: '',
            pricingTierId: '',
            showDate: '',
            actualStartTime: '',
            actualEndTime: '',
            status: 'Scheduled',
            showtimePricings: []
        });
    };

    const activeScreens = showtimeSetUpData?.screens.filter(s => s.isActive);

    return (
        <div className="min-h-screen bg-gray-50 p-6">
            {/* Notification */}
            {notification.show && (
                <div className={`fixed top-4 right-4 z-50 flex items-center gap-3 px-6 py-4 rounded-lg shadow-lg ${notification.type === 'success'
                    ? 'bg-green-50 border border-green-200'
                    : 'bg-red-50 border border-red-200'
                    }`}>
                    {notification.type === 'success' ? (
                        <CheckCircle className="w-5 h-5 text-green-600" />
                    ) : (
                        <AlertCircle className="w-5 h-5 text-red-600" />
                    )}
                    <span className={notification.type === 'success' ? 'text-green-800' : 'text-red-800'}>
                        {notification.message}
                    </span>
                </div>
            )}

            <div className="max-w-6xl mx-auto">
                {/* Header */}
                <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
                    <div className="flex items-center gap-3">
                        <div className="w-12 h-12 bg-blue-600 rounded-lg flex items-center justify-center">
                            <Calendar className="w-6 h-6 text-white" />
                        </div>
                        <div>
                            <h1 className="text-2xl font-bold text-gray-900">Tạo Lịch Chiếu Mới</h1>
                            <p className="text-gray-600 text-sm mt-1">
                                Quản lý lịch chiếu phim cho các phòng chiếu
                            </p>
                        </div>
                    </div>
                </div>

                {/* Form */}
                <form onSubmit={handleSubmit}>
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-8">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

                            {/* Movie Selection */}
                            <div className="space-y-2">
                                <label className="flex items-center gap-2 text-sm font-medium text-gray-700">
                                    <Film className="w-4 h-4 text-blue-600" />
                                    Phim <span className="text-red-500">*</span>
                                </label>
                                <select
                                    value={formData.movieId}
                                    onChange={(e) => handleInputChange('movieId', e.target.value)}
                                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                    required
                                    title="Chọn phim để tạo lịch chiếu"
                                >
                                    <option value="">Chọn phim</option>
                                    {showtimeSetUpData?.movies.map(movie => (
                                        <option key={movie.movieId} value={movie.movieId}>
                                            {movie.title} ({movie.duration} phút)
                                        </option>
                                    ))}
                                </select>
                            </div>



                            {/* Screen Selection */}
                            <div className="space-y-2">
                                <label className="flex items-center gap-2 text-sm font-medium text-gray-700">
                                    <Monitor className="w-4 h-4 text-blue-600" />
                                    Phòng Chiếu <span className="text-red-500">*</span>
                                </label>
                                <select
                                    value={formData.screenId}
                                    onChange={(e) => handleInputChange('screenId', e.target.value)}
                                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                    required
                                    title="Chọn phòng chiếu"
                                >
                                    <option value="">Chọn phòng chiếu</option>
                                    {activeScreens?.map(screen => (
                                        <option key={screen.screenId} value={screen.screenId}>
                                            {screen.screenName} - {screen.screenType} ({screen.seatCapacity} ghế)
                                        </option>
                                    ))}
                                </select>
                            </div>

                            {/* Slot Selection */}
                            <div className="space-y-2">
                                <label className="flex items-center gap-2 text-sm font-medium text-gray-700">
                                    <Clock className="w-4 h-4 text-blue-600" />
                                    Khung Giờ <span className="text-red-500">*</span>
                                </label>
                                <select
                                    value={formData.slotId}
                                    onChange={(e) => handleInputChange('slotId', e.target.value)}
                                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                    required
                                    title="Chọn khung giờ chiếu"
                                >
                                    <option value="">Chọn khung giờ</option>
                                    {showtimeSetUpData?.slots.map(slot => (
                                        <option key={slot.slotId} value={slot.slotId}>
                                            {slot.startTime} - {slot.endTime} {slot.isPeakTime && '(Giờ cao điểm)'}
                                        </option>
                                    ))}
                                </select>
                            </div>

                            {/* Pricing Tier */}
                            <div className="space-y-2">
                                <label className="flex items-center gap-2 text-sm font-medium text-gray-700">
                                    <DollarSign className="w-4 h-4 text-blue-600" />
                                    Bảng Giá <span className="text-red-500">*</span>
                                </label>
                                <select
                                    value={formData.pricingTierId}
                                    onChange={(e) => handleInputChange('pricingTierId', e.target.value)}
                                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                    required
                                    title="Chọn bảng giá áp dụng"
                                >
                                    <option value="">Chọn bảng giá</option>
                                    {showtimeSetUpData?.pricingTiers.map(tier => (
                                        <option key={tier.pricingTierId} value={tier.pricingTierId}>
                                            {tier.tierName} (x{tier.multiplier}) - {tier.description}
                                        </option>
                                    ))}
                                </select>
                            </div>

                            {/* Show Date */}
                            <div className="space-y-2">
                                <label className="flex items-center gap-2 text-sm font-medium text-gray-700">
                                    <Calendar className="w-4 h-4 text-blue-600" />
                                    Ngày Chiếu <span className="text-red-500">*</span>
                                </label>
                                <input
                                    type="date"
                                    value={formData.showDate}
                                    onChange={(e) => handleInputChange('showDate', e.target.value)}
                                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                    required
                                    title="Chọn ngày chiếu phim"
                                />
                            </div>

                            {/* Start Time */}
                            <div className="space-y-2">
                                <label className="flex items-center gap-2 text-sm font-medium text-gray-700">
                                    <Clock className="w-4 h-4 text-blue-600" />
                                    Giờ Bắt Đầu <span className="text-red-500">*</span>
                                </label>
                                <input
                                    type="datetime-local"
                                    value={formData.actualStartTime}
                                    onChange={(e) => handleInputChange('actualStartTime', e.target.value)}
                                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                    required
                                    title="Chọn thời gian bắt đầu"
                                />
                            </div>

                            {/* End Time */}
                            <div className="space-y-2">
                                <label className="flex items-center gap-2 text-sm font-medium text-gray-700">
                                    <Clock className="w-4 h-4 text-blue-600" />
                                    Giờ Kết Thúc <span className="text-red-500">*</span>
                                </label>
                                <input
                                    type="datetime-local"
                                    value={formData.actualEndTime}
                                    onChange={(e) => handleInputChange('actualEndTime', e.target.value)}
                                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                    required
                                    readOnly
                                    title="Chọn thời gian kết thúc"
                                />
                            </div>

                            {/* Status */}
                            <div className="space-y-2">
                                <label className="flex items-center gap-2 text-sm font-medium text-gray-700">
                                    Trạng Thái <span className="text-red-500">*</span>
                                </label>
                                <select
                                    value={formData.status}
                                    onChange={(e) => handleInputChange('status', e.target.value)}
                                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                    required
                                    title="Chọn trạng thái lịch chiếu"
                                >
                                    <option value="Scheduled">Đã lên lịch</option>
                                    <option value="Confirmed">Đang hoạt động</option>
                                    <option value="Cancelled">Đã hủy</option>
                                    <option value="Completed">Hoàn thành</option>
                                </select>
                            </div>
                        </div>
                        {/* Seat Pricing Section */}
                        <div className="mt-8 pt-6 border-t border-gray-200">
                            <div className="flex items-center justify-between mb-4">
                                <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                                    <DollarSign className="w-5 h-5 text-blue-600" />
                                    Giá Theo Loại Ghế <span className="text-red-500">*</span>
                                </h3>
                                <button
                                    type="button"
                                    onClick={handleAddSeatPricing}
                                    className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors text-sm"
                                >
                                    <Plus className="w-4 h-4" />
                                    Thêm Loại Ghế
                                </button>
                            </div>

                            {formData.showtimePricings.length === 0 ? (
                                <div className="text-center py-8 bg-gray-50 rounded-lg border-2 border-dashed border-gray-300">
                                    <DollarSign className="w-12 h-12 text-gray-400 mx-auto mb-3" />
                                    <p className="text-gray-600">Chưa có loại ghế nào được thêm</p>
                                    <p className="text-sm text-gray-500 mt-1">Nhấn "Thêm Loại Ghế" để bắt đầu</p>
                                </div>
                            ) : (
                                <div className="space-y-3">
                                    {formData.showtimePricings.map((pricing, index) => (
                                        <div key={index} className="flex items-center gap-3 p-4 bg-gray-50 rounded-lg border border-gray-200">
                                            <div className="flex-1">
                                                <select
                                                    title='select-seat-types'
                                                    value={pricing.seatTypeId}
                                                    onChange={(e) => handleSeatPricingChange(index, 'seatTypeId', e.target.value)}
                                                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                                    required
                                                >
                                                    <option value="">Chọn loại ghế</option>
                                                    {getAvailableSeatTypes(index).map(seatType => (
                                                        <option key={seatType.seatTypeId} value={seatType.seatTypeId}>
                                                            {seatType.seatTypeName} (x{seatType.multiplier})
                                                        </option>
                                                    ))}
                                                </select>
                                            </div>
                                            <div className="flex-1">
                                                <div className="relative">
                                                    <input
                                                        type="number"
                                                        value={pricing.basePrice || ''}
                                                        onChange={(e) => handleSeatPricingChange(index, 'basePrice', Number(e.target.value))}
                                                        placeholder="Nhập giá cơ bản"
                                                        min="0"
                                                        step="1000"
                                                        className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                                        required
                                                    />
                                                    <span className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-500 text-sm">VNĐ</span>
                                                </div>
                                            </div>
                                            <button
                                                type="button"
                                                onClick={() => handleRemoveSeatPricing(index)}
                                                className="p-2.5 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                                                title="Xóa loại ghế này"
                                            >
                                                <Trash2 className="w-5 h-5" />
                                            </button>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>

                        {/* Action Buttons */}
                        <div className="mt-8 flex items-center justify-end gap-4 pt-6 border-t border-gray-200">
                            <button
                                type="button"
                                onClick={handleReset}
                                className="flex items-center gap-2 px-6 py-2.5 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
                            >
                                <X className="w-4 h-4" />
                                Đặt Lại
                            </button>
                            <button
                                type="submit"
                                className="flex items-center gap-2 px-6 py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors shadow-sm"
                            >
                                <Save className="w-4 h-4" />
                                Tạo Lịch Chiếu
                            </button>
                        </div>
                    </div>
                </form>

                {/* Info Box */}
                <div className="mt-6 bg-blue-50 border border-blue-200 rounded-lg p-6">
                    <div className="flex gap-3">
                        <AlertCircle className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" />
                        <div className="space-y-2 text-sm text-blue-800">
                            <p className="font-medium">Lưu ý khi tạo lịch chiếu:</p>
                            <ul className="list-disc list-inside space-y-1 ml-2">
                                <li>Kiểm tra kỹ thời gian để tránh trùng lặp với các suất chiếu khác</li>
                                <li>Giờ kết thúc sẽ tự động tính dựa trên thời lượng phim</li>
                                <li>Chỉ các phòng chiếu đang hoạt động mới hiển thị trong danh sách</li>
                                <li>Bảng giá áp dụng theo từng khung giờ và loại ngày</li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CreateShowtime;