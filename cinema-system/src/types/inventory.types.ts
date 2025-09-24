interface InventoryItem {
    id: string;
    itemName: string;
    itemCategory: string;
    currentStock: number;
    unitPrice: number;
    image: string;
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
interface Shift {
    startTime: string;
    endTime: string;
    shiftDate: string;
}

interface Staff {
    fullName: string;
    position: string;
    department: string;
    phone: string;
    email: string;
    shifts: Shift[];
}
interface RevenueEmployee {
    saleDate: string;
    totalTransactions: number;
    totalRevenue: number;
}

interface StaffWorking {
    id: string;
    cinemaaId: string;
}

export type { InventoryItem, ConcessionSaleInfo, ConcessionResponse, ConcessionSaleQueryParameters, PagingInfo, ConcessionSalesItem, Staff, Shift, RevenueEmployee, StaffWorking };