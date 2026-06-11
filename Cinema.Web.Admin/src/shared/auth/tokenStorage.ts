import type { User } from '../types/auth';

const KEYS = {
  accessToken: 'cinema.admin.accessToken',
  refreshToken: 'cinema.admin.refreshToken',
  cachedUser: 'cinema.admin.user',
} as const;

export function getAccessToken(): string | null {
  return localStorage.getItem(KEYS.accessToken);
}

export function getRefreshToken(): string | null {
  return localStorage.getItem(KEYS.refreshToken);
}

export function setTokens(accessToken: string, refreshToken: string): void {
  localStorage.setItem(KEYS.accessToken, accessToken);
  localStorage.setItem(KEYS.refreshToken, refreshToken);
}

export function hasTokens(): boolean {
  return Boolean(getAccessToken() && getRefreshToken());
}

export function setCachedUser(user: User): void {
  localStorage.setItem(KEYS.cachedUser, JSON.stringify(user));
}

export function getCachedUser(): User | null {
  const value = localStorage.getItem(KEYS.cachedUser);
  if (!value) return null;

  try {
    return JSON.parse(value) as User;
  } catch {
    localStorage.removeItem(KEYS.cachedUser);
    return null;
  }
}

export function clearAuth(): void {
  localStorage.removeItem(KEYS.accessToken);
  localStorage.removeItem(KEYS.refreshToken);
  localStorage.removeItem(KEYS.cachedUser);
}

export function onAuthChange(callback: (hasAuth: boolean) => void): () => void {
  const handler = (event: StorageEvent) => {
    if (event.key === KEYS.accessToken || event.key === KEYS.refreshToken) {
      callback(hasTokens());
    }
  };

  window.addEventListener('storage', handler);
  return () => window.removeEventListener('storage', handler);
}
