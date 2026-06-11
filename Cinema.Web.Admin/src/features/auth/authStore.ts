import { create } from 'zustand';
import { clearAuth, getAccessToken, getCachedUser, hasTokens, setCachedUser } from '../../shared/auth/tokenStorage';
import type { User } from '../../shared/types/auth';
import { decodeJwt, extractRoles, isTokenExpired } from '../../shared/utils/jwt';

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isBackgroundVerifying: boolean;
  setUser: (user: User | null) => void;
  setLoading: (isLoading: boolean) => void;
  setBackgroundVerifying: (isBackgroundVerifying: boolean) => void;
  logoutLocal: () => void;
  hydrate: () => void;
}

function restoreUser(): User | null {
  if (!hasTokens()) return null;

  const cachedUser = getCachedUser();
  const accessToken = getAccessToken();
  if (!accessToken) return cachedUser;

  if (isTokenExpired(accessToken)) {
    return cachedUser;
  }

  if (cachedUser) return cachedUser;

  const payload = decodeJwt(accessToken);
  if (!payload) return null;

  const user: User = {
    id: payload.sub,
    email: payload.email ?? '',
    fullName: payload.fullName ?? payload.email ?? '',
    roles: extractRoles(payload),
  };

  setCachedUser(user);
  return user;
}

export const useAuthStore = create<AuthState>((set) => {
  const user = restoreUser();

  return {
    user,
    isAuthenticated: Boolean(user),
    isLoading: !user && hasTokens(),
    isBackgroundVerifying: false,
    setUser: (nextUser) => {
      if (nextUser) setCachedUser(nextUser);
      set({
        user: nextUser,
        isAuthenticated: Boolean(nextUser),
        isLoading: false,
      });
    },
    setLoading: (isLoading) => set({ isLoading }),
    setBackgroundVerifying: (isBackgroundVerifying) => set({ isBackgroundVerifying }),
    logoutLocal: () => {
      clearAuth();
      set({ user: null, isAuthenticated: false, isLoading: false });
    },
    hydrate: () => {
      const restoredUser = restoreUser();
      set({
        user: restoredUser,
        isAuthenticated: Boolean(restoredUser),
        isLoading: false,
      });
    },
  };
});
