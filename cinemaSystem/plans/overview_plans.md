# 🎬 Cinema System — Phân Tích Nghiệp Vụ & API Tổng Quan

> **Ngày tạo:** 2026-03-04  
> **Mục đích:** Phân tích toàn bộ nghiệp vụ theo từng actor, đánh giá API hiện tại, đề xuất bổ sung API còn thiếu.

---

## 📋 Mục Lục

1. [Tổng Quan Hệ Thống](#1-tổng-quan-hệ-thống)
2. [Phân Tích Nghiệp Vụ Theo Actor](#2-phân-tích-nghiệp-vụ-theo-actor)
3. [Bảng Tổng Hợp API Hiện Tại](#3-bảng-tổng-hợp-api-hiện-tại)
4. [Đánh Giá Tên Controller](#4-đánh-giá-tên-controller)
5. [Phân Tích API Còn Thiếu](#5-phân-tích-api-còn-thiếu)
6. [Đề Xuất API Cần Thiết Bổ Sung](#6-đề-xuất-api-cần-thiết-bổ-sung)

---

## 1. Tổng Quan Hệ Thống

### 1.1 Roles trong hệ thống

| Role           | Mô tả                                                     |
| -------------- | --------------------------------------------------------- |
| **SuperAdmin** | Quản trị tối cao, quản lý toàn bộ hệ thống                |
| **Admin**      | Quản trị viên, quản lý rạp/phim/khuyến mãi/nhân sự        |
| **Manager**    | Quản lý rạp cụ thể, quản lý lịch chiếu, nhân viên tại rạp |
| **Staff**      | Nhân viên quầy, bán vé/bắp nước tại POS, check-in         |
| **Customer**   | Khách hàng đặt vé online, xem phim, quản lý tài khoản     |

### 1.2 Domain Aggregates (10 nhóm)

| Aggregate               | Entities                                                             |
| ----------------------- | -------------------------------------------------------------------- |
| **BookingAggregate**    | Booking, BookingTicket, Payment, Refund                              |
| **CinemaAggregate**     | Cinema, Screen, Seat                                                 |
| **ConcessionAggregate** | ConcessionSale, ConcessionSaleItem                                   |
| **EquipmentAggregate**  | Equipment, MaintenanceLog                                            |
| **InventoryAggregate**  | InventoryItem, InventoryTransaction                                  |
| **MovieAggregate**      | Movie, MovieCastCrew, MovieCertification, MovieCopyright, MovieGenre |
| **PromotionAggregate**  | Promotion                                                            |
| **SharedAggregates**    | Genre, PricingTier, SeatType, TimeSlot                               |
| **ShowtimeAggregate**   | Showtime, ShowtimePricing                                            |
| **StaffAggregate**      | Staff, Shift, WorkSchedule                                           |

### 1.3 Controllers Hiện Tại (21 controllers)

```
AdminCinemasController     AdminMoviesController      AdminPromotionsController
AdminStaffController       AuthController             BaseApiController
BookingsController         CinemasController          ConcessionsController
GenresController           IdentityController         InventoryController
ManagerShowtimesController MoviesController           PosController
PricingTiersController     PromotionsController       RoleController
SeatTypesController        ShowtimesController        TimeSlotsController
```

---

## 2. Phân Tích Nghiệp Vụ Theo Actor

### 2.1 🔴 ADMIN (SuperAdmin + Admin)

> **Nhiệm vụ:** Quản trị toàn bộ hệ thống, thiết lập cấu hình, quản lý danh mục gốc.

#### Nghiệp vụ chính:

| STT | Nghiệp vụ                               | Controller Hiện Tại         | API Endpoints                         | Trạng Thái                    |
| --- | --------------------------------------- | --------------------------- | ------------------------------------- | ----------------------------- |
| 1   | **Quản lý Rạp** (CRUD Cinema)           | `AdminCinemasController`    | `POST`, `PUT /{id}`, `DELETE /{id}`   | ✅ Có                         |
| 2   | **Quản lý Phòng chiếu** (Screen)        | `AdminCinemasController`    | `POST /{cinemaId}/screens`            | ⚠️ Thiếu Update/Delete Screen |
| 3   | **Quản lý Ghế** (Bulk create)           | `AdminCinemasController`    | `POST /screens/{screenId}/seats/bulk` | ⚠️ Thiếu Update/Delete Seat   |
| 4   | **Quản lý Phim** (CRUD Movie)           | `AdminMoviesController`     | `POST`, `PUT /{id}`, `DELETE /{id}`   | ✅ Có                         |
| 5   | **Quản lý Khuyến mãi** (CRUD Promotion) | `AdminPromotionsController` | Full CRUD + Activate/Deactivate       | ✅ Đầy đủ                     |
| 6   | **Phân công Nhân sự** (Assign Staff)    | `AdminStaffController`      | `POST /assignments`                   | ⚠️ Thiếu nhiều                |
| 7   | **Quản lý Thể loại** (Genre)            | `GenresController`          | `GET`, `POST`, `PUT /{id}`            | ⚠️ Thiếu Delete               |
| 8   | **Quản lý Loại ghế** (SeatType)         | `SeatTypesController`       | `GET`, `POST`, `PUT /{id}`            | ⚠️ Thiếu Delete               |
| 9   | **Quản lý Bảng giá** (PricingTier)      | `PricingTiersController`    | `GET` (chỉ đọc)                       | ❌ Thiếu CUD                  |
| 10  | **Quản lý Khung giờ** (TimeSlot)        | `TimeSlotsController`       | `GET` (chỉ đọc)                       | ❌ Thiếu CUD                  |
| 11  | **Quản lý Role**                        | `RoleController`            | Full CRUD                             | ✅ Đầy đủ                     |
| 12  | **Tạo tài khoản Staff**                 | `IdentityController`        | `POST /staff`                         | ✅ Có                         |
| 13  | **Cập nhật vai trò User**               | `IdentityController`        | `PUT /users/{userId}/role`            | ✅ Có                         |
| 14  | **Dashboard/Báo cáo**                   | _(Chưa có controller)_      | —                                     | ❌ Thiếu hoàn toàn            |
| 15  | **Quản lý Equipment**                   | _(Chưa có controller)_      | —                                     | ❌ Thiếu hoàn toàn            |
| 16  | **Quản lý Users**                       | _(Chưa có controller)_      | —                                     | ❌ Thiếu                      |

#### Nhận xét Admin:

- ✅ CRUD cơ bản cho Cinema, Movie, Promotion đã có
- ❌ **Thiếu nghiêm trọng:** Dashboard, báo cáo doanh thu, quản lý user, quản lý equipment
- ❌ **Thiếu:** CRUD đầy đủ cho PricingTier, TimeSlot, Screen, Seat
- ⚠️ **Chưa tách biệt rõ ràng** giữa SuperAdmin và Admin

---

### 2.2 🟡 MANAGER (Quản Lý Rạp)

> **Nhiệm vụ:** Quản lý hoạt động tại 1 rạp chiếu phim cụ thể, điều phối lịch chiếu, nhân viên.

#### Nghiệp vụ chính:

| STT | Nghiệp vụ                           | Controller Hiện Tại          | API Endpoints                              | Trạng Thái         |
| --- | ----------------------------------- | ---------------------------- | ------------------------------------------ | ------------------ |
| 1   | **Quản lý Lịch chiếu**              | `ManagerShowtimesController` | Full CRUD + Bulk + Confirm                 | ✅ Đầy đủ          |
| 2   | **Xem Lịch chiếu theo rạp**         | `ManagerShowtimesController` | `GET /{cinemaId}`, `GET /detail/{id}`      | ✅ Có              |
| 3   | **Quản lý Tồn kho** (Bắp nước)      | `InventoryController`        | `GET /{cinemaId}`, `POST`, `POST /restock` | ✅ Có              |
| 4   | **Xem doanh thu Bắp nước**          | `ConcessionsController`      | `GET /{cinemaId}`                          | ⚠️ Cơ bản          |
| 5   | **Quản lý Ghế** (Block/Link/Unlink) | `CinemasController`          | Block/Link/Unlink Seat                     | ✅ Có              |
| 6   | **Quản lý Ca làm** (Shift/Schedule) | _(Chưa có controller)_       | —                                          | ❌ Thiếu hoàn toàn |
| 7   | **Xem danh sách đặt vé tại rạp**    | _(Chưa có)_                  | —                                          | ❌ Thiếu           |
| 8   | **Dashboard rạp**                   | _(Chưa có)_                  | —                                          | ❌ Thiếu           |
| 9   | **Duyệt Hoàn tiền**                 | `BookingsController`         | `POST /{id}/approve-refund`                | ⚠️ Chưa tách role  |
| 10  | **Quản lý Khuyến mãi** (view)       | `AdminPromotionsController`  | Authorized: `Admin,Manager`                | ✅ Có              |

#### Nhận xét Manager:

- ✅ Lịch chiếu đã rất đầy đủ (CRUD, bulk create, confirm)
- ❌ **Thiếu nghiêm trọng:** Quản lý ca làm/lịch làm nhân viên (domain entity `Shift`, `WorkSchedule` đã có nhưng chưa có API)
- ❌ **Thiếu:** Dashboard rạp, xem booking theo rạp, báo cáo doanh thu rạp
- ⚠️ Seat management nằm trong `CinemasController` (public), cần tách ra

---

### 2.3 🟢 STAFF (Nhân Viên Quầy)

> **Nhiệm vụ:** Bán vé tại quầy, bán bắp nước, check-in khách hàng. **Manager và Staff cùng làm việc tại quầy.**

#### Nghiệp vụ chính:

| STT | Nghiệp vụ                                 | Controller Hiện Tại   | API Endpoints            | Trạng Thái            |
| --- | ----------------------------------------- | --------------------- | ------------------------ | --------------------- |
| 1   | **Bán vé tại quầy** (Counter Booking)     | `PosController`       | `POST /bookings/counter` | ✅ Có                 |
| 2   | **Bán vé + bắp nước combo** (Unified POS) | `PosController`       | `POST /sales/unified`    | ✅ Có                 |
| 3   | **Bán bắp nước riêng**                    | `PosController`       | `POST /concessions`      | ✅ Có                 |
| 4   | **Check-in bằng ID**                      | `BookingsController`  | `POST /{id}/check-in`    | ✅ Có                 |
| 5   | **Check-in bằng QR Code**                 | `BookingsController`  | `POST /check-in` (body)  | ✅ Có                 |
| 6   | **Xem thông tin booking**                 | `BookingsController`  | `GET /{id}`              | ✅ Có                 |
| 7   | **Hủy booking** (tại quầy)                | `BookingsController`  | `POST /{id}/cancel`      | ⚠️ Chưa phân quyền rõ |
| 8   | **Xem lịch chiếu hôm nay**                | —                     | —                        | ❌ Thiếu (cho POS)    |
| 9   | **Xem ghế trống theo suất chiếu**         | `ShowtimesController` | `GET /{id}/seating-plan` | ✅ Có                 |
| 10  | **Xem tồn kho bắp nước**                  | `InventoryController` | `GET /{cinemaId}`        | ✅ Có                 |

#### Nhận xét Staff:

- ✅ POS flow đã tốt (counter booking, unified sale, concession-only)
- ✅ Check-in (cả ID và QR Code) đã có
- ❌ **Thiếu:** Xem lịch chiếu hôm nay tại rạp (cho giao diện POS)
- ⚠️ Booking cancel/refund chưa phân quyền rõ ràng Staff vs Customer

---

### 2.4 🔵 CUSTOMER (Khách Hàng)

> **Nhiệm vụ:** Đặt vé online, xem phim, quản lý tài khoản cá nhân.

#### Nghiệp vụ chính:

| STT | Nghiệp vụ                    | Controller Hiện Tại      | API Endpoints                    | Trạng Thái             |
| --- | ---------------------------- | ------------------------ | -------------------------------- | ---------------------- |
| 1   | **Đăng ký tài khoản**        | `AuthController`         | `POST /register`                 | ✅ Có                  |
| 2   | **Đăng nhập**                | `AuthController`         | `POST /login`                    | ✅ Có                  |
| 3   | **Refresh Token**            | `AuthController`         | `POST /refresh`                  | ✅ Có                  |
| 4   | **Quên mật khẩu (OTP)**      | `IdentityController`     | `POST /forgot-password-with-otp` | ✅ Có                  |
| 5   | **Xác thực OTP**             | `IdentityController`     | `POST /verify-reset-otp`         | ✅ Có                  |
| 6   | **Đặt lại mật khẩu**         | `IdentityController`     | `POST /reset-password-with-otp`  | ✅ Có                  |
| 7   | **Xem/Sửa hồ sơ**            | `IdentityController`     | `GET/PUT /profile/{userId}`      | ✅ Có                  |
| 8   | **Đổi mật khẩu**             | `IdentityController`     | `POST /change-password`          | ✅ Có                  |
| 9   | **Xem danh sách phim**       | `MoviesController`       | `GET /` (paged)                  | ✅ Có                  |
| 10  | **Xem chi tiết phim**        | `MoviesController`       | `GET /{id}`                      | ✅ Có                  |
| 11  | **Xem lịch chiếu theo phim** | `ShowtimesController`    | `GET /movie/{movieId}`           | ✅ Có                  |
| 12  | **Xem lịch chiếu theo rạp**  | `ShowtimesController`    | `GET /cinema/{cinemaId}`         | ✅ Có                  |
| 13  | **Xem sơ đồ ghế**            | `ShowtimesController`    | `GET /{id}/seating-plan`         | ✅ Có                  |
| 14  | **Đặt vé online**            | `BookingsController`     | `POST /`                         | ✅ Có                  |
| 15  | **Thanh toán callback**      | `BookingsController`     | `GET /callback`                  | ✅ Có                  |
| 16  | **Xem danh sách vé đã đặt**  | `BookingsController`     | `GET /my` (paged)                | ✅ Có                  |
| 17  | **Xem chi tiết booking**     | `BookingsController`     | `GET /{id}`                      | ✅ Có                  |
| 18  | **Hủy booking**              | `BookingsController`     | `POST /{id}/cancel`              | ✅ Có                  |
| 19  | **Yêu cầu hoàn tiền**        | `BookingsController`     | `POST /{id}/request-refund`      | ✅ Có                  |
| 20  | **Xem danh sách rạp**        | `CinemasController`      | `GET /`                          | ✅ Có                  |
| 21  | **Xem khuyến mãi**           | `PromotionsController`   | `GET /` (active only)            | ✅ Có                  |
| 22  | **Validate mã KM**           | `PromotionsController`   | `GET /validate`                  | ✅ Có                  |
| 23  | **Xem thể loại**             | `GenresController`       | `GET /`                          | ✅ Có (AllowAnonymous) |
| 24  | **Xem loại ghế**             | `SeatTypesController`    | `GET /`                          | ✅ Có (AllowAnonymous) |
| 25  | **Xem bảng giá**             | `PricingTiersController` | `GET /`                          | ✅ Có (AllowAnonymous) |
| 26  | **Xem khung giờ**            | `TimeSlotsController`    | `GET /`                          | ✅ Có (AllowAnonymous) |
| 27  | **Đăng xuất**                | —                        | —                                | ❌ Thiếu               |
| 28  | **Xem lịch sử giao dịch**    | —                        | —                                | ❌ Thiếu               |

#### Nhận xét Customer:

- ✅ **Rất tốt:** Flow đặt vé online hoàn chỉnh (xem phim → chọn suất → chọn ghế → đặt vé → thanh toán)
- ✅ Auth flow đầy đủ (register, login, refresh, OTP reset password)
- ❌ **Thiếu:** API Logout (invalidate refresh token), lịch sử giao dịch chi tiết

---

## 3. Bảng Tổng Hợp API Hiện Tại

### 3.1 Phân loại theo Actor

| Controller                   | Route                      | Actor                    | Số Endpoints |
| ---------------------------- | -------------------------- | ------------------------ | ------------ |
| `AdminCinemasController`     | `api/AdminCinemas` ⚠️      | Admin                    | 5            |
| `AdminMoviesController`      | `api/AdminMovies` ⚠️       | Admin                    | 3            |
| `AdminPromotionsController`  | `api/admin/promotions` ✅  | Admin, Manager           | 7            |
| `AdminStaffController`       | `api/admin/staff` ✅       | Admin                    | 1            |
| `AuthController`             | `api/Auth`                 | Public/All               | 3            |
| `BookingsController`         | `api/Bookings`             | Customer, Staff, Manager | 8            |
| `CinemasController`          | `api/Cinemas`              | Public + Manager⚠️       | 4            |
| `ConcessionsController`      | `api/Concessions`          | Manager, Staff           | 2            |
| `GenresController`           | `api/Genres`               | Public + Admin           | 3            |
| `IdentityController`         | `api/Identity`             | All (mixed)              | 6            |
| `InventoryController`        | `api/Inventory`            | Manager, Staff           | 3            |
| `ManagerShowtimesController` | `api/manager/showtimes` ✅ | Manager                  | 7            |
| `MoviesController`           | `api/Movies`               | Public                   | 2            |
| `PosController`              | `api/pos` ✅               | Staff, Manager           | 3            |
| `PricingTiersController`     | `api/PricingTiers`         | Public                   | 1            |
| `PromotionsController`       | `api/Promotions`           | Public                   | 2            |
| `RoleController`             | `api/Role`                 | Admin                    | 5            |
| `SeatTypesController`        | `api/SeatTypes`            | Public + Admin           | 3            |
| `ShowtimesController`        | `api/Showtimes`            | Public                   | 3            |
| `TimeSlotsController`        | `api/TimeSlots`            | Public                   | 1            |

**Tổng: ~62 API endpoints**

---

## 4. Đánh Giá Tên Controller

### 4.1 ✅ Tên Controller PHÙ HỢP

| Controller                   | Lý do                                                     |
| ---------------------------- | --------------------------------------------------------- |
| `AdminPromotionsController`  | Route `api/admin/promotions` — rõ ràng, có prefix "admin" |
| `AdminStaffController`       | Route `api/admin/staff` — đúng convention                 |
| `ManagerShowtimesController` | Route `api/manager/showtimes` — rõ role + resource        |
| `PosController`              | Route `api/pos` — đúng nghiệp vụ POS                      |
| `AuthController`             | Route `api/Auth` — đúng cho authentication                |
| `MoviesController`           | Route `api/Movies` — public API cho movies                |
| `ShowtimesController`        | Route `api/Showtimes` — public API cho showtimes          |
| `PromotionsController`       | Route `api/Promotions` — public API cho promotions        |

### 4.2 ⚠️ Tên Controller CẦN CẢI THIỆN

| Controller               | Vấn đề                                                       | Đề xuất                                                     |
| ------------------------ | ------------------------------------------------------------ | ----------------------------------------------------------- |
| `AdminCinemasController` | Route tự sinh = `api/AdminCinemas` (không có prefix)         | Đổi route thành `api/admin/cinemas`                         |
| `AdminMoviesController`  | Route tự sinh = `api/AdminMovies`                            | Đổi route thành `api/admin/movies`                          |
| `CinemasController`      | Trộn API public (GetAll) + quản trị (Block/Link/Unlink Seat) | Tách seat management ra `ManagerSeatsController`            |
| `GenresController`       | Trộn public (GetAll) + admin (Create/Update)                 | Tách admin: `AdminGenresController`                         |
| `SeatTypesController`    | Trộn public (GetAll) + admin (Create/Update)                 | Tách admin: `AdminSeatTypesController`                      |
| `IdentityController`     | Trộn customer (profile) + admin (create staff, update role)  | Tách: `ProfileController` + `AdminUsersController`          |
| `BookingsController`     | Trộn customer + staff + manager operations                   | Cân nhắc tách `StaffBookingsController` cho check-in/cancel |
| `RoleController`         | Route `api/Role` (singular)                                  | Đổi thành `api/admin/roles` (plural + admin prefix)         |
| `ConcessionsController`  | Nằm ngoài POS context                                        | Cân nhắc gộp vào `ManagerReportsController`                 |

### 4.3 Đề Xuất Convention Chuẩn

```
Route Pattern:
  api/{actor-prefix}/{resource}      → Cho role-specific APIs
  api/{resource}                    → Cho public APIs

Quy tắc:
  ✅ api/admin/cinemas              → Admin quản lý cinemas
  ✅ api/manager/showtimes          → Manager quản lý showtimes
  ✅ api/pos/bookings/counter       → Staff POS operations
  ✅ api/movies                     → Public movie listings
  ❌ api/AdminCinemas               → Tránh PascalCase trong route
  ❌ api/Role                       → Tránh singular, thiếu prefix
```

---

## 5. Phân Tích API Còn Thiếu

### 5.1 ❌ Thiếu Nghiêm Trọng (Critical — Ảnh hưởng vận hành doanh nghiệp)

#### A. Dashboard & Báo Cáo (Cả Admin lẫn Manager đều cần)

```
Hiện trạng: Application/Features/Dashboard/Queries/ đã có folder nhưng CHƯA CÓ controller.
```

| API Cần Có                               | Actor   | Mô tả                                                     |
| ---------------------------------------- | ------- | --------------------------------------------------------- |
| `GET api/admin/dashboard/revenue`        | Admin   | Doanh thu toàn chuỗi (theo ngày/tuần/tháng)               |
| `GET api/admin/dashboard/stats`          | Admin   | Thống kê tổng quan (tổng booking, tổng rạp, tổng phim...) |
| `GET api/admin/dashboard/top-movies`     | Admin   | Top phim ăn khách nhất                                    |
| `GET api/admin/dashboard/occupancy-rate` | Admin   | Tỉ lệ lấp đầy ghế                                         |
| `GET api/manager/dashboard/revenue`      | Manager | Doanh thu rạp đang quản lý                                |
| `GET api/manager/dashboard/stats`        | Manager | Thống kê rạp (bookings hôm nay, tổng doanh thu tháng...)  |
| `GET api/manager/dashboard/daily-report` | Manager | Báo cáo cuối ngày                                         |

#### B. Quản Lý Ca Làm & Lịch Làm Của Nhân Viên

```
Hiện trạng: Domain entity Shift, WorkSchedule ĐÃ CÓ nhưng CHƯA CÓ API.
```

| API Cần Có                       | Actor   | Mô tả                      |
| -------------------------------- | ------- | -------------------------- |
| `POST api/manager/shifts`        | Manager | Tạo ca làm                 |
| `PUT api/manager/shifts/{id}`    | Manager | Cập nhật ca làm            |
| `DELETE api/manager/shifts/{id}` | Manager | Xóa ca làm                 |
| `GET api/manager/shifts`         | Manager | Xem danh sách ca làm       |
| `POST api/manager/schedules`     | Manager | Tạo lịch làm cho nhân viên |
| `GET api/manager/schedules`      | Manager | Xem lịch làm theo tuần     |
| `PUT api/manager/schedules/{id}` | Manager | Cập nhật lịch làm          |

#### C. Quản Lý Người Dùng (Admin)

```
Hiện trạng: Identity system có nhưng CHƯA CÓ API quản lý users.
```

| API Cần Có                        | Actor | Mô tả                                  |
| --------------------------------- | ----- | -------------------------------------- |
| `GET api/admin/users`             | Admin | Danh sách tất cả users (paged, filter) |
| `GET api/admin/users/{id}`        | Admin | Chi tiết user                          |
| `PUT api/admin/users/{id}/status` | Admin | Khóa/Mở khóa tài khoản                 |
| `DELETE api/admin/users/{id}`     | Admin | Xóa mềm tài khoản                      |
| `GET api/admin/users/staff`       | Admin | Danh sách nhân viên                    |

---

### 5.2 ⚠️ Thiếu Quan Trọng (Important — Cần cho vận hành mượt mà)

#### D. Equipment Management (Thiết bị rạp)

```
Hiện trạng: Domain entity Equipment, MaintenanceLog ĐÃ CÓ nhưng CHƯA CÓ API.
```

| API Cần Có                                        | Actor   | Mô tả              |
| ------------------------------------------------- | ------- | ------------------ |
| `POST api/admin/equipment`                        | Admin   | Thêm thiết bị      |
| `PUT api/admin/equipment/{id}`                    | Admin   | Cập nhật thiết bị  |
| `GET api/admin/equipment`                         | Admin   | Danh sách thiết bị |
| `GET api/manager/equipment`                       | Manager | Thiết bị tại rạp   |
| `POST api/manager/equipment/{id}/maintenance`     | Manager | Ghi nhận bảo trì   |
| `GET api/manager/equipment/{id}/maintenance-logs` | Manager | Lịch sử bảo trì    |

#### E. Admin Screen & Seat Management (CRUD đầy đủ)

| API Cần Có                                                   | Actor | Mô tả                 |
| ------------------------------------------------------------ | ----- | --------------------- |
| `PUT api/admin/cinemas/{cinemaId}/screens/{screenId}`        | Admin | Cập nhật phòng chiếu  |
| `DELETE api/admin/cinemas/{cinemaId}/screens/{screenId}`     | Admin | Xóa phòng chiếu       |
| `GET api/admin/cinemas/{cinemaId}/screens`                   | Admin | Danh sách phòng chiếu |
| `DELETE api/admin/cinemas/screens/{screenId}/seats/{seatId}` | Admin | Xóa ghế               |
| `PUT api/admin/cinemas/screens/{screenId}/seats/{seatId}`    | Admin | Cập nhật ghế          |

#### F. Shared Resources CRUD (Cho Admin)

| API Cần Có                            | Actor | Mô tả              |
| ------------------------------------- | ----- | ------------------ |
| `POST api/admin/pricing-tiers`        | Admin | Tạo bảng giá       |
| `PUT api/admin/pricing-tiers/{id}`    | Admin | Cập nhật bảng giá  |
| `DELETE api/admin/pricing-tiers/{id}` | Admin | Xóa bảng giá       |
| `POST api/admin/time-slots`           | Admin | Tạo khung giờ      |
| `PUT api/admin/time-slots/{id}`       | Admin | Cập nhật khung giờ |
| `DELETE api/admin/time-slots/{id}`    | Admin | Xóa khung giờ      |
| `DELETE api/admin/genres/{id}`        | Admin | Xóa thể loại       |
| `DELETE api/admin/seat-types/{id}`    | Admin | Xóa loại ghế       |

---

### 5.3 💡 Thiếu Bổ Sung (Nice-to-have — Nâng cao trải nghiệm)

#### G. Customer Enhancements

| API Cần Có                       | Actor    | Mô tả                                         |
| -------------------------------- | -------- | --------------------------------------------- |
| `POST api/auth/logout`           | Customer | Đăng xuất (invalidate refresh token)          |
| `GET api/bookings/history`       | Customer | Lịch sử giao dịch chi tiết                    |
| `GET api/movies/now-showing`     | Public   | Phim đang chiếu                               |
| `GET api/movies/coming-soon`     | Public   | Phim sắp chiếu                                |
| `GET api/movies/search`          | Public   | Tìm kiếm phim (keyword, genre, rating)        |
| `GET api/cinemas/{id}`           | Public   | Chi tiết rạp (địa chỉ, phòng chiếu, tiện ích) |
| `GET api/cinemas/{id}/showtimes` | Public   | Tất cả suất chiếu theo rạp (grouped by movie) |

#### H. Manager — Quản Lý Booking Tại Rạp

| API Cần Có                                 | Actor   | Mô tả                                  |
| ------------------------------------------ | ------- | -------------------------------------- |
| `GET api/manager/bookings`                 | Manager | Danh sách booking tại rạp mình quản lý |
| `GET api/manager/bookings/today`           | Manager | Booking hôm nay                        |
| `GET api/manager/bookings/refund-requests` | Manager | Các yêu cầu hoàn tiền chờ duyệt        |
| `GET api/manager/staff`                    | Manager | Danh sách nhân viên thuộc rạp          |

#### I. Staff POS Enhancements

| API Cần Có                        | Actor | Mô tả                                       |
| --------------------------------- | ----- | ------------------------------------------- |
| `GET api/pos/showtimes/today`     | Staff | Lịch chiếu hôm nay tại rạp (cho POS screen) |
| `GET api/pos/booking/{id}/print`  | Staff | In vé (lấy data để in)                      |
| `GET api/pos/sales/daily-summary` | Staff | Tổng kết bán hàng trong ca                  |

---

## 6. Đề Xuất API Cần Thiết Bổ Sung

### 6.1 Đề Xuất Controller Mới

| STT | Controller Mới                | Route                     | Actor    | Mô tả                                             |
| --- | ----------------------------- | ------------------------- | -------- | ------------------------------------------------- |
| 1   | `AdminDashboardController`    | `api/admin/dashboard`     | Admin    | Dashboard & báo cáo toàn chuỗi                    |
| 2   | `AdminUsersController`        | `api/admin/users`         | Admin    | Quản lý users (list, lock, role)                  |
| 3   | `AdminEquipmentController`    | `api/admin/equipment`     | Admin    | Quản lý thiết bị hệ thống                         |
| 4   | `AdminScreensController`      | `api/admin/screens`       | Admin    | CRUD phòng chiếu (tách từ AdminCinemas)           |
| 5   | `AdminPricingTiersController` | `api/admin/pricing-tiers` | Admin    | CRUD bảng giá                                     |
| 6   | `AdminTimeSlotsController`    | `api/admin/time-slots`    | Admin    | CRUD khung giờ                                    |
| 7   | `AdminGenresController`       | `api/admin/genres`        | Admin    | CRUD thể loại (tách từ GenresController)          |
| 8   | `AdminSeatTypesController`    | `api/admin/seat-types`    | Admin    | CRUD loại ghế (tách từ SeatTypesController)       |
| 9   | `ManagerDashboardController`  | `api/manager/dashboard`   | Manager  | Dashboard & báo cáo rạp                           |
| 10  | `ManagerShiftsController`     | `api/manager/shifts`      | Manager  | Quản lý ca làm                                    |
| 11  | `ManagerSchedulesController`  | `api/manager/schedules`   | Manager  | Lịch làm nhân viên                                |
| 12  | `ManagerBookingsController`   | `api/manager/bookings`    | Manager  | Booking tại rạp + refund                          |
| 13  | `ManagerStaffController`      | `api/manager/staff`       | Manager  | Xem nhân viên tại rạp                             |
| 14  | `ManagerEquipmentController`  | `api/manager/equipment`   | Manager  | Thiết bị + bảo trì tại rạp                        |
| 15  | `ManagerSeatsController`      | `api/manager/seats`       | Manager  | Block/Link/Unlink ghế (tách từ CinemasController) |
| 16  | `ProfileController`           | `api/profile`             | Customer | Hồ sơ cá nhân (tách từ IdentityController)        |

### 6.2 Controller Hiện Tại Cần Fix Route

| Controller               | Route Hiện Tại     | Route Đề Xuất       |
| ------------------------ | ------------------ | ------------------- |
| `AdminCinemasController` | `api/AdminCinemas` | `api/admin/cinemas` |
| `AdminMoviesController`  | `api/AdminMovies`  | `api/admin/movies`  |
| `RoleController`         | `api/Role`         | `api/admin/roles`   |

### 6.3 Tổng Kết Ưu Tiên Triển Khai

```
🔴 P0 — Bắt buộc (Sprint tiếp theo):
   ├── AdminDashboardController (doanh thu, thống kê)
   ├── ManagerDashboardController (báo cáo rạp)
   ├── ManagerShiftsController + ManagerSchedulesController (ca làm)
   ├── AdminUsersController (quản lý users)
   └── Fix route conventions cho AdminCinemas, AdminMovies, RoleController

🟡 P1 — Quan trọng (Sprint sau P0):
   ├── AdminEquipmentController + ManagerEquipmentController
   ├── AdminPricingTiersController, AdminTimeSlotsController (CRUD đầy đủ)
   ├── ManagerBookingsController (booking tại rạp)
   └── Tách IdentityController → ProfileController + AdminUsersController

🟢 P2 — Nâng cao (Backlog):
   ├── Tách GenresController, SeatTypesController → Admin versions
   ├── Customer enhancements (logout, search, history)
   ├── POS enhancements (daily summary, print ticket)
   └── ManagerSeatsController (tách từ CinemasController)
```

---

## Phụ Lục: Sơ Đồ Phân Quyền API

```mermaid
graph TB
    subgraph "PUBLIC (AllowAnonymous)"
        A1[GET /api/movies]
        A2[GET /api/showtimes/movie/{id}]
        A3[GET /api/showtimes/cinema/{id}]
        A4[GET /api/cinemas]
        A5[GET /api/promotions]
        A6[GET /api/genres]
        A7[GET /api/seat-types]
        A8[GET /api/pricing-tiers]
        A9[GET /api/time-slots]
    end

    subgraph "AUTH (Public)"
        B1[POST /api/auth/register]
        B2[POST /api/auth/login]
        B3[POST /api/auth/refresh]
        B4[POST /api/identity/forgot-password]
    end

    subgraph "CUSTOMER (Authenticated)"
        C1[POST /api/bookings]
        C2[GET /api/bookings/my]
        C3[POST /api/bookings/{id}/cancel]
        C4[POST /api/bookings/{id}/request-refund]
        C5[GET/PUT /api/identity/profile/{userId}]
    end

    subgraph "STAFF (Role: Staff)"
        D1[POST /api/pos/bookings/counter]
        D2[POST /api/pos/sales/unified]
        D3[POST /api/pos/concessions]
        D4[POST /api/bookings/{id}/check-in]
    end

    subgraph "MANAGER (Role: Manager)"
        E1[CRUD /api/manager/showtimes]
        E2[CRUD /api/manager/shifts — CẦN THÊM]
        E3[GET /api/manager/dashboard — CẦN THÊM]
        E4[POST /api/bookings/{id}/approve-refund]
    end

    subgraph "ADMIN (Role: Admin)"
        F1[CRUD /api/admin/cinemas]
        F2[CRUD /api/admin/movies]
        F3[CRUD /api/admin/promotions]
        F4[POST /api/admin/staff/assignments]
        F5[CRUD /api/admin/roles]
        F6[GET /api/admin/dashboard — CẦN THÊM]
        F7[CRUD /api/admin/users — CẦN THÊM]
    end
```

---

> **Kết luận:** Hệ thống đã có nền tảng tốt với flow đặt vé online hoàn chỉnh và POS counter. Tuy nhiên, cần bổ sung urgently: **Dashboard/Báo cáo**, **Quản lý ca làm nhân viên**, **Quản lý users**, và **fix route conventions**. Các domain entity cho Shift/WorkSchedule/Equipment đã sẵn sàng, chỉ cần implement API layer.
