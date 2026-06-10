export interface JwtPayload {
  sub: string;
  email?: string;
  fullName?: string;
  role?: string | string[];
  exp?: number;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[];
  [key: string]: unknown;
}

export function decodeJwt(token: string): JwtPayload | null {
  try {
    const payload = token.split('.')[1];
    if (!payload) return null;

    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
    const json = decodeURIComponent(
      atob(base64)
        .split('')
        .map((char) => `%${`00${char.charCodeAt(0).toString(16)}`.slice(-2)}`)
        .join(''),
    );

    return JSON.parse(json) as JwtPayload;
  } catch {
    return null;
  }
}

export function isTokenExpired(token: string): boolean {
  const payload = decodeJwt(token);
  if (!payload?.exp) return true;

  return Date.now() >= (payload.exp - 30) * 1000;
}

export function extractRoles(payload: JwtPayload): string[] {
  const roleClaim =
    payload.role ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

  if (!roleClaim) return [];
  return Array.isArray(roleClaim) ? roleClaim : [roleClaim];
}
