// Auth Types

export interface LoginRequest {
    email: string;
    password: string;
    rememberMe?: boolean;
}

export interface RegisterRequest {
    email: string;
    password: string;
    confirmPassword: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
    dateOfBirth?: string;
}

export interface LoginResponse {
    accessToken: string;
    refreshToken: string;
    expiresIn: number;
    tokenType: string;
}

export interface TokenResponse {
    accessToken: string;
    refreshToken: string;
}

export interface RefreshTokenResponse {
    NewAccessToken: string;
}

export interface User {
    id: string;
    email: string;
    username: string;
    firstName?: string;
    lastName?: string;
    phoneNumber?: string;
    dateOfBirth?: string;
    avatar?: string;
    roles: string[];
    emailConfirmed: boolean;
    createdAt: string;
}

export interface AuthState {
    user: User | null;
    accessToken: string | null;
    refreshToken: string | null;
    isAuthenticated: boolean;
    isLoading: boolean;
}

export interface AuthActions {
    setAuth: (user: User, accessToken: string, refreshToken: string) => void;
    logout: () => void;
    setUser: (user: User | null) => void;
    setLoading: (loading: boolean) => void;
}

export type AuthStore = AuthState & AuthActions;

// API Response types
export interface ApiResponse<T> {
    data: T;
    message?: string;
    succeeded: boolean;
}

export interface PaginatedResponse<T> {
    data: T[];
    pageNumber: number;
    pageSize: number;
    totalPages: number;
    totalRecords: number;
    succeeded: boolean;
}
