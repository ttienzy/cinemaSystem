interface InventoryItem {
    id: string;
    itemName: string;
    itemCategory: string;
    currentStock: number;
    unitPrice: number;
    image: string;
}

export type { InventoryItem };