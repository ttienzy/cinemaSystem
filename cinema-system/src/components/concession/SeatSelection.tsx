import type { Seat, SeatsSelectedResponse, SelectedSeat } from "../../types/showtime.type";

interface SeatSelectionProps {
    seatData: SeatsSelectedResponse | null;
    selectedSeats: SelectedSeat[];
    onSelectSeat: (seat: Seat) => void;
    onConfirm: () => void;
    formatCurrency: (amount: number) => string;
    getSeatColor: (seat: Seat, isSelected: boolean) => string;
}

const SeatSelection: React.FC<SeatSelectionProps> = ({
    seatData,
    selectedSeats,
    onSelectSeat,
    onConfirm,
    formatCurrency,
    getSeatColor
}) => {
    if (!seatData) return null;

    const getSeatTypeIcon = (seatTypeName: string) => {
        switch (seatTypeName.toLowerCase()) {
            case 'vip':
                return '👑';
            case 'couple':
                return '💕';
            case 'standard':
            default:
                return '';
        }
    };

    const getSeatSize = (seatTypeName: string) => {
        switch (seatTypeName.toLowerCase()) {
            case 'couple':
                return 'col-span-2 w-16 h-10';
            case 'vip':
                return 'w-10 h-10';
            case 'standard':
            default:
                return 'w-8 h-8';
        }
    };

    return (
        <div className="mt-6 border-t border-gray-200 pt-4">
            <h4 className="font-medium mb-3">
                Chọn ghế cho {seatData.showtimeInfo.movieTitle} lúc {seatData.showtimeInfo.actualStartTime}:
            </h4>

            {/* Legend */}
            <div className="flex flex-wrap gap-4 mb-4 text-xs">
                <div className="flex items-center gap-1">
                    <div className="w-4 h-4 bg-green-100 border rounded"></div>
                    <span>Trống</span>
                </div>
                <div className="flex items-center gap-1">
                    <div className="w-4 h-4 bg-blue-500 border rounded"></div>
                    <span>Đã chọn</span>
                </div>
                <div className="flex items-center gap-1">
                    <div className="w-4 h-4 bg-red-100 border rounded"></div>
                    <span>Đã đặt</span>
                </div>
                <div className="flex items-center gap-1">
                    <div className="w-4 h-4 bg-yellow-100 border rounded"></div>
                    <span>Đang giữ</span>
                </div>
            </div>

            {/* Screen indicator */}
            <div className="text-center mb-6">
                <div className="bg-gray-800 text-white py-2 px-8 rounded-t-lg inline-block text-sm">
                    MÀN HÌNH
                </div>
            </div>

            <div className="grid grid-cols-20 gap-1 mb-4 justify-center">
                {seatData.seats.map(seat => {
                    const isSelected = selectedSeats.some(s => s.seatId === seat.id);
                    const colorClass = getSeatColor(seat, isSelected);
                    const sizeClass = getSeatSize(seat.seatTypeName);
                    const icon = getSeatTypeIcon(seat.seatTypeName);

                    return (
                        <div
                            key={seat.id}
                            onClick={() => seat.status.toLowerCase() === 'available' && onSelectSeat(seat)}
                            className={`
                                flex items-center justify-center text-center border rounded text-xs font-medium
                                ${colorClass} 
                                ${seat.status.toLowerCase() === 'available' ? 'cursor-pointer hover:scale-105 transition-transform' : 'cursor-not-allowed'} 
                                ${sizeClass}
                                ${seat.seatTypeName.toLowerCase() === 'vip' ? 'border-2 border-purple-300' : ''}
                            `}
                            title={`${seat.rowName}${seat.number} - ${seat.seatTypeName} - ${seat.status}`}
                        >
                            <div className="flex flex-col items-center">
                                {icon && <span className="text-xs">{icon}</span>}
                                <span className={seat.seatTypeName.toLowerCase() === 'couple' ? 'text-xs' : 'text-xs'}>
                                    {seat.rowName}{seat.number}
                                </span>
                            </div>
                        </div>
                    );
                })}
            </div>

            {/* Seat type legend */}
            <div className="flex flex-wrap gap-6 mb-4 text-sm">
                <div className="flex items-center gap-2">
                    <div className="w-8 h-8 border rounded bg-green-100 flex items-center justify-center text-xs">
                        A1
                    </div>
                    <span>Standard</span>
                </div>
                <div className="flex items-center gap-2">
                    <div className="w-10 h-10 border-2 border-purple-300 rounded bg-green-100 flex flex-col items-center justify-center text-xs">
                        <span>👑</span>
                        <span>A1</span>
                    </div>
                    <span>VIP</span>
                </div>
                <div className="flex items-center gap-2">
                    <div className="w-16 h-10 border rounded bg-green-100 flex flex-col items-center justify-center text-xs">
                        <span>💕</span>
                        <span>A1</span>
                    </div>
                    <span>Couple</span>
                </div>
            </div>

            {/* Display selected seats info */}
            <div className="mb-4">
                <h5 className="font-medium mb-2">Ghế đã chọn:</h5>
                {selectedSeats.length === 0 ? (
                    <p className="text-gray-500">Chưa chọn ghế nào</p>
                ) : (
                    <div className="grid grid-cols-1 gap-2">
                        {selectedSeats.map((s, index) => {
                            const seat = seatData.seats.find(se => se.id === s.seatId);
                            if (!seat) return null;
                            const icon = getSeatTypeIcon(seat.seatTypeName);
                            return (
                                <div key={index} className="flex justify-between items-center bg-gray-50 p-2 rounded">
                                    <span className="text-sm font-medium">
                                        {icon} {seat.rowName}{seat.number} ({seat.seatTypeName})
                                    </span>
                                    <span className="text-sm font-semibold text-blue-600">
                                        {formatCurrency(s.price)}
                                    </span>
                                </div>
                            );
                        })}
                        <div className="border-t pt-2 mt-2">
                            <div className="flex justify-between items-center">
                                <span className="font-medium">Tổng cộng:</span>
                                <span className="font-bold text-lg text-blue-600">
                                    {formatCurrency(selectedSeats.reduce((total, s) => total + s.price, 0))}
                                </span>
                            </div>
                        </div>
                    </div>
                )}
            </div>

            <button
                onClick={onConfirm}
                disabled={selectedSeats.length === 0}
                className="w-full bg-blue-600 text-white py-3 px-4 rounded-lg font-medium hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
            >
                Xác nhận ghế ({selectedSeats.length} ghế)
            </button>
        </div>
    );
};

export default SeatSelection;