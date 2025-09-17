import type { InventoryItem } from "../../types/inventory.types";
import ConcessionItem from "./ConcessionItem";



interface ConcessionGridProps {
    items: InventoryItem[];
    onAdd: (item: InventoryItem) => void;
    formatCurrency: (amount: number) => string;
}

const ConcessionGrid: React.FC<ConcessionGridProps> = ({ items, onAdd, formatCurrency }) => {
    return (
        <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
            {items.map(item => (
                <ConcessionItem
                    key={item.id}
                    item={item}
                    onAdd={() => onAdd(item)}
                    formatCurrency={formatCurrency}
                />
            ))}
        </div>
    );
};

export default ConcessionGrid;