# 🔄 Migration Guide: localStorage → HttpOnly Cookie

## 📌 Tại sao cần chuyển sang HttpOnly Cookie?

### ❌ Vấn đề với localStorage
- **XSS Vulnerability**: JavaScript có thể đọc được token → Dễ bị tấn công XSS
- **Không tự động**: Phải manually gắn token vào mỗi request
- **Không có expiry tự động**: Phải tự quản lý việc xóa token

### ✅ Ưu điểm của HttpOnly Cookie
- **XSS Protection**: JavaScript không thể đọc được cookie → An toàn hơn
- **Tự động gửi**: Browser tự động gửi cookie kèm mỗi request
- **Secure flag**: Chỉ gửi qua HTTPS
- **SameSite**: Chống CSRF attack

## 🎯 Kế hoạch Migration

### Phase 1: Backend Changes (Identity.API)

#### 1.1. Cập nhật AuthService.cs
```csharp
public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, HttpContext httpContext)
{
    // ... existing login logic ...

    // Thay vì trả về token trong response body
    // Set cookie trực tiếp
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = true, // Chỉ gửi qua HTTPS
        SameSite = SameSiteMode.Strict,
        Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
    };

    httpContext.Response.Cookies.Append("accessToken", accessToken, cookieOptions);

    var refreshCookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTime.UtcNow.AddDays(7)
    };

    httpContext.Response.Cookies.Append("refreshToken", refreshToken, refreshCookieOptions);

    // Response không cần trả về tokens nữa
    var response = new LoginResponse
    {
        UserId = user.Id,
        Email = user.Email!,
        FullName = user.FullName ?? string.Empty,
        Roles = roles.ToList(),
        ExpiresAt = expiresAt
    };

    return ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful");
}
```

#### 1.2. Cập nhật JWT Authentication Middleware
```csharp
// Program.cs hoặc Startup.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Đọc token từ cookie thay vì Authorization header
                context.Token = context.Request.Cookies["accessToken"];
                return Task.CompletedTask;
            }
        };
        
        // ... existing JWT configuration ...
    });
```

#### 1.3. Cập nhật CORS
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowCredentials() // QUAN TRỌNG: Cho phép gửi cookie
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

### Phase 2: Frontend Changes (React)

#### 2.1. Cập nhật axiosClient.ts
```typescript
// BEFORE
axiosClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// AFTER
const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_GATEWAY_URL,
  withCredentials: true, // QUAN TRỌNG: Cho phép gửi cookie
  headers: {
    'Content-Type': 'application/json',
  },
});

// Không cần request interceptor nữa, cookie tự động gửi
```

#### 2.2. Cập nhật authService.ts
```typescript
// BEFORE
setTokens(accessToken: string, refreshToken: string): void {
  localStorage.setItem('accessToken', accessToken);
  localStorage.setItem('refreshToken', refreshToken);
}

getAccessToken(): string | null {
  return localStorage.getItem('accessToken');
}

// AFTER
setTokens(accessToken: string, refreshToken: string): void {
  // Backend đã set cookie, không cần làm gì
}

getAccessToken(): string | null {
  // Cookie tự động gửi, không cần lấy
  return null;
}

clearTokens(): void {
  // Gọi API logout để backend xóa cookie
  // Không cần xóa localStorage nữa
}
```

#### 2.3. Cập nhật useAuth.ts
```typescript
// BEFORE
const login = useCallback(async (credentials: LoginRequest) => {
  const response = await authService.login(credentials);
  authService.setTokens(response.accessToken, response.refreshToken);
  setUser({
    id: response.userId,
    email: response.email,
    fullName: response.fullName,
    roles: response.roles,
  });
  // ...
}, []);

// AFTER
const login = useCallback(async (credentials: LoginRequest) => {
  const response = await authService.login(credentials);
  // Backend đã set cookie, chỉ cần set user
  setUser({
    id: response.userId,
    email: response.email,
    fullName: response.fullName,
    roles: response.roles,
  });
  // ...
}, []);
```

#### 2.4. Cập nhật App.tsx (Re-hydration)
```typescript
// BEFORE
useEffect(() => {
  const initializeAuth = async () => {
    const accessToken = authService.getAccessToken();
    if (!accessToken) {
      setLoading(false);
      return;
    }
    
    if (authService.isTokenExpired(accessToken)) {
      // Refresh token...
    } else {
      const user = authService.decodeToken(accessToken);
      setUser(user);
    }
  };
  initializeAuth();
}, []);

// AFTER
useEffect(() => {
  const initializeAuth = async () => {
    try {
      // Gọi API /me để lấy thông tin user
      // Cookie tự động gửi kèm request
      const user = await authService.getCurrentUser();
      setUser(user);
    } catch (error) {
      // Nếu lỗi 401, cookie đã hết hạn
      setUser(null);
    } finally {
      setLoading(false);
    }
  };
  initializeAuth();
}, []);
```

### Phase 3: Security Enhancements

#### 3.1. Implement CSRF Protection
```csharp
// Backend: Generate CSRF token
public IActionResult GetCsrfToken()
{
    var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
    return Ok(new { csrfToken = tokens.RequestToken });
}

// Frontend: Include CSRF token in requests
axiosClient.interceptors.request.use(async (config) => {
  const csrfToken = await getCsrfToken();
  config.headers['X-CSRF-TOKEN'] = csrfToken;
  return config;
});
```

#### 3.2. Add Token Rotation
```csharp
// Mỗi lần refresh, tạo cả accessToken và refreshToken mới
// Revoke refreshToken cũ ngay lập tức
```

## 📋 Checklist Migration

### Backend
- [ ] Update AuthService to set HttpOnly cookies
- [ ] Update JWT middleware to read from cookies
- [ ] Update CORS to allow credentials
- [ ] Update logout endpoint to clear cookies
- [ ] Implement CSRF protection
- [ ] Test all auth endpoints

### Frontend
- [ ] Update axiosClient with `withCredentials: true`
- [ ] Remove localStorage token management
- [ ] Update App.tsx re-hydration logic
- [ ] Update login/logout flows
- [ ] Remove token from Authorization header
- [ ] Test all protected routes
- [ ] Test token refresh flow

### Testing
- [ ] Test login flow
- [ ] Test logout flow
- [ ] Test token refresh
- [ ] Test protected routes
- [ ] Test CSRF protection
- [ ] Test cross-origin requests
- [ ] Test cookie expiration

## 🚨 Breaking Changes

1. **Frontend phải chạy trên HTTPS** (hoặc localhost) để cookie Secure hoạt động
2. **CORS phải được cấu hình đúng** với `AllowCredentials()`
3. **Domain phải match** giữa frontend và backend (hoặc dùng subdomain)

## 🎓 Best Practices

1. **Development**: Dùng `Secure = false` cho localhost
2. **Production**: Bắt buộc `Secure = true` và HTTPS
3. **SameSite**: Dùng `Strict` cho security tốt nhất
4. **Expiry**: AccessToken ngắn (15-30 phút), RefreshToken dài (7-30 ngày)
5. **Monitoring**: Log tất cả auth events để audit

## 📚 References

- [OWASP Cookie Security](https://owasp.org/www-community/controls/SecureCookieAttribute)
- [MDN HttpOnly Cookies](https://developer.mozilla.org/en-US/docs/Web/HTTP/Cookies)
- [ASP.NET Core Cookie Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie)

---

**Note**: Migration này nên được thực hiện trong một feature branch riêng và test kỹ trước khi merge vào production.
