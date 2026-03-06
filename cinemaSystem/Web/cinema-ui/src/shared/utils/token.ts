// Token utilities for JWT handling

const ACCESS_TOKEN_KEY = 'cinema_access_token';
const REFRESH_TOKEN_KEY = 'cinema_refresh_token';
const TOKEN_EXPIRY_KEY = 'cinema_token_expiry';

// Decode JWT token (base64) without verification
function decodeJWT(token: string): Record<string, unknown> | null {
    try {
        const base64Url = token.split('.')[1];
        if (!base64Url) return null;

        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split('')
                .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
        );

        return JSON.parse(jsonPayload);
    } catch {
        return null;
    }
}

// Get token expiry time from JWT
function getTokenExpiry(token: string): number | null {
    const decoded = decodeJWT(token);
    if (!decoded || !decoded.exp) return null;

    return decoded.exp as number;
}

// Check if token is expired
export function isTokenExpired(token: string | null): boolean {
    if (!token) return true;

    const expiry = getTokenExpiry(token);
    if (!expiry) return false;

    // Add 30 second buffer
    return Date.now() >= (expiry * 1000) - 30000;
}

// Check if token is structurally valid (has required claims)
export function isTokenValid(token: string | null): boolean {
    if (!token) return false;

    const decoded = decodeJWT(token);
    if (!decoded) return false;

    // Check for required claims
    const hasNameId = !!decoded['nameid'] || !!decoded['sub'];
    const hasExp = !!decoded['exp'];

    // Token is valid if it has nameid and exp, and is not expired
    return hasNameId && hasExp && !isTokenExpired(token);
}

// Get access token from storage
export function getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
}

// Get refresh token from storage
export function getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
}

// Save tokens to storage
export function setTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);

    // Save expiry time
    const expiry = getTokenExpiry(accessToken);
    if (expiry) {
        localStorage.setItem(TOKEN_EXPIRY_KEY, expiry.toString());
    }
}

// Clear all tokens
export function clearTokens(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(TOKEN_EXPIRY_KEY);
}

// Decode token and get user info - Handle backend JWT format
export function getUserFromToken(token: string): {
    userId?: string;
    email?: string;
    username?: string;
    roles?: string[];
} | null {
    const decoded = decodeJWT(token);
    if (!decoded) return null;

    // Backend JWT claims:
    // {
    //   "unique_name": "Tran Thi Admin",
    //   "nameid": "174b04a5-1a1d-4f3c-4f00-08de775e36e6",
    //   "email": "admin.cinestar@gmail.com",
    //   "role": "Admin"
    // }

    // Handle role claim - can be string or array
    // Check multiple possible claim keys
    const roleClaim = decoded['role']
        || decoded['roles']
        || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
        || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role'];

    let roles: string[] = [];
    if (roleClaim) {
        if (Array.isArray(roleClaim)) {
            roles = roleClaim as string[];
        } else {
            roles = [roleClaim as string];
        }
    }

    return {
        userId: decoded['nameid'] as string
            || decoded['sub'] as string
            || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] as string,
        email: decoded['email'] as string
            || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] as string,
        username: decoded['unique_name'] as string
            || decoded['name'] as string
            || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] as string,
        roles,
    };
}
