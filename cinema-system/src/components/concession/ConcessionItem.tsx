import { Plus } from "lucide-react";



interface InventoryItem {
    id: string;
    itemName: string;
    itemCategory: string;
    currentStock: number;
    unitPrice: number;
    image: string;
}

interface ConcessionItemProps {
    item: InventoryItem;
    onAdd: () => void;
    formatCurrency: (amount: number) => string;
}

const ConcessionItem: React.FC<ConcessionItemProps> = ({ item, onAdd, formatCurrency }) => {
    return (
        <div className="border border-gray-200 rounded-lg p-3">
            <div className="text-center">
                <img
                    src={item.image}
                    alt={item.itemName}
                    className="w-16 h-16 mx-auto mb-2 object-contain"
                />
                <div className="text-sm font-medium">{item.itemName}</div>
                <div className="text-xs text-gray-500">{item.itemCategory}</div>
                <div className="text-sm font-semibold text-blue-600 mt-1">
                    {formatCurrency(item.unitPrice)}
                </div>
                <div className="text-xs text-gray-500">Còn: {item.currentStock}</div>
                <button
                    title="Add"
                    onClick={onAdd}
                    disabled={item.currentStock === 0}
                    className="mt-2 bg-green-600 text-white px-3 py-1 rounded text-xs hover:bg-green-700 disabled:bg-gray-300"
                >
                    <Plus className="w-3 h-3" />
                </button>
            </div>
        </div>
    );
};

export default ConcessionItem;

