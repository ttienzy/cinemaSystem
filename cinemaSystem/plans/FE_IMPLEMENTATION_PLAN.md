# Cinema System - Frontend Implementation Plan

## 1. Project Overview

- **Project Name**: Cinema System Web (React + TypeScript + Vite)
- **Tech Stack**: React 19, TypeScript, Vite, Ant Design 6, React Query, Zustand, React Router v7
- **Backend API**: ASP.NET Core 8.0 at port **8080**
- **Frontend Port**: 5173 (Vite default)

---

## 2. API Configuration

### Environment Setup (Đã cập nhật)

```env
# .env.development
VITE_API_URL=http://localhost:8080

# .env.production
VITE_API_URL=https://api.yourdomain.com
```

---

## 3. API Mapping by Role

### 3.1. PUBLIC APIs (Không cần authentication)

| Controller     | Endpoint                                           | Method | Description                     |
| -------------- | -------------------------------------------------- | ------ | ------------------------------- |
| **Auth**       | `/api/auth/register`                               | POST   | Đăng ký tài khoản mới           |
| **Auth**       | `/api/auth/login`                                  | POST   | Đăng nhập                       |
| **Auth**       | `/api/auth/refresh`                                | POST   | Refresh token                   |
| **Movies**     | `/api/movies`                                      | GET    | Lấy danh sách phim (phân trang) |
| **Movies**     | `/api/movies/{id}`                                 | GET    | Chi tiết phim                   |
| **Movies**     | `/api/movies/now-showing`                          | GET    | Phim đang chiếu                 |
| **Movies**     | `/api/movies/coming-soon`                          | GET    | Phim sắp chiếu                  |
| **Showtimes**  | `/api/showtimes/movie/{movieId}`                   | GET    | Lịch chiếu theo phim            |
| **Showtimes**  | `/api/showtimes/cinema/{cinemaId}`                 | GET    | Lịch chiếu theo rạp             |
| **Showtimes**  | `/api/showtimes/{id}/seating-plan`                 | GET    | Sơ đồ ghế                       |
| **Cinemas**    | `/api/cinemas`                                     | GET    | Danh sách rạp                   |
| **Genres**     | `/api/genres`                                      | GET    | Danh sách thể loại phim         |
| **Genres**     | `/api/genres?activeOnly=true`                      | GET    | Thể loại đang hoạt động         |
| **SeatTypes**  | `/api/seat-types`                                  | GET    | Loại ghế                        |
| **Promotions** | `/api/promotions`                                  | GET    | Khuyến mãi đang hoạt động       |
| **Promotions** | `/api/promotions/validate?code=XXX&orderTotal=XXX` | GET    | Kiểm tra mã KM                  |

---

### 3.2. CUSTOMER APIs (User đã đăng nhập)

| Controller   | Endpoint                            | Method | Description           |
| ------------ | ----------------------------------- | ------ | --------------------- |
| **Auth**     | `/api/auth/logout`                  | POST   | Đăng xuất             |
| **Profile**  | `/api/profile`                      | GET    | Lấy thông tin profile |
| **Profile**  | `/api/profile`                      | PUT    | Cập nhật profile      |
| **Profile**  | `/api/profile/change-password`      | POST   | Đổi mật khẩu          |
| **Bookings** | `/api/bookings`                     | POST   | Tạo đặt vé mới        |
| **Bookings** | `/api/bookings/my`                  | GET    | Danh sách vé của tôi  |
| **Bookings** | `/api/bookings/{id}`                | GET    | Chi tiết đặt vé       |
| **Bookings** | `/api/bookings/{id}/cancel`         | POST   | Hủy đặt vé            |
| **Bookings** | `/api/bookings/{id}/request-refund` | POST   | Yêu cầu hoàn tiền     |
| **Bookings** | `/api/bookings/callback`            | GET    | Callback từ VNPay     |

---

### 3.3. MANAGER APIs (Role: Manager)

| Controller            | Endpoint                                                                    | Method | Description            |
| --------------------- | --------------------------------------------------------------------------- | ------ | ---------------------- |
| **Manager Dashboard** | `/api/manager/dashboard/stats?cinemaId=XXX`                                 | GET    | Thống kê tổng quan rạp |
| **Manager Dashboard** | `/api/manager/dashboard/revenue?cinemaId=XXX&from=...&to=...&groupBy=day`   | GET    | Báo cáo doanh thu      |
| **Manager Dashboard** | `/api/manager/dashboard/top-movies?cinemaId=XXX&limit=10`                   | GET    | Top phim ăn khách      |
| **Manager Showtimes** | `/api/manager/showtimes`                                                    | POST   | Tạo lịch chiếu mới     |
| **Manager Showtimes** | `/api/manager/showtimes/bulk`                                               | POST   | Tạo nhiều lịch chiếu   |
| **Manager Showtimes** | `/api/manager/showtimes/{showtimeId}`                                       | PUT    | Cập nhật lịch chiếu    |
| **Manager Showtimes** | `/api/manager/showtimes/{showtimeId}`                                       | DELETE | Hủy lịch chiếu         |
| **Manager Showtimes** | `/api/manager/showtimes/{showtimeId}/confirm`                               | POST   | Xác nhận lịch chiếu    |
| **Manager Showtimes** | `/api/manager/showtimes/{cinemaId}?date=...`                                | GET    | DS lịch chiếu theo rạp |
| **Manager Showtimes** | `/api/manager/showtimes/detail/{showtimeId}`                                | GET    | Chi tiết lịch chiếu    |
| **Manager Bookings**  | `/api/manager/bookings?cinemaId=...&status=...&date=...&page=1&pageSize=20` | GET    | DS đặt vé tại rạp      |
| **Manager Bookings**  | `/api/manager/bookings/today?cinemaId=XXX`                                  | GET    | Đặt vé hôm nay         |
| **Manager Bookings**  | `/api/manager/bookings/refund-requests?cinemaId=XXX`                        | GET    | DS yêu cầu hoàn tiền   |
| **Manager Bookings**  | `/api/manager/bookings/{id}/approve-refund`                                 | POST   | Duyệt hoàn tiền        |
| **Admin Promotions**  | `/api/admin/promotions`                                                     | GET    | DS khuyến mãi          |
| **Admin Promotions**  | `/api/admin/promotions`                                                     | POST   | Tạo khuyến mãi         |
| **Admin Promotions**  | `/api/admin/promotions/{id}`                                                | PUT    | Cập nhật KM            |
| **Admin Promotions**  | `/api/admin/promotions/{id}`                                                | DELETE | Xóa khuyến mãi         |
| **Admin Promotions**  | `/api/admin/promotions/{id}/activate`                                       | POST   | Kích hoạt KM           |
| **Admin Promotions**  | `/api/admin/promotions/{id}/deactivate`                                     | POST   | Vô hiệu hóa KM         |

---

### 3.4. ADMIN APIs (Role: Admin)

| Controller           | Endpoint                                                               | Method | Description         |
| -------------------- | ---------------------------------------------------------------------- | ------ | ------------------- |
| **Admin Dashboard**  | `/api/admin/dashboard/stats?cinemaId=XXX`                              | GET    | Thống kê toàn chuỗi |
| **Admin Dashboard**  | `/api/admin/dashboard/revenue?from=...&to=...&groupBy=day`             | GET    | Báo cáo doanh thu   |
| **Admin Dashboard**  | `/api/admin/dashboard/top-movies?limit=10`                             | GET    | Top phim toàn chuỗi |
| **Admin Movies**     | `/api/admin/movies`                                                    | POST   | Tạo phim mới        |
| **Admin Movies**     | `/api/admin/movies/{id}`                                               | PUT    | Cập nhật phim       |
| **Admin Movies**     | `/api/admin/movies/{id}`                                               | DELETE | Xóa phim            |
| **Admin Cinemas**    | `/api/admin/cinemas`                                                   | POST   | Tạo rạp mới         |
| **Admin Cinemas**    | `/api/admin/cinemas/{id}`                                              | PUT    | Cập nhật rạp        |
| **Admin Cinemas**    | `/api/admin/cinemas/{id}`                                              | DELETE | Xóa rạp             |
| **Admin Cinemas**    | `/api/admin/cinemas/{cinemaId}/screens`                                | GET    | DS màn hình         |
| **Admin Cinemas**    | `/api/admin/cinemas/{cinemaId}/screens`                                | POST   | Tạo màn hình mới    |
| **Admin Cinemas**    | `/api/admin/cinemas/{cinemaId}/screens/{screenId}`                     | PUT    | Cập nhật màn hình   |
| **Admin Cinemas**    | `/api/admin/cinemas/{cinemaId}/screens/{screenId}`                     | DELETE | Xóa màn hình        |
| **Admin Cinemas**    | `/api/admin/cinemas/screens/{screenId}/seats/bulk`                     | POST   | Tạo ghế hàng loạt   |
| **Admin Cinemas**    | `/api/admin/cinemas/screens/{screenId}/seats/{seatId}`                 | PUT    | Cập nhật ghế        |
| **Admin Cinemas**    | `/api/admin/cinemas/screens/{screenId}/seats/{seatId}`                 | DELETE | Xóa ghế             |
| **Admin Users**      | `/api/admin/users?role=...&search=...&isLocked=...&page=1&pageSize=20` | GET    | DS người dùng       |
| **Admin Users**      | `/api/admin/users/{userId}`                                            | GET    | Chi tiết user       |
| **Admin Users**      | `/api/admin/users/{userId}/lock`                                       | PUT    | Khóa tài khoản      |
| **Admin Users**      | `/api/admin/users/{userId}/unlock`                                     | PUT    | Mở khóa tài khoản   |
| **Admin Users**      | `/api/admin/users/staff`                                               | POST   | Tạo tài khoản NV    |
| **Admin Users**      | `/api/admin/users/{userId}/role`                                       | PUT    | Cập nhật vai trò    |
| **Admin Promotions** | Same as Manager                                                        |        | All promotion CRUD  |
| **Admin Promotions** | `/api/admin/promotions/{id}`                                           | GET    | Chi tiết KM         |

---

### 3.5. STAFF APIs (Role: Staff)

| Controller   | Endpoint                      | Method | Description      |
| ------------ | ----------------------------- | ------ | ---------------- |
| **Bookings** | `/api/bookings/{id}/check-in` | POST   | Check-in vé      |
| **Bookings** | `/api/bookings/check-in`      | POST   | Check-in bằng QR |

---

## 4. Frontend Structure

```
src/
├── app/
│   ├── providers/           # QueryClientProvider, RouterProvider, ThemeProvider
│   ├── router/              # Route definitions, lazy loading
│   │   ├── index.tsx        # createBrowserRouter
│   │   ├── routes.public.tsx
│   │   ├── routes.user.tsx
│   │   └── routes.admin.tsx
│   └── styles/              # Global CSS, Ant Design theme token
│
├── pages/                   # Page-level components
│   ├── home/                # Trang chủ
│   ├── movies/              # Danh sách phim, chi tiết phim
│   ├── cinemas/             # Danh sách rạp
│   ├── booking/             # Đặt vé, thanh toán, kết quả
│   ├── auth/                # Login, Register
│   ├── profile/             # Hồ sơ, lịch sử đặt vé
│   ├── admin/               # Admin Dashboard
│   │   ├── dashboard/
│   │   ├── movies/
│   │   ├── cinemas/
│   │   ├── showtimes/
│   │   ├── bookings/
│   │   ├── users/
│   │   └── promotions/
│   └── errors/              # 404, 403
│
├── features/                # Business logic theo domain
│   ├── auth/               # Đăng nhập, đăng ký
│   ├── movies/             # Phim
│   ├── cinemas/             # Rạp chiếu
│   ├── showtimes/          # Lịch chiếu
│   ├── booking/             # Đặt vé
│   └── admin/               # Admin features
│
└── shared/                  # Dùng chung toàn app
    ├── api/                 # Axios instance, endpoints
    ├── components/          # Layout, common UI
    ├── hooks/               # Reusable hooks
    ├── utils/              # Utilities
    └── types/              # Global types
```

---

## 5. Implementation Status

### ✅ Đã hoàn thành (Phase 2 & 3)

| Task  | Description                   | Status  |
| ----- | ----------------------------- | ------- |
| 2.1.1 | Tạo folder structure          | ✅ Done |
| 2.1.2 | Environment config (.env)     | ✅ Done |
| 2.1.3 | Axios instance + interceptors | ✅ Done |
| 2.1.4 | Endpoints (cơ bản)            | ✅ Done |
| 2.2   | Auth Infrastructure           | ✅ Done |
| 2.3   | Router Setup                  | ✅ Done |
| 2.4   | React Query Setup             | ✅ Done |
| 2.5   | Ant Design Theme              | ✅ Done |
| 3.1   | Authentication                | ✅ Done |
| 3.2   | Movie Catalog                 | ✅ Done |
| 3.3   | Showtimes & Booking           | ✅ Done |

### 🔄 Cần cập nhật

| Task | Description                               | Priority |
| ---- | ----------------------------------------- | -------- |
| U1   | Cập nhật endpoints.ts đầy đủ (tất cả API) | High     |
| U2   | Tạo Admin types và API                    | High     |
| U3   | Tạo Manager types và API                  | High     |
| U4   | Tạo Profile API (change password)         | Medium   |
| U5   | Cập nhật payment callback URL trong BE    | Medium   |

---

## 6. Next Steps

1. **Cập nhật endpoints.ts** - Thêm tất cả API còn thiếu
2. **Tạo Admin features** - Dashboard, Movies CRUD, Cinemas CRUD, Users, Promotions
3. **Tạo Manager features** - Showtimes management, Bookings management
4. **Cập nhật Payment** - Sửa callback URL từ `localhost:5173` thành production URL
5. **Testing** - Test kết nối BE-FE

---

## 7. Notes

- **Backend port**: 8080 (đã cập nhật trong .env)
- **Roles**: Admin, Manager, Staff, Customer
- **Payment**: VNPay integration (cần cập nhật callback URL)
- **JWT**: Access token + Refresh token mechanism
