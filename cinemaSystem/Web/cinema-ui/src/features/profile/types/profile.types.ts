// Profile Types - Based on Backend API

export interface UserProfile {
    id: string;
    userName: string;
    email: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
    dateOfBirth?: string;
    gender?: string;
    address?: string;
    avatarUrl?: string;
    roles: string[];
    emailConfirmed: boolean;
    phoneNumberConfirmed: boolean;
    isLocked: boolean;
    createdAt: string;
    lastLoginAt?: string;
}

export interface UpdateProfileRequest {
    firstName?: string;
    lastName?: string;
    phoneNumber?: string;
    dateOfBirth?: string;
    gender?: string;
    address?: string;
    avatarUrl?: string;
}

export interface ChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
    confirmPassword: string;
}

export interface ForgotPasswordRequest {
    email: string;
}

export interface VerifyResetOtpRequest {
    email: string;
    otpCode: string;
}

export interface ResetPasswordRequest {
    email: string;
    otpCode: string;
    newPassword: string;
}
