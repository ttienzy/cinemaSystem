import { ShoppingCart } from "lucide-react";
import CartItemRow from "./CartItemRow";
import type { CartItem } from "../../types/dashboard.types";


interface CartProps {
    cart: CartItem[];
    totalAmount: number;
    onClear: () => void;
    onConfirmPayment: () => void;
    paymentMethod: 'cash' | 'card' | null;
    onPaymentMethodChange: (method: 'cash' | 'card') => void;
    formatCurrency: (amount: number) => string;
}

const Cart: React.FC<CartProps> = ({
    cart,
    totalAmount,
    onClear,
    onConfirmPayment,
    paymentMethod,
    onPaymentMethodChange,
    formatCurrency
}) => {
    return (
        <div className="bg-white rounded-lg shadow border border-gray-200 p-6">
            <h3 className="text-lg font-semibold mb-4 flex items-center">
                <ShoppingCart className="w-5 h-5 mr-2" />
                Giỏ hàng ({cart.length})
            </h3>

            <div className="space-y-3 mb-6 max-h-96 overflow-y-auto">
                {cart.length === 0 ? (
                    <div className="text-center text-gray-500 py-8">
                        <ShoppingCart className="w-12 h-12 mx-auto mb-3 text-gray-300" />
                        <p>Giỏ hàng trống</p>
                    </div>
                ) : (
                    cart.map((item, index) => (
                        <CartItemRow
                            key={`${item.id}-${item.type}-${index}`}
                            item={item}
                            formatCurrency={formatCurrency}
                        />
                    ))
                )}
            </div>

            {cart.length > 0 && (
                <div className="border-t border-gray-200 pt-4">
                    <div className="flex justify-between items-center mb-4">
                        <span className="font-semibold">Tổng cộng:</span>
                        <span className="text-xl font-bold text-blue-600">
                            {formatCurrency(totalAmount)}
                        </span>
                    </div>

                    <div className="mb-4">
                        <h4 className="font-medium mb-2">Phương thức thanh toán:</h4>
                        <div className="flex space-x-4">
                            <button
                                onClick={() => onPaymentMethodChange('cash')}
                                className={`py-2 px-4 rounded font-medium ${paymentMethod === 'cash'
                                    ? 'bg-blue-600 text-white'
                                    : 'bg-gray-200 text-gray-800 hover:bg-gray-300'
                                    }`}
                            >
                                Tiền mặt
                            </button>
                            <button
                                onClick={() => onPaymentMethodChange('card')}
                                className={`py-2 px-4 rounded font-medium ${paymentMethod === 'card'
                                    ? 'bg-blue-600 text-white'
                                    : 'bg-gray-200 text-gray-800 hover:bg-gray-300'
                                    }`}
                            >
                                Thẻ/QR
                            </button>
                        </div>
                    </div>

                    <button
                        onClick={onConfirmPayment}
                        disabled={!paymentMethod}
                        className="w-full bg-green-600 text-white py-3 px-4 rounded-lg hover:bg-green-700 disabled:bg-gray-300 disabled:cursor-not-allowed font-semibold"
                    >
                        Thanh toán
                    </button>

                    <button
                        onClick={onClear}
                        className="w-full mt-2 bg-gray-200 text-gray-800 py-2 px-4 rounded-lg hover:bg-gray-300"
                    >
                        Xóa tất cả
                    </button>
                </div>
            )}
        </div>
    );
};

export default Cart;