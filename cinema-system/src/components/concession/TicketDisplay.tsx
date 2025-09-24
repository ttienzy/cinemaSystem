import { Ticket, Trash2 } from "lucide-react";
import type { TicketCartItem } from "../../types/dashboard.types";

interface TicketDisplayProps {
    tickets: TicketCartItem;
    formatCurrency: (amount: number) => string;
    onRemoveTicket: () => void;
}

const TicketDisplay: React.FC<TicketDisplayProps> = ({
    tickets,
    formatCurrency,
    onRemoveTicket
}) => {
    const totalPrice = tickets.selectedSeats.reduce((sum, seat) => sum + seat.price, 0);

    return (
        <div className="border border-gray-200 rounded-lg p-3">
            <div className="flex justify-between items-start">
                <div className="flex-1">
                    <div className="font-medium text-sm flex items-center">
                        <Ticket className="w-4 h-4 mr-1 text-blue-500" />
                        {tickets.movieTitle}
                    </div>
                    <div className="text-gray-600 text-xs mb-2">
                        Suất chiếu: {tickets.actualStartTime} - {tickets.actualEndTime}
                    </div>

                    <div className="flex items-center space-x-2">
                        <span className="font-medium text-sm">
                            Số vé: {tickets.selectedSeats.length}
                        </span>
                    </div>
                </div>

                <div className="text-right flex flex-col items-end space-y-2">
                    <div className="font-semibold">
                        {formatCurrency(totalPrice)}
                    </div>

                    <button
                        onClick={onRemoveTicket}
                        className="text-red-500 hover:text-red-700 p-1 hover:bg-red-50 rounded transition-colors"
                        title="Xóa vé khỏi giỏ hàng"
                    >
                        <Trash2 size={16} />
                    </button>
                </div>
            </div>
        </div>
    );
};

export default TicketDisplay;