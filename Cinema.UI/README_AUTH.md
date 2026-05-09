# 🔐 Hệ thống Authentication - CinemaHub

## 📋 Tổng quan

Hệ thống Authentication được thiết kế theo chuẩn **Senior Level** với 3 tầng bảo vệ và **KHÔNG decode JWT ở frontend**.

### ✅ Tầng 1: Protected Routes (Hàng rào bảo vệ)
- **File**: `src/components/auth/ProtectedRoute.tsx`
- **Chức năng**: Kiểm tra quyền truy cập tập trung
- **Features**:
  - Redirect về `/login` nếu chưa đăng nhập
  - Kiểm tra role-based access control
  - Lưu lại trang user định vào để redirect sau khi login
  - Hiển thị loading khi đang kiểm tra token

### ✅ Tầng 2: Persistent Login (Re-hydration)
- **File**: `src/App.tsx`
- **Chức năng**: Khôi phục trạng thái login khi F5
- **Features**:
  - Gọi API `/me` để lấy user info (KHÔNG decode JWT)
  - Axios interceptor tự động refresh token nếu hết hạn
  - Hiển thị splash screen đẹp khi đang khởi tạo
  - Xử lý lỗi gracefully

### ✅ Tầng 3: Axios Interceptor (Silent Refresh)
- **File**: `src/api/axiosClient.ts`
- **Chức năng**: Tự động refresh token khi hết hạn
- **Features**:
  - Tự động gắn Bearer token vào mỗi request
  - Silent refresh khi token hết hạn (401)
  - Queue mechanism để tránh multiple refresh calls
  - Retry failed requests sau khi refresh thành công

## 🏗️ Kiến trúc

```
┌─────────────────────────────────────────────────────────┐
│                    React Application                     │
├─────────────────────────────────────────────────────────┤
│  App.tsx (Re-hydration)                                 │
│  ├─ Check localStorage for tokens                       │
│  ├─ Call GET /api/auth/me                               │
│  └─ Set user from API response                          │
├─────────────────────────────────────────────────────────┤
│  Routes                                                  │
│  ├─ ProtectedRoute (Admin) → Role: Admin               │
│  ├─ ProtectedRoute (Booking) → Authenticated           │
│  └─ PublicRoute (Login) → Redirect if authenticated    │
├─────────────────────────────────────────────────────────┤
│  useAuth Hook (Abstraction Layer)                       │
│  ├─ login() → Get user from LoginResponse              │
│  ├─ logout()                                            │
│  ├─ hasRole()                                           │
│  └─ hasAnyRole()                                        │
├─────────────────────────────────────────────────────────┤
│  authService (Business Logic)                           │
│  ├─ setTokens() / getTokens() / clearTokens()          │
│  ├─ hasTokens() → Simple check                         │
│  ├─ login() → Returns LoginResponse with user info     │
│  ├─ getCurrentUser() → GET /api/auth/me                │
│  └─ refreshToken()                                      │
├─────────────────────────────────────────────────────────┤
│  Axios Interceptor (HTTP Layer)                         │
│  ├─ Request: Attach Bearer token                        │
│  ├─ Response: Unwrap ApiResponse<T>                     │
│  └─ Response: Silent refresh on 401                     │
└─────────────────────────────────────────────────────────┘
```

## 🎯 Best Practices Implemented

### ✅ 1. KHÔNG Decode JWT ở Frontend
**Tại sao?**
- JWT claims format phụ thuộc vào backend (.NET, Java, Node.js khác nhau)
- Hard-code claims là anti-pattern
- Dễ bị lỗi khi backend thay đổi format

**Giải pháp:**
- Backend trả user info trong `LoginResponse`
- Gọi API `/me` khi cần refresh user info
- Frontend chỉ lưu và gửi token, không parse

### ✅ 2. Backend Trả User Info Trong Response
```typescript
// Login Response từ backend
interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  userId: string;        // ✅ Backend trả sẵn
  email: string;         // ✅ Backend trả sẵn
  fullName: string;      // ✅ Backend trả sẵn
  roles: string[];       // ✅ Backend trả sẵn
  expiresAt: string;
}
```

### ✅ 3. API Response Wrapper
Backend bọc tất cả response trong `ApiResponse<T>`:
```typescript
interface ApiResponse<T> {
  data: T;
  success: boolean;
  message: string;
  statusCode: number;
  errors?: ErrorDetail[];
  traceId?: string;
}
```

Axios interceptor tự động unwrap:
```typescript
axiosClient.interceptors.response.use(
  (response) => {
    // Unwrap ApiResponse<T> → return T
    return response.data?.data !== undefined 
      ? response.data.data 
      : response.data;
  }
);
```

## 🔑 Token Management

### LocalStorage Keys
- `accessToken`: JWT access token (short-lived)
- `refreshToken`: Refresh token (long-lived)

### Token Flow
```
1. Login → Backend trả về LoginResponse với user info
2. Save tokens to localStorage
3. Set user từ LoginResponse (KHÔNG decode token)
4. Axios tự động gắn token vào mỗi request
5. Khi 401 → Tự động refresh → Retry request
6. Khi F5 → Call GET /api/auth/me → Set user
```

## 🎯 Cách sử dụng

### 1. Trong Component
```tsx
import { useAuth } from '../hooks/useAuth';

const MyComponent = () => {
  const { user, isAuthenticated, login, logout, hasRole } = useAuth();

  if (!isAuthenticated) {
    return <div>Please login</div>;
  }

  return (
    <div>
      <p>Welcome {user?.fullName}</p>
      {hasRole('Admin') && <AdminPanel />}
      <button onClick={logout}>Logout</button>
    </div>
  );
};
```

### 2. Protected Route
```tsx
// Admin routes - Chỉ Admin mới vào được
<Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
  <Route path="/admin" element={<AdminDashboard />} />
</Route>

// Authenticated routes - Chỉ cần login
<Route element={<ProtectedRoute />}>
  <Route path="/booking" element={<BookingPage />} />
</Route>
```

### 3. API Call
```tsx
// Không cần thêm token thủ công, axios tự động xử lý
const response = await axiosClient.get('/api/movies');
// response đã được unwrap từ ApiResponse<T>
```

## 🔄 Migration Path: localStorage → HttpOnly Cookie

Khi cần chuyển sang HttpOnly Cookie (production), chỉ cần sửa **1 file duy nhất**: `authService.ts`

```typescript
// Before (localStorage)
setTokens(accessToken: string, refreshToken: string) {
  localStorage.setItem('accessToken', accessToken);
  localStorage.setItem('refreshToken', refreshToken);
}

// After (HttpOnly Cookie)
setTokens(accessToken: string, refreshToken: string) {
  // Backend tự động set cookie qua Set-Cookie header
  // Frontend không cần làm gì
}
```

## 🛡️ Security Best Practices

### ✅ Đã implement
- Không decode JWT ở frontend
- User info từ API response
- Silent refresh token (UX mượt mà)
- Queue mechanism (tránh race condition)
- Role-based access control
- Redirect về trang cũ sau khi login
- Clear tokens khi logout/error
- ApiResponse wrapper unwrapping

### 🔜 Nên làm khi production
- [ ] Chuyển sang HttpOnly Cookie
- [ ] Implement CSRF protection
- [ ] Add token rotation
- [ ] Add device fingerprinting
- [ ] Implement rate limiting
- [ ] Add audit logging

## 🧪 Testing

### Test Scenarios
1. ✅ Login thành công → User info từ response
2. ✅ F5 trang → Call /me API → Vẫn giữ trạng thái
3. ✅ Token hết hạn → Tự động refresh
4. ✅ Refresh token hết hạn → Redirect về login
5. ✅ Truy cập trang protected khi chưa login → Redirect về login
6. ✅ Truy cập trang admin khi không phải admin → 403
7. ✅ Đã login mà vào /login → Redirect về trang chủ

## 📝 Notes

- **localStorage** được dùng để test nhanh, dễ debug
- **KHÔNG decode JWT** ở frontend → Không phụ thuộc backend format
- Backend trả user info trong response → Linh hoạt, dễ maintain
- Kiến trúc code đã sẵn sàng để migrate sang **HttpOnly Cookie**
- Backend API đã support đầy đủ: `/login`, `/refresh-token`, `/me`, `/revoke-token`

## 🎓 Senior Tips

1. **Never Decode JWT in Frontend**: Phụ thuộc vào backend format
2. **Get User Info from API**: Backend trả trong response hoặc endpoint `/me`
3. **Single Source of Truth**: authService là nơi duy nhất quản lý tokens
4. **Abstraction**: useAuth hook che giấu implementation details
5. **Type Safety**: TypeScript types cho tất cả auth-related data
6. **Error Handling**: Graceful degradation khi có lỗi
7. **UX First**: Loading states, smooth transitions, no white screens

---

**Tác giả**: Senior Developer  
**Ngày cập nhật**: 2026-05-01  
**Version**: 2.0.0 (No JWT Decode)
