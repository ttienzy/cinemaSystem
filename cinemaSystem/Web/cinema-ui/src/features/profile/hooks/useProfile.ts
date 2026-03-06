// Profile Hooks - React Query hooks for profile
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { profileApi } from '../api/profileApi';
import type { UpdateProfileRequest, ChangePasswordRequest } from '../types/profile.types';

// Query keys
const profileKeys = {
    all: ['profile'] as const,
    myProfile: () => [...profileKeys.all, 'my'] as const,
    profileById: (userId: string) => [...profileKeys.all, 'byId', userId] as const,
};

export { profileKeys };

// Hook: Get my profile
export const useMyProfile = () => {
    return useQuery({
        queryKey: profileKeys.myProfile(),
        queryFn: () => profileApi.getMyProfile(),
        staleTime: 5 * 60 * 1000, // 5 minutes
    });
};

// Hook: Get profile by user ID (admin)
export const useProfileById = (userId: string) => {
    return useQuery({
        queryKey: profileKeys.profileById(userId),
        queryFn: () => profileApi.getProfileById(userId),
        enabled: !!userId,
        staleTime: 5 * 60 * 1000,
    });
};

// Hook: Update profile
export const useUpdateProfile = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (data: UpdateProfileRequest) => profileApi.updateMyProfile(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: profileKeys.myProfile() });
        },
    });
};

// Hook: Change password
export const useChangePassword = () => {
    return useMutation({
        mutationFn: (data: ChangePasswordRequest) => profileApi.changePassword(data),
    });
};

// Hook: Forgot password
export const useForgotPassword = () => {
    return useMutation({
        mutationFn: (email: string) => profileApi.forgotPassword(email),
    });
};

// Hook: Reset password
export const useResetPassword = () => {
    return useMutation({
        mutationFn: ({ email, otpCode, newPassword }: { email: string; otpCode: string; newPassword: string }) =>
            profileApi.resetPassword(email, otpCode, newPassword),
    });
};

export default {
    useMyProfile,
    useProfileById,
    useUpdateProfile,
    useChangePassword,
    useForgotPassword,
    useResetPassword,
};
