import type { UserInfo, Purchase } from "../../types/profile.types";
import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import { profileServices } from "../../services/profile.services";

interface ProfileState {
    userInfo: UserInfo | null;
    purchases: Purchase[];
    loading: boolean;
    error: string | null;
}

const initialState: ProfileState = {
    userInfo: {
        createdAt: '',  
        email: '',
        phoneNumber: '',
        role: [],
        userName: ''
    },
    purchases: [],
    loading: false,
    error: null,
};


// Async thunk
export const getCurrentUser = createAsyncThunk(
    'profile/getCurrentUser',
    async (id: string) => {
        return await profileServices.getCurrentUser(id);
    }
)
export const purchaseHistory = createAsyncThunk(
    'profile/purchaseHistory',
    async (userId: string) => {
        return await profileServices.purchaseHistory(userId);
    }
)

const profileSlice = createSlice({
    name: 'profile',
    initialState,
    reducers: {
        clearError: (state) => {
            state.error = null;
        }
    },
    extraReducers: (builder) => {
        builder
            // Get current user
            .addCase(getCurrentUser.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(getCurrentUser.fulfilled, (state, action) => {
                state.loading = false;
                state.userInfo = action.payload;
            })
            .addCase(getCurrentUser.rejected, (state, action) => {
                state.loading = false;
                state.error = action.error.message || 'Get current user failed';
            })

            // Get purchase history
            .addCase(purchaseHistory.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(purchaseHistory.fulfilled, (state, action) => {
                state.loading = false;
                state.purchases = action.payload;
            })
            .addCase(purchaseHistory.rejected, (state, action) => {
                state.loading = false;
                state.error = action.error.message || 'Get purchase history failed';
            })
    }
});

export const { clearError } = profileSlice.actions;
export default profileSlice.reducer;