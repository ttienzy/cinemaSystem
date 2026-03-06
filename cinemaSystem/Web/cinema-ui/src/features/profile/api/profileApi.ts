// Profile API - Based on Backend API endpoints
import axios from '../../../shared/api/axios.instance';
import { Endpoints } from '../../../shared/api/endpoints';
import type { UserProfile, UpdateProfileRequest, ChangePasswordRequest } from '../types/profile.types';

export const profileApi = {
    // Get my profile (authenticated user)
    getMyProfile: async (): Promise<UserProfile> => {
        const response = await axios.get<UserProfile>(Endpoints.PROFILE.BASE);
        return response.data;
    },

    // Update my profile
    updateMyProfile: async (data: UpdateProfileRequest): Promise<void> => {
        const response = await axios.put(Endpoints.PROFILE.BASE, data);
        return response.data;
    },

    // Change password
    changePassword: async (data: ChangePasswordRequest): Promise<void> => {
        const response = await axios.post(Endpoints.PROFILE.CHANGE_PASSWORD, data);
        return response.data;
    },

    // Get profile by user ID (for admin)
    getProfileById: async (userId: string): Promise<UserProfile> => {
        const response = await axios.get<UserProfile>(Endpoints.IDENTITY.PROFILE(userId));
        return response.data;
    },

    // Update profile by user ID (for admin)
    updateProfileById: async (userId: string, data: UpdateProfileRequest): Promise<void> => {
        const response = await axios.put(Endpoints.IDENTITY.PROFILE(userId), data);
        return response.data;
    },

    // Request password reset OTP
    forgotPassword: async (email: string): Promise<void> => {
        const response = await axios.post(Endpoints.IDENTITY.FORGOT_PASSWORD, JSON.stringify(email), {
            headers: { 'Content-Type': 'application/json' },
        });
        return response.data;
    },

    // Verify reset OTP
    verifyResetOtp: async (email: string, otpCode: string): Promise<void> => {
        const response = await axios.post(Endpoints.IDENTITY.VERIFY_RESET_OTP, { email, otpCode });
        return response.data;
    },

    // Reset password with OTP
    resetPassword: async (email: string, otpCode: string, newPassword: string): Promise<void> => {
        const response = await axios.post(Endpoints.IDENTITY.RESET_PASSWORD, { email, otpCode, newPassword });
        return response.data;
    },
};

export default profileApi;
