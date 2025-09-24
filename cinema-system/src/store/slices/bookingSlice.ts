import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import { bookingServices } from "../../services/booking.servives";
import type { BookingCheckedInResponse } from "../../types/booking.types";

interface BookingState {
    checkedIn: BookingCheckedInResponse | null;
    loading: boolean;
    error: string | null;
}

const initialState: BookingState = {
    checkedIn: null,
    loading: false,
    error: null
}


// Async thunk
export const checkInBooking = createAsyncThunk(
    'booking/checkInBooking',
    async (bookingId: string) => {
        return await bookingServices.checkInBooking(bookingId);
    }
)
export const confirmBookingCheckIn = createAsyncThunk(
    'booking/confirmBookingCheckIn',
    async (bookingId: string) => {
        return await bookingServices.confirmBooking(bookingId);
    }
)

const bookingSlice = createSlice({
    name: 'booking',
    initialState,
    reducers: {},
    extraReducers: (builder) => {
        builder.addCase(checkInBooking.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(checkInBooking.fulfilled, (state, action) => {
            state.checkedIn = action.payload;
            state.loading = false;
            state.error = null;
        });
        builder.addCase(checkInBooking.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Check in booking failed';
        });
        builder.addCase(confirmBookingCheckIn.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(confirmBookingCheckIn.fulfilled, (state, action) => {
            state.checkedIn = action.payload;
            state.loading = false;
            state.error = null;
        });
        builder.addCase(confirmBookingCheckIn.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Confirm check in failed';
        });
    }
})

export const bookingReducer = bookingSlice.reducer;
export default bookingReducer;