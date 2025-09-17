import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import { inventoryServices } from "../../services/inventory.services";
import type { InventoryItem } from "../../types/inventory.types";

interface InventoryState {
    items: InventoryItem[];
    loading: boolean;
    error: string | null;
}
const initialState: InventoryState = {
    items: [],
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

const inventorySlice = createSlice({
    name: 'inventory',
    initialState,
    reducers: {},
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
    }
});

export default inventorySlice.reducer;