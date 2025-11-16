interface InventoryItem {
    id: string;
    itemName: string;
    itemCategory: string;
    currentStock: number;
    unitPrice: number;
    image: string;
}
export interface StockItemType {
    id: string;
    cinemaId: string;
    itemName: string;
    itemCategory: string;
    currentStock: number;
    minimumStock: number;
    unitPrice: number; // Giá nhập
    costPrice: number; // Giá bán
    imageUrl: string;
}
interface ConcessionSalesItem {
    quantity: number;
    unitPrice: number;
    itemName: string;
}

interface ConcessionSaleInfo {
    saleDate: string;
    totalAmount: number;
    paymentMethod: string;
    items: ConcessionSalesItem[];
    ticketSales?: {
        totalTickets: number;
        status: string;
    };
}
interface RevenueEmployee {
    saleDate: string;
    totalTransactions: number;
    totalRevenue: number;
}
interface PagingInfo {
    pageIndex: number;
    totalPages: number;
    count: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
}

interface ConcessionResponse {
    data: ConcessionSaleInfo[];
    pagination: PagingInfo;
}
interface ConcessionSaleQueryParameters {
    fromDate: string;
    toDate: string;
    paymentMethod: string;
    pageIndex: number;
    pageSize: number;
}



export type { InventoryItem, ConcessionSaleInfo, ConcessionResponse, ConcessionSaleQueryParameters, PagingInfo, ConcessionSalesItem, RevenueEmployee };