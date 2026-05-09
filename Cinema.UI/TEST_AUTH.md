# 🧪 Authentication Testing Guide

## 📋 Test Scenarios

### 1. ✅ Login Flow

#### Test Case 1.1: Successful Login (Admin)
```
Steps:
1. Navigate to /login
2. Enter: admin@cinema.com / Admin@123
3. Click "Đăng nhập"

Expected:
✓ Success message appears
✓ Redirected to /admin
✓ User info displayed in header
✓ Tokens saved in localStorage
✓ Admin menu accessible
```

#### Test Case 1.2: Successful Login (Customer)
```
Steps:
1. Navigate to /login
2. Enter: customer@cinema.com / Customer@123
3. Click "Đăng nhập"

Expected:
✓ Success message appears
✓ Redirected to /
✓ User info displayed in header
✓ Tokens saved in localStorage
✓ Can access booking pages
```

#### Test Case 1.3: Failed Login (Wrong Password)
```
Steps:
1. Navigate to /login
2. Enter: admin@cinema.com / WrongPassword
3. Click "Đăng nhập"

Expected:
✓ Error message appears
✓ Stay on /login page
✓ No tokens in localStorage
```

#### Test Case 1.4: Failed Login (Invalid Email)
```
Steps:
1. Navigate to /login
2. Enter: invalid-email / password
3. Click "Đăng nhập"

Expected:
✓ Validation error appears
✓ Form not submitted
```

### 2. ✅ Persistent Login (F5 Refresh)

#### Test Case 2.1: Refresh with Valid Token
```
Steps:
1. Login successfully
2. Press F5 to refresh page

Expected:
✓ Loading spinner appears briefly
✓ User stays logged in
✓ User info still displayed
✓ No redirect to login
```

#### Test Case 2.2: Refresh with Expired Access Token
```
Steps:
1. Login successfully
2. Wait for access token to expire (or manually set expired token)
3. Press F5 to refresh page

Expected:
✓ Loading spinner appears
✓ Auto refresh token called
✓ New tokens saved
✓ User stays logged in
```

#### Test Case 2.3: Refresh with Expired Refresh Token
```
Steps:
1. Login successfully
2. Manually expire both tokens in localStorage
3. Press F5 to refresh page

Expected:
✓ Tokens cleared
✓ Redirected to /login
```

### 3. ✅ Protected Routes

#### Test Case 3.1: Access Admin Route (Not Logged In)
```
Steps:
1. Clear localStorage
2. Navigate to /admin

Expected:
✓ Redirected to /login
✓ Location state saved (from: /admin)
✓ After login, redirected back to /admin
```

#### Test Case 3.2: Access Admin Route (Logged as Customer)
```
Steps:
1. Login as customer
2. Navigate to /admin

Expected:
✓ Redirected to /unauthorized
✓ 403 page displayed
```

#### Test Case 3.3: Access Booking Route (Not Logged In)
```
Steps:
1. Clear localStorage
2. Navigate to /booking/123

Expected:
✓ Redirected to /login
✓ After login, redirected back to /booking/123
```

### 4. ✅ Public Routes

#### Test Case 4.1: Access Login (Already Logged In as Admin)
```
Steps:
1. Login as admin
2. Navigate to /login

Expected:
✓ Redirected to /admin
✓ Cannot access login page
```

#### Test Case 4.2: Access Login (Already Logged In as Customer)
```
Steps:
1. Login as customer
2. Navigate to /login

Expected:
✓ Redirected to /
✓ Cannot access login page
```

### 5. ✅ Silent Token Refresh

#### Test Case 5.1: API Call with Expired Token
```
Steps:
1. Login successfully
2. Manually expire access token
3. Make any API call (e.g., fetch movies)

Expected:
✓ First request fails with 401
✓ Refresh token called automatically
✓ New tokens saved
✓ Original request retried with new token
✓ Data loaded successfully
✓ No user interruption
```

#### Test Case 5.2: Multiple Concurrent Requests with Expired Token
```
Steps:
1. Login successfully
2. Manually expire access token
3. Make multiple API calls simultaneously

Expected:
✓ Only ONE refresh token call made
✓ Other requests queued
✓ After refresh, all requests retried
✓ All data loaded successfully
```

### 6. ✅ Logout Flow

#### Test Case 6.1: Logout from Admin Panel
```
Steps:
1. Login as admin
2. Click user avatar → "Đăng xuất"

Expected:
✓ Revoke token API called
✓ Tokens cleared from localStorage
✓ Redirected to /login
✓ Success message appears
```

#### Test Case 6.2: Logout from Customer Page
```
Steps:
1. Login as customer
2. Click user avatar → "Đăng xuất"

Expected:
✓ Revoke token API called
✓ Tokens cleared from localStorage
✓ Redirected to /login
✓ Success message appears
```

### 7. ✅ Role-Based Access Control

#### Test Case 7.1: Admin Access
```
Steps:
1. Login as admin
2. Check accessible routes

Expected:
✓ Can access /admin/*
✓ Can access /booking/*
✓ Can access /checkout
```

#### Test Case 7.2: Customer Access
```
Steps:
1. Login as customer
2. Try to access /admin

Expected:
✓ Cannot access /admin/*
✓ Redirected to /unauthorized
✓ Can access /booking/*
✓ Can access /checkout
```

## 🔧 Manual Testing Tools

### Browser DevTools
```javascript
// Check tokens in localStorage
localStorage.getItem('accessToken')
localStorage.getItem('refreshToken')

// Decode JWT token
const token = localStorage.getItem('accessToken');
const payload = JSON.parse(atob(token.split('.')[1]));
console.log(payload);

// Check expiry
const exp = payload.exp * 1000;
const now = Date.now();
console.log('Expired:', exp < now);
console.log('Expires in:', (exp - now) / 1000 / 60, 'minutes');

// Manually expire token
const expiredToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'; // Old token
localStorage.setItem('accessToken', expiredToken);
```

### Network Tab
```
Check these requests:
✓ POST /api/auth/login
✓ POST /api/auth/refresh-token
✓ POST /api/auth/revoke-token
✓ GET /api/auth/me

Check headers:
✓ Authorization: Bearer <token>
✓ Content-Type: application/json
```

## 🐛 Common Issues & Solutions

### Issue 1: "401 Unauthorized" on every request
```
Cause: Token not being sent
Solution: Check axiosClient interceptor
```

### Issue 2: Infinite refresh loop
```
Cause: Refresh token also expired
Solution: Clear localStorage and login again
```

### Issue 3: User logged out after F5
```
Cause: Re-hydration not working
Solution: Check App.tsx useEffect
```

### Issue 4: Cannot access protected routes
```
Cause: ProtectedRoute not configured
Solution: Check routes configuration
```

### Issue 5: CORS error
```
Cause: Backend CORS not configured
Solution: Add AllowOrigin in backend
```

## 📊 Test Coverage Checklist

- [ ] Login with valid credentials
- [ ] Login with invalid credentials
- [ ] Login validation (email format, password length)
- [ ] Logout functionality
- [ ] Token persistence (F5 refresh)
- [ ] Token expiration handling
- [ ] Silent token refresh
- [ ] Protected routes (authenticated)
- [ ] Protected routes (role-based)
- [ ] Public routes (redirect if logged in)
- [ ] Unauthorized page (403)
- [ ] Not found page (404)
- [ ] Multiple concurrent requests
- [ ] Network error handling
- [ ] Loading states
- [ ] Error messages
- [ ] Success messages

## 🎯 Performance Metrics

### Expected Response Times
- Login: < 500ms
- Refresh Token: < 300ms
- Get User Info: < 200ms
- Logout: < 300ms

### Expected UX
- Loading spinner: < 2s
- No white screens
- Smooth transitions
- No flickering

---

**Last Updated**: 2026-05-01  
**Test Environment**: Development (localhost)
