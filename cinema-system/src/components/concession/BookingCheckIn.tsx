// src/components/concession/BookingCheckIn.tsx

import React, { useState, type KeyboardEvent } from 'react';
import { Search, Ticket, XCircle } from 'lucide-react';

// --- HOOKS & ACTIONS ---
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { checkInBooking, confirmBookingCheckIn } from '../../store/slices/bookingSlice';

// --- REUSABLE COMPONENTS ---
import { BookingDetails } from '../booking/BookingDetails';
import { LoadingSpinner, InitialState } from '../common/StateDisplays';

// =================================================================
// MAIN COMPONENT
// =================================================================
const BookingCheckIn: React.FC = () => {
    // --- STATE MANAGEMENT ---
    const [bookingCode, setBookingCode] = useState('');
    const [clientError, setClientError] = useState('');

    // --- REDUX HOOKS ---
    const dispatch = useAppDispatch();
    const { checkedIn, loading, error: apiError } = useAppSelector(state => state.booking);

    // --- EVENT HANDLERS ---
    const handleSearch = () => {
        if (!bookingCode.trim()) {
            setClientError('Vui lòng nhập mã vé.');
            return;
        }
        setClientError('');
        dispatch(checkInBooking(bookingCode));
    };

    const handleKeyPress = (e: KeyboardEvent) => {
        if (e.key === 'Enter') {
            handleSearch();
        }
    };

    const handleCheckIn = async () => {
        if (!checkedIn) return;

        try {
            await dispatch(confirmBookingCheckIn(bookingCode)).unwrap();
            dispatch(checkInBooking(bookingCode)); // Tải lại dữ liệu mới nhất
        } catch (error) {
            console.error('Failed to confirm check-in:', error);
            alert('Đã xảy ra lỗi trong quá trình check-in. Vui lòng thử lại.');
        }
    };

    // --- RENDER ---
    return (
        <div className="w-full">
            {/* Page Header (Tùy chọn: có thể giữ lại hoặc bỏ nếu trang cha đã có tiêu đề) */}
            <div className="text-center mb-6">
                <h1 className="text-2xl font-bold text-gray-800">Check-in thủ công</h1>
                <p className="text-gray-500 mt-1">Nhập mã vé để truy xuất thông tin chi tiết và check-in.</p>
            </div>

            {/* Search Section */}
            <div className="mb-6">
                <div className="flex flex-col sm:flex-row gap-3">
                    <div className="flex-grow relative">
                        <Ticket className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
                        <input
                            type="text"
                            value={bookingCode}
                            onChange={(e) => setBookingCode(e.target.value.toUpperCase())}
                            onKeyPress={handleKeyPress}
                            placeholder="Nhập mã vé (ví dụ: ZN8C2)"
                            className="pl-12 w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-base"
                        />
                    </div>
                    <button
                        onClick={handleSearch}
                        disabled={loading}
                        className="bg-indigo-600 text-white px-6 py-3 rounded-lg hover:bg-indigo-700 disabled:opacity-60 disabled:cursor-not-allowed flex items-center justify-center gap-2 font-semibold transition-colors"
                    >
                        <Search className="h-5 w-5" />
                        {loading ? 'Đang tìm...' : 'Tìm kiếm'}
                    </button>
                </div>
                {(clientError || apiError) && (
                    <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-center gap-3 text-sm">
                        <XCircle className="h-5 w-5 text-red-500 flex-shrink-0" />
                        <span className="text-red-700">{clientError || apiError}</span>
                    </div>
                )}
            </div>

            {/* Results Section */}
            <div className="mt-8">
                {loading && <LoadingSpinner />}

                {!loading && checkedIn && (
                    <BookingDetails data={checkedIn} onCheckIn={handleCheckIn} />
                )}

                {/* SỬ DỤNG COMPONENT INITIALSTATE ĐÃ TÁI CẤU TRÚC */}
                {!loading && !checkedIn && !apiError && (
                    <InitialState
                        title="Sẵn sàng để tìm vé của bạn"
                        message="Nhập mã từ email xác nhận của bạn để bắt đầu."
                    />
                )}
            </div>
        </div>
    );
};

export default BookingCheckIn;