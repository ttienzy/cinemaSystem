import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { AuthStore, User } from '../types/auth.types';
import { getUserFromToken, clearTokens as clearTokensUtil, isTokenValid } from '../../../shared/utils/token';

interface AuthPersistState {
    accessToken: string | null;
    refreshToken: string | null;
    user: User | null;
}

export const useAuthStore = create<AuthStore>()(
    persist(
        (set) => ({
            // Initial state
            user: null,
            accessToken: null,
            refreshToken: null,
            isAuthenticated: false,
            isLoading: true,

            // Actions
            setAuth: (user: User, accessToken: string, refreshToken: string) => {
                // Normalize roles to lowercase for consistent comparison
                const normalizedUser = {
                    ...user,
                    roles: user.roles.map(r => r.toLowerCase())
                };
                set({
                    user: normalizedUser,
                    accessToken,
                    refreshToken,
                    isAuthenticated: true,
                    isLoading: false,
                });
            },

            logout: () => {
                clearTokensUtil();
                set({
                    user: null,
                    accessToken: null,
                    refreshToken: null,
                    isAuthenticated: false,
                    isLoading: false,
                });
            },

            setUser: (user: User | null) => {
                // Normalize roles when setting user
                if (user) {
                    set({ user: { ...user, roles: user.roles.map(r => r.toLowerCase()) } });
                } else {
                    set({ user: null });
                }
            },

            setLoading: (isLoading: boolean) => {
                set({ isLoading });
            },
        }),
        {
            name: 'cinema-auth-storage',
            partialize: (state): AuthPersistState => ({
                accessToken: state.accessToken,
                refreshToken: state.refreshToken,
                user: state.user,
            }),
            onRehydrateStorage: () => (state) => {
                // Restore isAuthenticated based on token presence and validity
                const token = state?.accessToken;
                if (token && isTokenValid(token)) {
                    const userInfo = getUserFromToken(token);
                    if (userInfo && state) {
                        // Normalize roles on rehydration
                        const normalizedRoles = (userInfo.roles || []).map((r: string) => r.toLowerCase());
                        state.user = {
                            id: userInfo.userId || '',
                            email: userInfo.email || '',
                            username: userInfo.username || '',
                            roles: normalizedRoles,
                            emailConfirmed: false,
                            createdAt: new Date().toISOString(),
                        };
                        state.isAuthenticated = true;
                        state.isLoading = false;
                    }
                } else {
                    if (state) {
                        // Clear invalid tokens
                        state.accessToken = null;
                        state.refreshToken = null;
                        state.user = null;
                        state.isAuthenticated = false;
                        state.isLoading = false;
                    }
                }
            },
        }
    )
);

// Helper hook to check if user has specific role (case-insensitive)
export const useHasRole = (roles: string[]): boolean => {
    const user = useAuthStore((state) => state.user);
    if (!user) return false;

    // Normalize input roles to lowercase
    const normalizedRoles = roles.map(r => r.toLowerCase());
    return user.roles.some((role) => normalizedRoles.includes(role.toLowerCase()));
};

// Check if user is admin (case-insensitive)
export const useIsAdmin = (): boolean => {
    return useHasRole(['admin']);
};

// Check if user is admin or manager (case-insensitive)
export const useIsAdminOrManager = (): boolean => {
    return useHasRole(['admin', 'manager']);
};
