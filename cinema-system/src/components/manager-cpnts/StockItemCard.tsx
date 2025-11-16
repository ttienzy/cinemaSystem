import React, { useState } from 'react';
import { type StockItemType } from '../../types/inventory.types';
import { Edit, Save, X, AlertTriangle, Trash2 } from 'lucide-react';

interface StockItemCardProps {
    item: StockItemType;
    onUpdate: (itemId: string, updatedData: { currentStock: number; costPrice: number }) => void;
    onDelete: (itemId: string) => void;
}

const StockItemCard: React.FC<StockItemCardProps> = ({ item, onUpdate, onDelete }) => {
    const [isEditing, setIsEditing] = useState(false);
    const [fields, setFields] = useState({
        currentStock: item.currentStock,
        costPrice: item.costPrice,
    });

    const isLowStock = item.currentStock <= item.minimumStock;
    const stockPercentage = Math.min((item.currentStock / item.minimumStock) * 100, 100);

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        // Chuyển đổi giá trị sang số, đảm bảo không phải số âm
        const numericValue = Math.max(0, parseInt(value, 10) || 0);
        setFields(prev => ({ ...prev, [name]: numericValue }));
    };

    const handleSave = () => {
        // Có thể thêm validation phức tạp hơn ở đây
        onUpdate(item.id, {
            currentStock: fields.currentStock,
            costPrice: fields.costPrice
        });
        setIsEditing(false);
    };

    const handleCancel = () => {
        setFields({ currentStock: item.currentStock, costPrice: item.costPrice });
        setIsEditing(false);
    };

    const handleDelete = () => {
        if (window.confirm(`Bạn có chắc chắn muốn xóa "${item.itemName}"? Thao tác này không thể hoàn tác.`)) {
            onDelete(item.id);
        }
    };

    return (
        <div className="bg-white rounded-lg border border-gray-200 shadow-sm flex flex-col transition-all duration-300 hover:shadow-md">
            {/* Phần ảnh */}
            <div className="h-40 w-full overflow-hidden rounded-t-lg">
                <img src={item.imageUrl} alt={item.itemName} className="w-full h-full object-cover" />
            </div>

            {/* Phần nội dung */}
            <div className="p-4 flex-grow flex flex-col">
                <div className="flex-grow">
                    <span className="text-xs font-semibold text-indigo-600 bg-indigo-50 px-2 py-1 rounded-full">
                        {item.itemCategory}
                    </span>
                    <h3 className="text-lg font-bold text-gray-800 mt-2 truncate" title={item.itemName}>
                        {item.itemName}
                    </h3>

                    {/* Thanh trạng thái tồn kho */}
                    <div className="mt-4">
                        <div className="flex justify-between items-center mb-1">
                            <span className="text-sm font-medium text-gray-600">Tồn kho</span>
                            <span className={`text-sm font-bold ${isLowStock ? 'text-red-600' : 'text-gray-800'}`}>
                                {isEditing ? (
                                    <input
                                        type="number"
                                        name="currentStock"
                                        value={fields.currentStock}
                                        onChange={handleInputChange}
                                        className="w-20 text-right font-bold border-b-2 border-indigo-500 bg-indigo-50 rounded-t-md p-1 focus:outline-none"
                                    />
                                ) : (
                                    item.currentStock
                                )}
                                <span className="text-gray-500 font-normal"> / {item.minimumStock}</span>
                                {isLowStock && <AlertTriangle className="w-4 h-4 inline-block ml-1 text-red-500" />}
                            </span>
                        </div>
                        <div className="w-full bg-gray-200 rounded-full h-2">
                            <div
                                className={`h-2 rounded-full ${isLowStock ? 'bg-red-500' : 'bg-indigo-600'}`}
                                style={{ width: `${stockPercentage}%` }}
                            ></div>
                        </div>
                    </div>

                    {/* Phần giá */}
                    <div className="mt-4 space-y-2 border-t pt-3">
                        <div className="flex justify-between items-center">
                            <span className="text-sm text-gray-500">Giá nhập:</span>
                            <span className="text-sm font-medium text-gray-700">{item.unitPrice.toLocaleString('vi-VN')} ₫</span>
                        </div>
                        <div className="flex justify-between items-center">
                            <span className="text-base font-medium text-gray-800">Giá bán:</span>
                            {isEditing ? (
                                <input
                                    type="number"
                                    name="costPrice"
                                    value={fields.costPrice}
                                    onChange={handleInputChange}
                                    className="w-28 text-right font-bold text-indigo-600 border-b-2 border-indigo-500 bg-indigo-50 rounded-t-md p-1 focus:outline-none"
                                    step="1000"
                                />
                            ) : (
                                <span className="text-base font-bold text-indigo-600">{item.costPrice.toLocaleString('vi-VN')} ₫</span>
                            )}
                        </div>
                    </div>
                </div>

                {/* Phần nút hành động */}
                <div className="mt-4 pt-4 border-t">
                    {isEditing ? (
                        <div className="flex gap-2">
                            <button onClick={handleSave} className="w-full flex items-center justify-center gap-2 bg-indigo-600 text-white font-semibold py-2 px-3 rounded-md hover:bg-indigo-700 transition-colors text-sm">
                                <Save className="w-4 h-4" /> Lưu
                            </button>
                            <button onClick={handleCancel} className="flex-shrink-0 bg-gray-200 text-gray-700 p-2 rounded-md hover:bg-gray-300 transition-colors">
                                <X className="w-5 h-5" />
                            </button>
                        </div>
                    ) : (
                        <div className="flex gap-2">
                            <button onClick={() => setIsEditing(true)} className="w-full flex items-center justify-center gap-2 bg-white text-gray-800 font-semibold py-2 px-3 rounded-md border border-gray-300 hover:bg-gray-100 transition-colors text-sm">
                                <Edit className="w-4 h-4" /> Điều chỉnh
                            </button>
                            <button onClick={handleDelete} className="flex-shrink-0 bg-white text-red-600 p-2 rounded-md border border-gray-300 hover:bg-red-50 transition-colors">
                                <Trash2 className="w-5 h-5" />
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default StockItemCard;