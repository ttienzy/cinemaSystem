export interface User {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  fullName: string;
  password: string;
  phoneNumber: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserInfoResponse;
}

export interface UserInfoResponse {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  roles: string[];
}
