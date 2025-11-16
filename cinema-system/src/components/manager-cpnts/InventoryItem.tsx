import React, { useState, useRef, useEffect } from 'react';
import { Edit, Trash2, Save, X, ImageOff } from 'lucide-react'; // Thêm icon ImageOff
import { type InventoryItem } from '../../types/inventory.types';

interface InventoryItemProps {
    item: InventoryItem;
    onUpdate: (itemId: string, newName: string) => void;
    onDelete: (itemId: string) => void;
}

const InventoryItemComponent: React.FC<InventoryItemProps> = ({ item, onUpdate, onDelete }) => {
    const [isEditing, setIsEditing] = useState(false);
    const [itemName, setItemName] = useState(item.itemName);
    const [imageError, setImageError] = useState(false); // State để xử lý lỗi ảnh
    const inputRef = useRef<HTMLInputElement>(null);

    // Focus vào input khi bắt đầu chỉnh sửa
    useEffect(() => {
        if (isEditing) {
            inputRef.current?.select(); // Select toàn bộ text để dễ dàng thay đổi
        }
    }, [isEditing]);

    const handleSave = () => {
        if (itemName.trim() === '') {
            alert('Tên sản phẩm không được để trống.');
            setItemName(item.itemName); // Reset lại tên cũ
            return;
        }
        onUpdate(item.id, itemName.trim());
        setIsEditing(false);
    };

    const handleCancel = () => {
        setItemName(item.itemName);
        setIsEditing(false);
    };

    const handleDelete = () => {
        if (window.confirm(`Bạn có chắc chắn muốn xóa "${item.itemName}"?`)) {
            onDelete(item.id);
        }
    };

    return (
        <div className="group flex flex-col bg-white rounded-lg border border-gray-200 shadow-sm overflow-hidden transition-all duration-300 hover:shadow-xl hover:-translate-y-1">
            {/* Phần Hình ảnh */}
            <div className="relative h-48 w-full bg-gray-100">
                {imageError ? (
                    <div className="w-full h-full flex items-center justify-center bg-gray-200">
                        <ImageOff className="w-12 h-12 text-gray-400" />
                    </div>
                ) : (
                    <img
                        src={item.image}
                        alt={item.itemName}
                        className="w-full h-full object-cover"
                        onError={() => setImageError(true)} // Xử lý lỗi ảnh theo cách của React
                    />
                )}
            </div>

            {/* Phần Nội dung */}
            <div className="p-4 flex flex-col flex-grow">
                <div className="flex-grow">
                    {/* Thẻ danh mục */}
                    <span className={`text-xs font-bold px-2.5 py-1 rounded-full ${item.itemCategory === 'Snack' ? 'bg-orange-100 text-orange-700' : 'bg-sky-100 text-sky-700'}`}>
                        {item.itemCategory}
                    </span>

                    {/* Tên sản phẩm hoặc Input chỉnh sửa */}
                    <div className="mt-2 min-h-[56px]">
                        {isEditing ? (
                            <input
                                ref={inputRef}
                                type="text"
                                value={itemName}
                                onChange={(e) => setItemName(e.target.value)}
                                onKeyDown={(e) => {
                                    if (e.key === 'Enter') handleSave();
                                    if (e.key === 'Escape') handleCancel();
                                }}
                                className="w-full text-lg font-semibold text-gray-800 p-2 border-2 border-indigo-500 rounded-md bg-indigo-50 focus:outline-none"
                            />
                        ) : (
                            <h3 className="text-lg font-bold text-gray-800 leading-tight group-hover:text-indigo-600 transition-colors" title={item.itemName}>
                                {item.itemName}
                            </h3>
                        )}
                    </div>

                    {/* Thông tin phụ: Tồn kho và Giá */}
                    <div className="flex justify-between items-baseline text-sm text-gray-500 mt-4 border-t pt-3">
                        <p>Tồn kho: <span className="font-semibold text-gray-800">{item.currentStock}</span></p>
                        <p className="font-bold text-lg text-indigo-600">{item.unitPrice.toLocaleString('vi-VN')} ₫</p>
                    </div>
                </div>

                {/* Các nút hành động */}
                <div className="mt-4 pt-4 border-t">
                    {isEditing ? (
                        <div className="flex items-center gap-2">
                            <button
                                onClick={handleSave}
                                className="w-full flex items-center justify-center gap-2 bg-indigo-600 text-white font-semibold py-2 px-3 rounded-md hover:bg-indigo-700 transition-colors text-sm"
                            >
                                <Save className="w-4 h-4" />
                                Lưu thay đổi
                            </button>
                            <button
                                onClick={handleCancel}
                                className="p-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300 transition-colors"
                            >
                                <X className="w-5 h-5" />
                            </button>
                        </div>
                    ) : (
                        <div className="flex items-center gap-2">
                            <button
                                onClick={() => setIsEditing(true)}
                                className="w-full flex items-center justify-center gap-2 bg-white text-gray-700 font-semibold py-2 px-3 rounded-md border border-gray-300 hover:bg-gray-100 hover:border-gray-400 transition-colors text-sm"
                            >
                                <Edit className="w-4 h-4" />
                                Chỉnh sửa
                            </button>
                            <button
                                onClick={handleDelete}
                                title="Xóa sản phẩm"
                                className="p-2 text-gray-500 rounded-md hover:bg-red-50 hover:text-red-600 transition-colors"
                            >
                                <Trash2 className="w-5 h-5" />
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default InventoryItemComponent;