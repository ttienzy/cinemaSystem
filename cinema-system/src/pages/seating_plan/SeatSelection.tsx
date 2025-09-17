import React, { useState, useMemo, useEffect } from 'react';
import { Calendar, Clock, MapPin, User, CreditCard, X } from 'lucide-react';
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import type { PaymentInfoRequest, Seat, SelectedSeat } from '../../types/showtime.type';
import { jwtDecode } from 'jwt-decode';
import type { DecodedToken } from '../../types/auth.types';
import { useNavigate, useParams } from 'react-router-dom';
import { getPaymentUrl, getShowtimeSeatingPlan } from '../../store/slices/showtimeSlice';
import { useSignalR } from '../../contexts/SignalRContext';


const SeatSelection: React.FC = () => {
    const token = localStorage.getItem('accessToken');
    const decodedToken = token ? jwtDecode<DecodedToken>(token) : null;
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const { connection, isConnected, isCompleted, setIsCompleted } = useSignalR();
    const [selectedSeats, setSelectedSeats] = useState<string[]>([]);
    const [showConfirmation, setShowConfirmation] = useState(false);

    const { showtimeInfo, pricings, seats, loading, error } = useAppSelector(state => state.showtime);
    const { id: showtimeId } = useParams<{ id: string }>();
    console.log('Showtime ID:', showtimeId);


    useEffect(() => {
        if (showtimeId) {
            dispatch(getShowtimeSeatingPlan(showtimeId));
        }
    }, [showtimeId, dispatch]);
    useEffect(() => {
        if (isConnected && connection && showtimeId) {
            // Tham gia group khi component được mount và kết nối sẵn sàng
            connection.invoke("JoinShowtimeGroup", showtimeId)
                .catch(err => console.error("Failed to join group: ", err));

            // // Rời group khi component unmount
            // return () => {
            //     connection.invoke("LeaveShowtimeGroup", showtimeId)
            //         .catch(err => console.error("Failed to leave group: ", err));
            // }
        }
    }, [isConnected, connection, showtimeId]);
    // Group seats by row
    const seatsByRow = useMemo(() => {
        const grouped = seats.reduce((acc, seat) => {
            if (!acc[seat.rowName]) {
                acc[seat.rowName] = [];
            }
            acc[seat.rowName].push(seat);
            return acc;
        }, {} as Record<string, Seat[]>);

        // Sort seats within each row
        Object.keys(grouped).forEach(row => {
            grouped[row].sort((a, b) => a.number - b.number);
        });

        return grouped;
    }, [seats]);

    // Get price for a seat
    const getSeatPrice = (seatTypeId: string): number => {
        const pricing = pricings.find(p => p.seatTypeId === seatTypeId);
        return pricing?.finalPrice || 0;
    };

    // Handle seat selection
    const handleSeatClick = (seat: Seat): void => {
        if (seat.status === 'Reserved') return;
        if (seat.status === 'Booked') return;
        setSelectedSeats(prev => {
            if (prev.includes(seat.id)) {
                return prev.filter(id => id !== seat.id);
            } else {
                return [...prev, seat.id];
            }
        });
    };

    // Get seat type color
    const getSeatTypeColor = (seatTypeName: string, status: string, isSelected: boolean): string => {
        if (status === 'Reserved') return 'bg-red-500 cursor-not-allowed';
        if (status === 'Booked') return 'bg-gray-500 cursor-not-allowed';
        if (isSelected) return 'bg-green-500 border-green-600';

        switch (seatTypeName) {
            case 'Standard': return 'bg-blue-500 hover:bg-blue-600 border-blue-600';
            case 'VIP': return 'bg-yellow-500 hover:bg-yellow-600 border-yellow-600';
            case 'Couple': return 'bg-pink-500 hover:bg-pink-600 border-pink-600';
            default: return 'bg-gray-500 hover:bg-gray-600 border-gray-600';
        }
    };

    // Get seat width class
    const getSeatWidth = (seatTypeName: string): string => {
        return seatTypeName === 'Couple' ? 'w-16' : 'w-8';
    };

    // Calculate total price
    const totalPrice = selectedSeats.reduce((total, seatId) => {
        const seat = seats.find(s => s.id === seatId);
        if (seat) {
            return total + getSeatPrice(seat.seatTypeId);
        }
        return total;
    }, 0);

    // Handle booking confirmation
    const handleConfirmBooking = async () => {
        if (!decodedToken) {
            navigate("/login");
            return;
        }

        const userId = decodedToken.nameid;
        if (!userId) {
            navigate("/login");
            return;
        }

        const selectedSeatDetails: SelectedSeat[] = selectedSeats.map((seatId) => {
            const seat = seats.find((s) => s.id === seatId)!;
            return {
                seatId: seat.id,
                price: getSeatPrice(seat.seatTypeId),
            };
        });
        if (isConnected && connection && selectedSeats.length > 0) {
            try {
                // Gọi thẳng invoke từ component
                await connection.invoke("SeatsReserved", showtimeInfo.id, selectedSeats);

                const paymentInfo: PaymentInfoRequest = {
                    userId,
                    showtimeId: showtimeInfo.id,
                    selectedSeats: selectedSeatDetails,
                };

                console.log("Payment Information:", paymentInfo);
                await dispatch(getPaymentUrl(paymentInfo));

            } catch (err) {
                console.error("Failed to reserve seats: ", err);
                // Xử lý lỗi (vd: thông báo cho người dùng ghế đã có người khác chọn)
            }
        }

    };


    const formatPrice = (price: number): string => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(price);
    };

    const formatDateTime = (dateTimeStr: string, type: "date" | "time"): string => {
        const date = new Date(dateTimeStr);

        if (type === "date") {
            // Chỉ hiển thị ngày/tháng/năm
            return date.toLocaleDateString("vi-VN", {
                day: "2-digit",
                month: "2-digit",
                year: "numeric",
            });
        }

        if (type === "time") {
            // Chỉ hiển thị giờ:phút (24h)
            return date.toLocaleTimeString("vi-VN", {
                hour: "2-digit",
                minute: "2-digit",
                hour12: false,
            });
        }

        // fallback nếu không truyền type
        return date.toLocaleString("vi-VN");
    };


    return (
        <div className="max-w-7xl mx-auto p-6 bg-gray-50 min-h-screen">
            <div className="flex flex-col lg:flex-row gap-8">

                {/* Seat Map Section */}
                <div className="flex-1">
                    <div className="bg-white rounded-lg shadow-lg p-6">

                        {/* Screen */}
                        <div className="text-center mb-8">
                            <div className="bg-gradient-to-r from-gray-700 to-gray-900 text-white py-3 px-8 rounded-t-3xl mx-auto max-w-md">
                                <span className="text-lg font-semibold">MÀN HÌNH</span>
                            </div>
                            <div className="h-2 bg-gradient-to-r from-transparent via-gray-300 to-transparent mx-auto max-w-md rounded-b-lg"></div>
                        </div>

                        {/* Seat Map */}
                        <div className="space-y-3">
                            {Object.entries(seatsByRow).map(([rowName, rowSeats]) => (
                                <div key={rowName} className="flex items-center justify-center gap-2">

                                    {/* Row Label */}
                                    <div className="w-8 text-center font-bold text-gray-700">
                                        {rowName}
                                    </div>

                                    {/* Seats */}
                                    <div className="flex gap-1">
                                        {rowSeats.map((seat, index) => {
                                            const isSelected = selectedSeats.includes(seat.id);
                                            const isMiddleBreak = index === 4 && rowSeats.length > 8; // Add gap after 5th seat if more than 8 seats

                                            return (
                                                <React.Fragment key={seat.id}>
                                                    <button
                                                        onClick={() => handleSeatClick(seat)}
                                                        className={`
                                ${getSeatWidth(seat.seatTypeName)} h-8 rounded border-2 text-white text-xs font-bold
                                transition-all duration-200 transform hover:scale-105
                                ${getSeatTypeColor(seat.seatTypeName, seat.status, isSelected)}
                                ${seat.status === 'Reserved' ? 'opacity-50' : ''}
                                ${seat.status === 'Booked' ? 'opacity-80' : ''}
                            `}
                                                        disabled={seat.status === 'Reserved' || seat.status === 'Booked'}
                                                        title={`${seat.rowName}${seat.number} - ${seat.seatTypeName} - ${formatPrice(getSeatPrice(seat.seatTypeId))}`}
                                                    >
                                                        {seat.number}
                                                    </button>
                                                    {isMiddleBreak && <div className="w-4"></div>}
                                                </React.Fragment>
                                            );
                                        })}
                                    </div>

                                    {/* Row Label (Right side) */}
                                    <div className="w-8 text-center font-bold text-gray-700">
                                        {rowName}
                                    </div>
                                </div>
                            ))}
                        </div>

                        {/* Legend */}
                        <div className="mt-8 flex flex-wrap justify-center gap-4 text-sm">
                            <div className="flex items-center gap-2">
                                <div className="w-4 h-4 bg-blue-500 rounded border"></div>
                                <span>Standard (70,000đ)</span>
                            </div>
                            <div className="flex items-center gap-2">
                                <div className="w-8 h-4 bg-pink-500 rounded border"></div>
                                <span>Couple (140,000đ)</span>
                            </div>
                            <div className="flex items-center gap-2">
                                <div className="w-4 h-4 bg-green-500 rounded border"></div>
                                <span>Đã chọn</span>
                            </div>
                            <div className="flex items-center gap-2">
                                <div className="w-4 h-4 bg-red-500 rounded border opacity-50"></div>
                                <span>Đã đặt</span>
                            </div>
                            <div className="flex items-center gap-2">
                                <div className="w-4 h-4 bg-gray-500 rounded border opacity-50"></div>
                                <span>Khóa</span>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Booking Info Section */}
                <div className="w-full lg:w-96">
                    <div className="bg-white rounded-lg shadow-lg p-6 sticky top-6">

                        {/* Movie Info */}
                        <div className="border-b pb-4 mb-4">
                            <h2 className="text-xl font-bold text-gray-800 mb-2">
                                {showtimeInfo.movieTitle}
                            </h2>
                            <div className="space-y-2 text-sm text-gray-600">
                                <div className="flex items-center gap-2">
                                    <MapPin className="h-4 w-4" />
                                    <span>{showtimeInfo.cinemaName}</span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <Calendar className="h-4 w-4" />
                                    <span>{formatDateTime(showtimeInfo.showDate, 'date')}</span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <Clock className="h-4 w-4" />
                                    <span>{formatDateTime(showtimeInfo.actualStartTime, 'time')} - {formatDateTime(showtimeInfo.actualEndTime, 'time')}</span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <User className="h-4 w-4" />
                                    <span>Phòng {showtimeInfo.screenName}</span>
                                </div>
                            </div>
                        </div>

                        {/* Selected Seats */}
                        <div className="border-b pb-4 mb-4">
                            <h3 className="font-semibold text-gray-800 mb-2">Ghế đã chọn</h3>
                            {selectedSeats.length === 0 ? (
                                <p className="text-gray-500 text-sm">Chưa chọn ghế nào</p>
                            ) : (
                                <div className="space-y-2">
                                    {selectedSeats.map(seatId => {
                                        const seat = seats.find(s => s.id === seatId)!;
                                        return (
                                            <div key={seatId} className="flex justify-between items-center text-sm">
                                                <span className="font-medium">
                                                    {seat.rowName}{seat.number} ({seat.seatTypeName})
                                                </span>
                                                <span className="text-red-600 font-semibold">
                                                    {formatPrice(getSeatPrice(seat.seatTypeId))}
                                                </span>
                                            </div>
                                        );
                                    })}
                                </div>
                            )}
                        </div>

                        {/* Total */}
                        <div className="border-b pb-4 mb-4">
                            <div className="flex justify-between items-center">
                                <span className="text-lg font-semibold">Tổng cộng:</span>
                                <span className="text-xl font-bold text-red-600">
                                    {formatPrice(totalPrice)}
                                </span>
                            </div>
                            <p className="text-xs text-gray-500 mt-1">
                                {selectedSeats.length} ghế đã chọn
                            </p>
                        </div>

                        {/* Confirm Button */}
                        <button
                            onClick={handleConfirmBooking}
                            disabled={selectedSeats.length === 0}
                            className={`
                w-full py-3 px-4 rounded-lg font-semibold text-white transition-all duration-200
                ${selectedSeats.length === 0
                                    ? 'bg-gray-400 cursor-not-allowed'
                                    : 'bg-red-600 hover:bg-red-700 hover:shadow-lg transform hover:scale-105'
                                }
              `}
                        >
                            <CreditCard className="h-5 w-5 inline mr-2" />
                            Xác nhận đặt chỗ
                        </button>
                    </div>
                </div>
            </div>

            {/* Confirmation Modal */}
            {showConfirmation && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg max-w-md w-full p-6">
                        <div className="flex justify-between items-center mb-4">
                            <h3 className="text-lg font-bold">Xác nhận đặt vé</h3>
                            <button
                                title='Close'
                                onClick={() => setShowConfirmation(false)}
                                className="text-gray-400 hover:text-gray-600"
                            >
                                <X className="h-6 w-6" />
                            </button>
                        </div>

                        <div className="text-center">
                            <div className="text-6xl mb-4">🎬</div>
                            <p className="text-gray-600 mb-4">
                                Thông tin đặt vé đã được ghi nhận và in ra console.
                                Vui lòng kiểm tra để xem chi tiết!
                            </p>
                            <button
                                onClick={() => setShowConfirmation(false)}
                                className="bg-green-600 text-white px-6 py-2 rounded-lg hover:bg-green-700"
                            >
                                Đóng
                            </button>
                        </div>
                    </div>
                </div>
            )}


            {/* Modal test */}
            {isCompleted && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
                    <div className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4 p-6">
                        <div className="flex items-center justify-between mb-4">
                            <h3 className="text-lg font-semibold text-gray-900">Hoàn thành!</h3>
                            <button
                                onClick={() => setIsCompleted(false)}
                                className="text-gray-400 hover:text-gray-600"
                            >
                                ✕
                            </button>
                        </div>
                        <p className="text-gray-600 mb-6">Thao tác đã được thực hiện thành công.</p>
                        <div className="flex justify-end space-x-3">
                            <button
                                onClick={() => setIsCompleted(false)}
                                className="px-4 py-2 text-gray-600 hover:text-gray-800"
                            >
                                Đóng
                            </button>
                            <button
                                onClick={() => setIsCompleted(false)}
                                className="px-6 py-2 bg-blue-500 hover:bg-blue-600 text-white rounded-md"
                            >
                                OK
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default SeatSelection;


