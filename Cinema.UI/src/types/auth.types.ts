export interface User {
    id: string;
    email: string;
    fullName: string;
    roles: string[];
}

export interface RegisterRequest {
    email: string;
    password: string;
    fullName: string;
    phoneNumber?: string;
}

export interface LoginRequest {
    email: string;
    password: string;
}

export interface LoginResponse {
    accessToken: string;
    refreshToken: string;
    userId: string;
    email: string;
    fullName: string;
    roles: string[];
    expiresAt: string;
}

export interface UserInfoResponse {
    userId: string;
    email: string;
    fullName: string;
    phoneNumber?: string;
    roles: string[];
}

export interface RefreshTokenRequest {
    refreshToken: string;
}

export interface AuthState {
    user: User | null;
    isAuthenticated: boolean;
    isLoading: boolean;            // True khi đang khởi tạo lần đầu (splash screen)
    isBackgroundVerifying: boolean; // True khi đang xác thực ngầm với server
    setUser: (user: User | null) => void;
    setLoading: (loading: boolean) => void;
    setBackgroundVerifying: (verifying: boolean) => void;
    logout: () => void;
    hydrate: () => void;           // Tái hydrate từ localStorage
}
