import type { ShowtimeInfo, Pricing, Seat, SeatsSelectedResponse, ShowtimeSetupDataDto, Showtime, InputShowtimeDto } from "../../types/showtime.type";
import { createAsyncThunk, createSlice, type PayloadAction } from "@reduxjs/toolkit";
import { showtimeServices } from "../../services/showtime.services";


interface ShowtimeState {
    showtimeInfo: ShowtimeInfo;
    showtimeSetUpData: ShowtimeSetupDataDto | null;
    showtime: Showtime[];
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
    showtimeSetUpData: {
        screens: [],
        slots: [],
        pricingTiers: [],
        movies: [],
        seatTypes: [],
    },
    showtime: [],
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
export const getShowtimeSetUpData = createAsyncThunk(
    'showtime/getShowtimeSetUpData',
    async (cinemaId: string) => {
        return await showtimeServices.getShowtimeSetUpData(cinemaId);
    }
)
export const getShowtimePerformance = createAsyncThunk(
    'showtime/getShowtimePerformance',
    async (cinemaId: string) => {
        return await showtimeServices.getShowtimePerformance(cinemaId);
    }
)
export const postShowtimeForm = createAsyncThunk(
    'showtime/postShowtimeForm',
    async (showtimeData: InputShowtimeDto) => {
        return await showtimeServices.postShowtimeForm(showtimeData);
    }
)
export const confirmShowtime = createAsyncThunk(
    'showtime/confirmShowtime',
    async (showtimeId: string) => {
        return await showtimeServices.putConfirmShowtime(showtimeId);
    }
)
export const cancelShowtime = createAsyncThunk(
    'showtime/cancelShowtime',
    async (showtimeId: string) => {
        return await showtimeServices.putCancelShowtime(showtimeId);
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
        updateShowtimeStatus: (state, action: PayloadAction<{ showtimeId: string, status: string }>) => {
            const { showtimeId, status } = action.payload;
            state.showtime.forEach(showtime => {
                if (showtime.showtimeId === showtimeId) {
                    showtime.status = status;
                }
            });
        }
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

            //Get showtime setup data
            .addCase(getShowtimeSetUpData.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(getShowtimeSetUpData.fulfilled, (state, action) => {
                state.loading = false;
                state.showtimeSetUpData = action.payload;
                state.error = null;
            })
            .addCase(getShowtimeSetUpData.rejected, (state, action) => {
                state.loading = false;
                state.error = action.error.message || 'Get showtime setup data failed';
            })
            //Get showtime performance
            .addCase(getShowtimePerformance.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(getShowtimePerformance.fulfilled, (state, action) => {
                state.loading = false;
                state.showtime = action.payload;
                state.error = null;
            })
            .addCase(getShowtimePerformance.rejected, (state, action) => {
                state.loading = false;
                state.error = action.error.message || 'Get showtime performance failed';
            })
            //Post showtime form
            .addCase(postShowtimeForm.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(postShowtimeForm.fulfilled, (state, action) => {
                state.loading = false;
                state.error = null;
            })
            .addCase(postShowtimeForm.rejected, (state, action) => {
                state.loading = false;
                state.error = action.error.message || 'Post showtime form failed';
            })

            .addCase(confirmShowtime.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(confirmShowtime.fulfilled, (state, action) => {
                state.loading = false;
                state.error = null;
            })
            .addCase(confirmShowtime.rejected, (state, action) => {
                state.loading = false;
                state.error = action.error.message || 'Confirm showtime failed';
            })
            .addCase(cancelShowtime.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(cancelShowtime.fulfilled, (state, action) => {
                state.loading = false;
                state.error = null;
            })
            .addCase(cancelShowtime.rejected, (state, action) => {
                state.loading = false;
                state.error = action.error.message || 'Cancel showtime failed';
            })

    }
});

export const { setSeatingPlan, updateSeatStatus, updateShowtimeStatus } = showtimeSlice.actions;
export default showtimeSlice.reducer;
