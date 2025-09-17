import { Plus } from "lucide-react";


interface InventoryItem {
    id: string;
    itemName: string;
    itemCategory: string;
    currentStock: number;
    unitPrice: number;
    image: string;
}

interface StandaloneConcessionGridProps {
    items: InventoryItem[];
    onAdd: (item: InventoryItem) => void;
    formatCurrency: (amount: number) => string;
}

const StandaloneConcessionGrid: React.FC<StandaloneConcessionGridProps> = ({ items, onAdd, formatCurrency }) => {
    return (
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
            {items.map(item => (
                <div key={item.id} className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow">
                    <div className="text-center">
                        <img
                            src={item.image}
                            alt={item.itemName}
                            className="w-12 h-12 mx-auto mb-3 object-contain"
                        />

                        <div className="font-medium text-gray-900">{item.itemName}</div>
                        <div className="text-sm text-gray-500 mb-2">{item.itemCategory}</div>
                        <div className="text-lg font-semibold text-blue-600 mb-2">
                            {formatCurrency(item.unitPrice)}
                        </div>
                        <div className="text-sm text-gray-500 mb-3">
                            Tồn kho: {item.currentStock}
                        </div>
                        <button
                            onClick={() => onAdd(item)}
                            disabled={item.currentStock === 0}
                            className="w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed flex items-center justify-center"
                        >
                            <Plus className="w-4 h-4 mr-2" />
                            Thêm
                        </button>
                    </div>
                </div>
            ))}
        </div>
    );
};

export default StandaloneConcessionGrid;