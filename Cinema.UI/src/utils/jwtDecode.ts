// ============================================================
// JWT Decode - Trình giải mã token nhẹ (Zero-dependency)
// ============================================================
// Giải mã phần payload của JWT để lấy thông tin user ngay lập tức
// mà không cần gọi API. Dùng cho việc hydrate nhanh khi F5.
//
// LƯU Ý: Đây KHÔNG phải là xác thực (verify). Việc verify
// signature chỉ diễn ra ở phía server. Client chỉ decode
// để hiển thị UI tạm thời trước khi server xác nhận.
// ============================================================

export interface JwtPayload {
  sub: string;                 // userId
  email: string;
  fullName?: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[];
  exp: number;                 // expiration timestamp (seconds)
  iat?: number;                // issued at
  [key: string]: unknown;
}

/**
 * Decode một JWT token thành payload object.
 * Trả về null nếu token không hợp lệ.
 */
export function decodeJwt(token: string): JwtPayload | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;

    // Base64Url → Base64 → decode
    const base64 = parts[1]
      .replace(/-/g, '+')
      .replace(/_/g, '/');

    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );

    return JSON.parse(jsonPayload) as JwtPayload;
  } catch {
    return null;
  }
}

/**
 * Kiểm tra xem token đã hết hạn chưa.
 * Trả về true nếu token hết hạn hoặc không decode được.
 */
export function isTokenExpired(token: string): boolean {
  const payload = decodeJwt(token);
  if (!payload?.exp) return true;

  // Trừ 30 giây buffer để tránh race condition
  const bufferSeconds = 30;
  return Date.now() >= (payload.exp - bufferSeconds) * 1000;
}

/**
 * Trích xuất danh sách roles từ JWT payload.
 * ASP.NET Core Identity lưu role trong claim đặc biệt.
 */
export function extractRoles(payload: JwtPayload): string[] {
  const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  if (!roleClaim) return [];
  return Array.isArray(roleClaim) ? roleClaim : [roleClaim];
}
