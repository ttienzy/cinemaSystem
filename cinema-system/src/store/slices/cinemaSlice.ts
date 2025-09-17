import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import type { Cinema, CinemaDetailsData } from "../../types/cinema.types";
import { cinemaServices } from "../../services/cinema.services";



interface CinemaState {
    cinemas: Cinema[];
    cinemaDetails: CinemaDetailsData;
    loading: boolean;
    error: string | null;
}

const initialState: CinemaState = {
    cinemas: [],
    cinemaDetails: {
        cinema: {
            cinemaName: '',
            address: '',
            phone: '',
            email: '',
            image: '',
            managerName: '',
            status: '',
        },
        screens: []
    },
    loading: false,
    error: null
}

// Async thunk
export const getCinemas = createAsyncThunk(
    'cinema/getCinemas',
    async () => {
        return await cinemaServices.getCinemas();
    }
)
export const getCinemaDetails = createAsyncThunk(
    'cinema/getCinemaDetails',
    async (id: string) => {
        return await cinemaServices.getCinemaById(id);
    }
)

export const cinemaSlice = createSlice({
    name: 'cinema',
    initialState,
    reducers: {},
    extraReducers: (builder) => {
        builder.addCase(getCinemas.fulfilled, (state, action) => {
            state.cinemas = action.payload;
        });
        builder.addCase(getCinemas.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Get cinemas failed';
        })
        // get cinema details
        builder.addCase(getCinemaDetails.fulfilled, (state, action) => {
            state.cinemaDetails = action.payload;
        });
        builder.addCase(getCinemaDetails.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Get cinema details failed';
        })
    }
})



export default cinemaSlice.reducer