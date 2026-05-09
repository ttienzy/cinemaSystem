// ============================================================
// Token Storage - Lớp Lưu trữ (Storage Layer)
// ============================================================
// Trừu tượng hóa toàn bộ logic đọc/ghi token & cached user.
// Hiện tại dùng localStorage. Sau này chỉ cần thay đổi
// file này sang Cookie/SessionStorage mà KHÔNG ảnh hưởng
// bất kỳ logic nào khác trong toàn bộ ứng dụng.
// ============================================================

import type { User } from '../types/auth.types';
import { isTokenExpired } from './jwtDecode';

const KEYS = {
  ACCESS_TOKEN: 'accessToken',
  REFRESH_TOKEN: 'refreshToken',
  CACHED_USER: 'cachedUser', // User info cache cho instant hydration
} as const;

// ---- Token operations ----

export function getAccessToken(): string | null {
  return localStorage.getItem(KEYS.ACCESS_TOKEN);
}

export function getRefreshToken(): string | null {
  return localStorage.getItem(KEYS.REFRESH_TOKEN);
}

export function setTokens(accessToken: string, refreshToken: string): void {
  localStorage.setItem(KEYS.ACCESS_TOKEN, accessToken);
  localStorage.setItem(KEYS.REFRESH_TOKEN, refreshToken);
}

export function hasTokens(): boolean {
  const accessToken = getAccessToken();
  const refreshToken = getRefreshToken();

  if (!accessToken || !refreshToken) {
    return false;
  }

  // Check if access token is expired
  // If expired, let the interceptor handle refresh
  // We still return true because refresh token might be valid
  return true;
}

/**
 * Check if we have valid (non-expired) tokens
 * Use this for critical checks where you need to ensure token is fresh
 */
export function hasValidTokens(): boolean {
  const accessToken = getAccessToken();
  const refreshToken = getRefreshToken();

  if (!accessToken || !refreshToken) {
    return false;
  }

  // If access token is expired, check refresh token
  if (isTokenExpired(accessToken)) {
    // If refresh token is also expired, clear auth
    if (isTokenExpired(refreshToken)) {
      clearAuth();
      return false;
    }
    // Refresh token is valid, let interceptor handle refresh
    return true;
  }

  return true;
}

export function clearAuth(): void {
  localStorage.removeItem(KEYS.ACCESS_TOKEN);
  localStorage.removeItem(KEYS.REFRESH_TOKEN);
  localStorage.removeItem(KEYS.CACHED_USER);
}

// ---- Cached User operations ----
// Lưu thông tin user cơ bản vào localStorage để khi F5,
// UI có thể hiển thị ngay lập tức mà không cần đợi API.

export function setCachedUser(user: User): void {
  try {
    localStorage.setItem(KEYS.CACHED_USER, JSON.stringify(user));
  } catch {
    // Quota exceeded hoặc lỗi serialize - bỏ qua
  }
}

export function getCachedUser(): User | null {
  try {
    const raw = localStorage.getItem(KEYS.CACHED_USER);
    return raw ? (JSON.parse(raw) as User) : null;
  } catch {
    localStorage.removeItem(KEYS.CACHED_USER);
    return null;
  }
}

export function clearCachedUser(): void {
  localStorage.removeItem(KEYS.CACHED_USER);
}

// ---- Storage event listener (Tab Sync) ----
// Khi một tab khác thay đổi token, tab hiện tại sẽ nhận được event.

type AuthChangeCallback = (hasAuth: boolean) => void;

export function onAuthChange(callback: AuthChangeCallback): () => void {
  const handler = (e: StorageEvent) => {
    if (e.key === KEYS.ACCESS_TOKEN || e.key === KEYS.REFRESH_TOKEN) {
      callback(hasTokens());
    }
  };
  window.addEventListener('storage', handler);
  return () => window.removeEventListener('storage', handler);
}
