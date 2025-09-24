import { createAsyncThunk, createSlice, type PayloadAction } from "@reduxjs/toolkit";
import { inventoryServices } from "../../services/inventory.services";
import type { ConcessionResponse, ConcessionSaleQueryParameters, InventoryItem, RevenueEmployee, Staff, StaffWorking } from "../../types/inventory.types";
import type { CartItem } from "../../types/dashboard.types";

interface InventoryState {
    items: InventoryItem[];
    concessionHistory: ConcessionResponse;
    reportEmployee: RevenueEmployee[];
    schedules: Staff[];
    staffWorking: StaffWorking | null;
    loading: boolean;
    error: string | null;
}
const initialState: InventoryState = {
    items: [],
    schedules: [],
    reportEmployee: [],
    staffWorking: null,
    concessionHistory: { data: [], pagination: { pageIndex: 1, totalPages: 1, count: 0, hasNextPage: false, hasPreviousPage: false } },
    loading: false,
    error: null,
};


//Async thunks
export const getInventory = createAsyncThunk(
    'inventory/getInventory',
    async (cinemaId: string) => {
        return await inventoryServices.getInventory(cinemaId);
    }
)
export const confirmConcessionPurchase = createAsyncThunk(
    'inventory/confirmConcessionPurchase',
    async ({ cinemaId, cartItem }: { cinemaId: string; cartItem: CartItem }) => {
        return await inventoryServices.confirmConcessionPurchase({ cinemaId, cartItem });
    }
)
export const getConcessionSalesHistory = createAsyncThunk(
    'inventory/getConcessionSalesHistory',
    async ({ cinemaId, queryParams }: { cinemaId: string; queryParams: ConcessionSaleQueryParameters }) => {
        return await inventoryServices.getConcessionSaleHistory(cinemaId, queryParams);
    }
)
export const getStaffSchedule = createAsyncThunk(
    'inventory/getStaffSchedule',
    async (cinemaId: string) => {
        return await inventoryServices.getStaffSchedule(cinemaId);
    }
)
export const getReportRevenueAndStock = createAsyncThunk(
    'inventory/getReportRevenueAndStock',
    async (cinemaId: string) => {
        return await inventoryServices.getReportRevenueAndStock(cinemaId);
    }
)
export const getStaffWorkingInfo = createAsyncThunk(
    'inventory/GetStaffOnTime',
    async (email: string) => {
        return await inventoryServices.getStaffOnOnWork(email);
    }
)


const inventorySlice = createSlice({
    name: 'inventory',
    initialState,
    reducers: {
        pageChange(state, action) {
            state.concessionHistory.pagination.pageIndex = action.payload;
        },
        setInfo: (state, action: PayloadAction<StaffWorking>) => {
            state.staffWorking = action.payload;

            localStorage.setItem('staffId', action.payload.id);
            localStorage.setItem('cinemaId', action.payload.cinemaaId);
        }
    },
    extraReducers: (builder) => {
        builder.addCase(getInventory.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(getInventory.fulfilled, (state, action) => {
            state.items = action.payload;
            state.loading = false;
            state.error = null;
        });
        builder.addCase(getInventory.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to fetch inventory';
        });

        builder.addCase(confirmConcessionPurchase.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(confirmConcessionPurchase.fulfilled, (state, action) => {
            state.loading = false;
            state.error = null;
        });
        builder.addCase(confirmConcessionPurchase.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to confirm concession purchase';
        });
        builder.addCase(getConcessionSalesHistory.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(getConcessionSalesHistory.fulfilled, (state, action) => {
            state.concessionHistory = action.payload;
            state.loading = false;
            state.error = null;
        });
        builder.addCase(getConcessionSalesHistory.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to fetch concession sales history';
        });
        builder.addCase(getStaffSchedule.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(getStaffSchedule.fulfilled, (state, action) => {
            state.schedules = action.payload;
            state.loading = false;
            state.error = null;
        });
        builder.addCase(getStaffSchedule.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to fetch staff schedule';
        });
        builder.addCase(getReportRevenueAndStock.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(getReportRevenueAndStock.fulfilled, (state, action) => {
            state.reportEmployee = action.payload;
            state.loading = false;
            state.error = null;
        });
        builder.addCase(getReportRevenueAndStock.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to fetch report revenue and stock';
        });
        //
        builder.addCase(getStaffWorkingInfo.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(getStaffWorkingInfo.fulfilled, (state, action) => {
            state.staffWorking = action.payload;
            console.log(action.payload);
            localStorage.setItem('staffId', action.payload.id);
            localStorage.setItem('cinemaId', action.payload.cinemaId);
            state.loading = false;
            state.error = null;
        });
        builder.addCase(getStaffWorkingInfo.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to fetch staff working';
        });
    }
});

export const { setInfo } = inventorySlice.actions;
export default inventorySlice.reducer;