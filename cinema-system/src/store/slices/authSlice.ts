import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import type { PayloadAction } from "@reduxjs/toolkit";
import { authServices } from "../../services/auth.services";

interface AuthState {
    accessToken: string | null;
    refreshToken: string | null;
    isAuthenticated: boolean;
    loading: boolean;
    error: string | null;
}
const initialState: AuthState = {
    accessToken: localStorage.getItem('accessToken'),
    refreshToken: localStorage.getItem('refreshToken'),
    isAuthenticated: !!localStorage.getItem('accessToken'),  // note: (!!) casts the value to boolean
    loading: false,
    error: null,
};

// Async thunk
export const loginUser = createAsyncThunk(
    'auth/login',
    async (credentials: { email: string; password: string }) => {
        return await authServices.login(credentials);
    }
)
export const registerUser = createAsyncThunk(
    'auth/register',
    async (userData: { userName: string; email: string; password: string; phoneNumber: string }) => {
        return await authServices.register(userData);
    }
)

export const refreshAccessToken = createAsyncThunk(
    'auth/refresh',
    async (tokenModels: { accessToken: string; refreshToken: string }) => {
        return await authServices.refreshAccessToken(tokenModels);
    }
)


const authSlice = createSlice({
    name: 'auth',
    initialState,
    reducers: {
        logout: (state) => {
            state.accessToken = null;
            state.refreshToken = null;
            state.isAuthenticated = false;
            localStorage.clear();
        },
        clearError: (state) => {
            state.error = null;
        },
        setTokens: (state, action: PayloadAction<{ accessToken: string; refreshToken: string }>) => {
            state.accessToken = action.payload.accessToken;
            state.refreshToken = action.payload.refreshToken;
            state.isAuthenticated = true;
            localStorage.setItem('accessToken', action.payload.accessToken);
            localStorage.setItem('refreshToken', action.payload.refreshToken);
        }
    },
    extraReducers: (builder) => {
        builder
            // Login handling
            .addCase(loginUser.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(loginUser.fulfilled, (state, action) => {
                state.loading = false;
                state.accessToken = action.payload.token.accessToken;
                state.refreshToken = action.payload.token.refreshToken;
                state.isAuthenticated = true;
                localStorage.setItem('accessToken', action.payload.token.accessToken);
                localStorage.setItem('refreshToken', action.payload.token.refreshToken);
            })
            .addCase(loginUser.rejected, (state, action) => {
                state.loading = false;
                state.error = action.error.message || 'Login failed';
            })

            // Register handling
            .addCase(registerUser.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(registerUser.fulfilled, (state) => {
                state.loading = false;
            })
            .addCase(registerUser.rejected, (state, action) => {
                state.loading = false;
                state.error = action.error.message || 'Registration failed';
            })

            // Refresh access token handling
            .addCase(refreshAccessToken.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(refreshAccessToken.fulfilled, (state, action) => {
                state.loading = false;
                state.accessToken = action.payload.accessToken;
                localStorage.setItem('accessToken', action.payload.accessToken);
            })
            .addCase(refreshAccessToken.rejected, (state, action) => {
                setTimeout(() => state.loading = false, 3000);
                state.loading = false;
                state.error = action.error.message || 'Failed to refresh access token';
                //localStorage.clear();
                //window.location.href = "/login"; // Redirect to login on failure
            })
    },
})
export const { logout, clearError, setTokens } = authSlice.actions;
export default authSlice.reducer;