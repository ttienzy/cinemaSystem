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

    return (
        <div className="mt-6 border-t border-gray-200 pt-4">
            <h4 className="font-medium mb-3">
                Chọn ghế cho {seatData.showtimeInfo.movieTitle} lúc {seatData.showtimeInfo.actualStartTime}:
            </h4>
            <div className="grid grid-cols-20 gap-2 mb-4">
                {seatData.seats.map(seat => {
                    const isSelected = selectedSeats.some(s => s.seatId === seat.id);
                    const colorClass = getSeatColor(seat, isSelected);
                    return (
                        <div
                            key={seat.id}
                            onClick={() => seat.status === 'available' && onSelectSeat(seat)}
                            className={`
                                p-2 text-center border rounded text-sm 
                                ${colorClass} 
                                ${seat.status === 'available' ? 'cursor-pointer' : 'cursor-not-allowed'} 
                                w-8
                                ${seat.seatTypeName === 'couple' ? 'col-span-2 w-16' : ''}
                            `}
                        >
                            {seat.rowName}
                            {seat.number}
                        </div>
                    );
                })}
            </div>
            {/* Display selected seats info */}
            <div className="mb-4">
                <h5 className="font-medium mb-2">Ghế đã chọn:</h5>
                {selectedSeats.length === 0 ? (
                    <p className="text-gray-500">Chưa chọn ghế nào</p>
                ) : (
                    <ul className="space-y-1">
                        {selectedSeats.map((s, index) => {
                            const seat = seatData.seats.find(se => se.id === s.seatId);
                            if (!seat) return null;
                            return (
                                <li key={index} className="text-sm">
                                    {seat.rowName}
                                    {seat.number} ({seat.seatTypeName}) - {formatCurrency(s.price)}
                                </li>
                            );
                        })}
                    </ul>
                )}
            </div>
            <button
                onClick={onConfirm}
                disabled={selectedSeats.length === 0}
                className="bg-blue-600 text-white py-2 px-4 rounded hover:bg-blue-700 disabled:bg-gray-300"
            >
                Xác nhận ghế ({selectedSeats.length} ghế)
            </button>
        </div>
    );
};

export default SeatSelection;