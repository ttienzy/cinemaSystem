// ============================================================
// Auth Store - Lớp Tái hydrate (Hydration Layer)
// ============================================================
// Zustand store tích hợp instant-hydration từ localStorage.
// Khi F5, store sẽ đọc cached user ngay lập tức để UI hiện
// ra tức thì, KHÔNG ĐỢI bất kỳ API nào.
// ============================================================

import { create } from 'zustand';
import type { AuthState, User } from '../types/auth.types';
import {
  getCachedUser,
  setCachedUser,
  clearAuth,
  hasTokens,
  getAccessToken,
} from '../utils/tokenStorage';
import { decodeJwt, extractRoles, isTokenExpired } from '../utils/jwtDecode';

/**
 * Thử khôi phục user từ cached data hoặc decode JWT.
 * Ưu tiên: cached user > decode từ access token > null.
 */
function restoreUser(): User | null {
  // Nếu không có token thì không cần restore
  if (!hasTokens()) return null;

  const accessToken = getAccessToken();
  if (!accessToken) return null;

  // Kiểm tra token hết hạn chưa (có buffer 30s)
  if (isTokenExpired(accessToken)) {
    // Token hết hạn, nhưng vẫn giữ cached user để hiển thị
    // cho đến khi background verify xử lý refresh
    return getCachedUser();
  }

  // Thử đọc từ cache trước (nhanh nhất)
  const cached = getCachedUser();
  if (cached) return cached;

  // Fallback: decode JWT để lấy thông tin cơ bản
  const payload = decodeJwt(accessToken);
  if (!payload) return null;

  const user: User = {
    id: payload.sub,
    email: payload.email || '',
    fullName: payload.fullName || payload.email || '',
    roles: extractRoles(payload),
  };

  // Cache lại cho lần sau
  setCachedUser(user);
  return user;
}

export const useAuthStore = create<AuthState>((set) => {
  // Hydrate ngay lập tức khi store được tạo (trước cả khi React render)
  const restoredUser = restoreUser();

  return {
    user: restoredUser,
    isAuthenticated: !!restoredUser,
    isLoading: !restoredUser && hasTokens(), // Chỉ loading nếu có token nhưng chưa restore được
    isBackgroundVerifying: false,

    setUser: (user: User | null) => {
      if (user) {
        setCachedUser(user); // Đồng bộ cache
      }
      set({ user, isAuthenticated: !!user, isLoading: false });
    },

    setLoading: (loading: boolean) => set({ isLoading: loading }),

    setBackgroundVerifying: (verifying: boolean) => set({ isBackgroundVerifying: verifying }),

    logout: () => {
      clearAuth(); // Xóa tokens + cached user
      set({ user: null, isAuthenticated: false, isLoading: false });
    },

    hydrate: () => {
      const user = restoreUser();
      set({
        user,
        isAuthenticated: !!user,
        isLoading: false,
      });
    },
  };
});
