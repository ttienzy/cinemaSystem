import type { ConcessionCartItem } from "../../types/dashboard.types";
import { Coffee, Minus, Plus, Trash2 } from "lucide-react";


interface CartItemRowProps {
    item: ConcessionCartItem;
    formatCurrency: (amount: number) => string;
    onUpdateItem?: (itemId: string, action: 'increase' | 'decrease' | 'remove') => void;
}

const CartItemRow: React.FC<CartItemRowProps> = ({
    item,
    formatCurrency,
    onUpdateItem
}) => {
    const handleIncrease = () => {
        if (onUpdateItem) {
            onUpdateItem(item.itemId, 'increase');
        }
    };

    const handleDecrease = () => {
        if (onUpdateItem && item.quantity > 1) {
            onUpdateItem(item.itemId, 'decrease');
        }
    };

    const handleRemove = () => {
        if (onUpdateItem) {
            onUpdateItem(item.itemId, 'remove');
        }
    };

    return (
        <div className="border border-gray-200 rounded-lg p-3">
            <div className="flex justify-between items-start">
                <div className="flex-1">
                    <div className="font-medium text-sm flex items-center">
                        <Coffee className="w-4 h-4 mr-1 text-orange-500" />
                        {item.itemName}
                    </div>
                    <div className="text-blue-600 font-semibold text-sm">
                        {formatCurrency(item.price)}
                        <span className="text-gray-500 text-xs ml-1">/ cái</span>
                    </div>

                    <div className="flex items-center mt-2 space-x-2">
                        <div className="flex items-center space-x-2">
                            <span className="font-medium text-sm">Số lượng:</span>
                            <div className="flex items-center border border-gray-300 rounded">
                                <button
                                    title="Decrease quantity"
                                    onClick={handleDecrease}
                                    disabled={item.quantity <= 1}
                                    className="p-1 hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
                                >
                                    <Minus size={16} />
                                </button>
                                <span className="px-3 py-1 border-x border-gray-300 min-w-[40px] text-center">
                                    {item.quantity}
                                </span>
                                <button
                                    title="Increase quantity"
                                    onClick={handleIncrease}
                                    className="p-1 hover:bg-gray-100"
                                >
                                    <Plus size={16} />
                                </button>
                            </div>
                        </div>
                    </div>
                </div>

                <div className="text-right flex flex-col items-end space-y-2">
                    <div className="font-semibold">
                        {formatCurrency(item.price * item.quantity)}
                    </div>

                    {onUpdateItem && (
                        <button
                            onClick={handleRemove}
                            className="text-red-500 hover:text-red-700 p-1"
                            title="Xóa khỏi giỏ hàng"
                        >
                            <Trash2 size={16} />
                        </button>
                    )}
                </div>
            </div>
        </div>
    );
};

export default CartItemRow;