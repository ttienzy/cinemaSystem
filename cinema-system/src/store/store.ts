import { configureStore } from "@reduxjs/toolkit";
import authReducer from "./slices/authSlice";
import profileReducer from "./slices/profileSice";
import movieReducer from "./slices/movieSlice";
import showtimeReducer from "./slices/showtimeSlice";
import cinemaReducer from "./slices/cinemaSlice";
import inventoryReducer from "./slices/inventorySlice";
import bookingReducer from "./slices/bookingSlice";

export const store = configureStore({
    reducer: {
        auth: authReducer,
        profile: profileReducer,
        movie: movieReducer,
        showtime: showtimeReducer,
        cinema: cinemaReducer,
        inventory: inventoryReducer,
        booking: bookingReducer,
    },
    middleware: (getDefaultMiddleware) =>
        getDefaultMiddleware({
            serializableCheck: false, // Disable serializable check for non-serializable data
        }),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
