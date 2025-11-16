import React, { useState } from 'react';
import { type StockItemType } from '../../../types/inventory.types';
import StockItemCard from '../../../components/manager-cpnts/StockItemCard';
import { PlusCircle, Package, AlertTriangle } from 'lucide-react';

// Dữ liệu mock
const initialStockData: StockItemType[] = [
    { "id": "2e677a67-0d7c-4d91-dc75-08ddf5c815b3", "cinemaId": "779e27fd-dcf4-4f63-9b55-08dde64deed3", "itemName": "Popcorn (Large)", "itemCategory": "Snack", "currentStock": 15, "minimumStock": 20, "unitPrice": 30000, "costPrice": 50000, "imageUrl": "https://images.unsplash.com/photo-1586201375761-83865001e17d" },
    { "id": "aacd0218-9488-4ea9-dc76-08ddf5c815b3", "cinemaId": "779e27fd-dcf4-4f63-9b55-08dde64deed3", "itemName": "Coca Cola 330ml", "itemCategory": "Drink", "currentStock": 193, "minimumStock": 50, "unitPrice": 15000, "costPrice": 25000, "imageUrl": "https://images.unsplash.com/photo-1603398938378-e54eab4b44c7" },
    { "id": "be958999-5638-4c8d-dc78-08ddf5c815b3", "cinemaId": "779e27fd-dcf4-4f63-9b55-08dde64deed3", "itemName": "Nachos with Cheese", "itemCategory": "Snack", "currentStock": 80, "minimumStock": 30, "unitPrice": 40000, "costPrice": 65000, "imageUrl": "https://images.unsplash.com/photo-1598715682977-5b589b9b4f4a" },
    { "id": "d41a5857-767d-411a-dc79-08ddf5c815b3", "cinemaId": "779e27fd-dcf4-4f63-9b55-08dde64deed3", "itemName": "Bottled Water 500ml", "itemCategory": "Drink", "currentStock": 45, "minimumStock": 50, "unitPrice": 8000, "costPrice": 15000, "imageUrl": "https://images.unsplash.com/photo-1581338787928-875f9d1a3843" }
];

const ManagerStockControl: React.FC = () => {
    const [stockItems, setStockItems] = useState<StockItemType[]>(initialStockData);

    const handleUpdateItem = (itemId: string, updatedData: { currentStock: number; costPrice: number }) => {
        setStockItems(prevItems =>
            prevItems.map(item =>
                item.id === itemId ? { ...item, ...updatedData } : item
            )
        );
        // TODO: Gọi API để cập nhật dữ liệu trên server
        console.log(`Updated item ${itemId} with:`, updatedData);
    };

    const handleDeleteItem = (itemId: string) => {
        setStockItems(prevItems => prevItems.filter(item => item.id !== itemId));
        // TODO: Gọi API để xóa sản phẩm trên server
        console.log(`Deleted item ${itemId}`);
    };

    const lowStockItems = stockItems.filter(item => item.currentStock <= item.minimumStock);

    return (
        <div className="bg-slate-50 min-h-screen p-4 sm:p-6 lg:p-8">
            <div className="max-w-7xl mx-auto">
                {/* Header */}
                <div className="flex flex-wrap justify-between items-center gap-4 mb-6">
                    <div>
                        <h1 className="text-3xl font-bold text-gray-900">Kiểm soát tồn kho</h1>
                        <p className="text-md text-gray-600 mt-1">
                            Theo dõi và điều chỉnh số lượng, giá bán các mặt hàng.
                        </p>
                    </div>
                    <button className="flex items-center gap-2 bg-indigo-600 text-white font-semibold py-2 px-4 rounded-lg shadow-sm hover:bg-indigo-700 transition-colors">
                        <PlusCircle className="w-5 h-5" />
                        Thêm mặt hàng
                    </button>
                </div>

                {/* Cảnh báo tồn kho thấp */}
                {lowStockItems.length > 0 && (
                    <div className="mb-6 p-4 rounded-lg bg-red-50 border border-red-200">
                        <div className="flex items-start">
                            <div className="flex-shrink-0 text-red-500">
                                <AlertTriangle className="h-5 w-5" />
                            </div>
                            <div className="ml-3">
                                <h3 className="text-sm font-semibold text-red-800">
                                    Có {lowStockItems.length} mặt hàng sắp hết hàng
                                </h3>
                                <div className="mt-2 text-sm text-red-700">
                                    <p>Vui lòng kiểm tra và nhập thêm hàng cho các sản phẩm: {lowStockItems.map(item => item.itemName).join(', ')}.</p>
                                </div>
                            </div>
                        </div>
                    </div>
                )}

                {/* Danh sách sản phẩm */}
                {stockItems.length > 0 ? (
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                        {stockItems.map((item) => (
                            <StockItemCard
                                key={item.id}
                                item={item}
                                onUpdate={handleUpdateItem}
                                onDelete={handleDeleteItem}
                            />
                        ))}
                    </div>
                ) : (
                    <div className="text-center col-span-full py-20 bg-white rounded-lg border-2 border-dashed">
                        <Package className="w-16 h-16 text-gray-400 mx-auto mb-4" />
                        <h3 className="text-lg font-medium text-gray-900">Kho hàng trống</h3>
                        <p className="text-gray-500 mt-1">Chưa có sản phẩm nào. Hãy thêm một sản phẩm mới để bắt đầu quản lý.</p>
                    </div>
                )}
            </div>
        </div>
    );
};

export default ManagerStockControl;