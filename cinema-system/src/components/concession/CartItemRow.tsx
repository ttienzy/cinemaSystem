import type { SelectedSeat } from "../../types/showtime.type";

interface CartItem {
    id: string;
    name: string;
    price: number;
    quantity: number;
    type: 'ticket' | 'concession';
    details?: any;
    selectedSeats?: SelectedSeat[]; // For tickets
}
interface CartItemRowProps {
    item: CartItem;
    formatCurrency: (amount: number) => string;
}

const CartItemRow: React.FC<CartItemRowProps> = ({ item, formatCurrency }) => {
    return (
        <div className="border border-gray-200 rounded-lg p-3">
            <div className="flex justify-between items-start">
                <div className="flex-1">
                    <div className="font-medium text-sm">{item.name}</div>
                    <div className="text-blue-600 font-semibold">
                        {formatCurrency(item.price)}
                    </div>
                    <div className="flex items-center mt-2 space-x-2">
                        {item.type === "ticket" ? (
                            <span className="font-medium">Số vé : {item.quantity}</span>
                        ) : (
                            <span className="font-medium">Số lượng: {item.quantity}</span>
                        )}
                    </div>
                </div>
                <div className="text-right">
                    <div className="font-semibold">
                        {formatCurrency(item.price * item.quantity)}
                    </div>
                </div>
            </div>
        </div>
    );
}

export default CartItemRow