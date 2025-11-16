import React,
{
    useState
}
    from 'react';
import InventoryItemComponent from '../../../components/manager-cpnts/InventoryItem'; // Component con sẽ được tạo ở dưới
import {
    PlusCircle
}
    from 'lucide-react';
import { type InventoryItem as Item } from '../../../types/inventory.types';
import { useAppDispatch, useAppSelector } from '../../../hooks/redux';
// Dữ liệu mock giống như bạn cung cấp




const ManagerInventoryList: React.FC = () => {
    const dispatch = useAppDispatch();
    const { items, loading, error } = useAppSelector(state => state.inventory);

    // Hàm xử lý cập nhật tên sản phẩm
    const handleUpdateItemName = (itemId: string, newName: string) => {
        setInventory(prevInventory =>
            prevInventory.map(item =>
                item.id === itemId ? {
                    ...item,
                    itemName: newName
                } : item
            )
        );
        // Ở đây bạn có thể gọi API để cập nhật lên server
        console.log(`Updated item ${itemId} with new name: ${newName}`);
    };

    // Hàm xử lý xóa sản phẩm
    const handleDeleteItem = (itemId: string) => {
        setInventory(prevInventory =>
            prevInventory.filter(item => item.id !== itemId)
        );
        // Ở đây bạn có thể gọi API để xóa sản phẩm trên server
        console.log(`Deleted item ${itemId}`);
    };

    return (
        <div className="bg-gray-50 min-h-screen p-4 sm:p-6 lg:p-8">
            <div className="max-w-7xl mx-auto">
                <div className="flex justify-between items-center mb-6">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">Quản lý kho</h1>
                        <p className="text-sm text-gray-600 mt-1">
                            Xem, cập nhật và xóa các mặt hàng trong kho của bạn.
                        </p>
                    </div>
                    <button className="flex items-center gap-2 bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg shadow-sm hover:bg-blue-700 transition-colors">
                        <PlusCircle className="w-5 h-5" />
                        Thêm sản phẩm
                    </button>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                    {items.map((item) => (
                        <InventoryItemComponent
                            key={item.id}
                            item={item}
                            onUpdate={handleUpdateItemName}
                            onDelete={handleDeleteItem}
                        />
                    ))}
                </div>

                {items.length === 0 && (
                    <div className="text-center col-span-full py-16 bg-white rounded-lg border border-dashed">
                        <p className="text-gray-500">Không có sản phẩm nào trong kho.</p>
                    </div>
                )}
            </div>
        </div>
    );
};

export default ManagerInventoryList;