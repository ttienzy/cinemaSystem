import type { ShowtimeInfo, Pricing, Seat, SeatsSelectedResponse } from "../../types/showtime.type";
import { createAsyncThunk, createSlice, type PayloadAction } from "@reduxjs/toolkit";
import { showtimeServices } from "../../services/showtime.services";


interface ShowtimeState {
    showtimeInfo: ShowtimeInfo;
    pricings: Pricing[];
    seats: Seat[];
    loading: boolean;
    error: string | null;
}

const initialState: ShowtimeState = {
    showtimeInfo: {
        id: '',
        showDate: '',
        actualStartTime: '',
        actualEndTime: '',
        movieTitle: '',
        cinemaName: '',
        screenName: '',
    },
    pricings: [],
    seats: [],
    loading: false,
    error: null,
};


// Async thunk
export const getShowtimeSeatingPlan = createAsyncThunk(
    'showtime/getShowtimeSeatingPlan',
    async (showtimeId: string) => {
        return await showtimeServices.getSeatingPlan(showtimeId);
    }
)
export const getPaymentUrl = createAsyncThunk(
    'showtime/getPaymentUrl',
    async (paymentInfo: any) => {
        return await showtimeServices.getPaymentUrl(paymentInfo);
    }
)


const showtimeSlice = createSlice({
    name: 'showtime',
    initialState,
    reducers: {
        setSeatingPlan: (state, action: PayloadAction<SeatsSelectedResponse>) => {
            state.showtimeInfo = action.payload.showtimeInfo;
            state.pricings = action.payload.pricings;
            state.seats = action.payload.seats;
        },
        updateSeatStatus: (state, action: PayloadAction<{ seatIds: string[], status: string }>) => {
            const { seatIds, status } = action.payload;
            state.seats.forEach(seat => {
                if (seatIds.includes(seat.id)) {
                    seat.status = status;
                }
            });
        },
    },//Matkhauvnp123
    extraReducers: (builder) => {
        builder
            .addCase(getShowtimeSeatingPlan.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(getShowtimeSeatingPlan.fulfilled, (state, action) => {
                state.loading = false;
                showtimeSlice.caseReducers.setSeatingPlan(state, action);
                state.error = null;
            })
            .addCase(getShowtimeSeatingPlan.rejected, (state, action) => {
                state.loading = false;
                state.error = action.error.message || 'Get showtime failed';
            })
            // Get payment url
            .addCase(getPaymentUrl.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(getPaymentUrl.fulfilled, (state, action) => {
                window.open(action.payload, '_blank');
                state.loading = false;
                state.error = null;
            })
            .addCase(getPaymentUrl.rejected, (state, action) => {
                state.loading = false;
                state.error = action.error.message || 'Get payment url failed';
            })
    }
});

export const { setSeatingPlan, updateSeatStatus } = showtimeSlice.actions;
export default showtimeSlice.reducer;
