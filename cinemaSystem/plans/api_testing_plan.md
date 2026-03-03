# Cinema System API - Testing Priority Plan

## Overview

This document lists all APIs in the Cinema System project with testing priorities based on CRUD operations SQL database to.

---

## PRIORITY 1: Core Master Data APIs (Test First - Foundation Data)

These APIs require basic CRUD and are prerequisites for other features.

### 1.1 Genres API (`/api/genres`)

| Method | Endpoint           | Description    | SQL Operation |
| ------ | ------------------ | -------------- | ------------- |
| GET    | `/api/genres`      | Get all genres | READ          |
| POST   | `/api/genres`      | Create genre   | CREATE        |
| PUT    | `/api/genres/{id}` | Update genre   | UPDATE        |

**Domain Entity**: `Genre`

### 1.2 Seat Types API (`/api/seattypes`)

| Method | Endpoint              | Description        | SQL Operation |
| ------ | --------------------- | ------------------ | ------------- |
| GET    | `/api/seattypes`      | Get all seat types | READ          |
| POST   | `/api/seattypes`      | Create seat type   | CREATE        |
| PUT    | `/api/seattypes/{id}` | Update seat type   | UPDATE        |

**Domain Entity**: `SeatType`

### 1.3 Pricing Tiers API (`/api/pricingtiers`)

| Method | Endpoint            | Description           | SQL Operation |
| ------ | ------------------- | --------------------- | ------------- |
| GET    | `/api/pricingtiers` | Get all pricing tiers | READ          |

**Domain Entity**: `PricingTier`

### 1.4 Time Slots API (`/api/timeslots`)

| Method | Endpoint         | Description        | SQL Operation |
| ------ | ---------------- | ------------------ | ------------- |
| GET    | `/api/timeslots` | Get all time slots | READ          |

**Domain Entity**: `TimeSlot`

### 1.5 Roles API (`/api/roles`)

| Method | Endpoint              | Description    | SQL Operation |
| ------ | --------------------- | -------------- | ------------- |
| GET    | `/api/roles`          | Get all roles  | READ          |
| GET    | `/api/roles/{roleId}` | Get role by ID | READ          |
| POST   | `/api/roles`          | Create role    | CREATE        |
| PUT    | `/api/roles/{roleId}` | Update role    | UPDATE        |
| DELETE | `/api/roles/{roleId}` | Delete role    | DELETE        |

---

## PRIORITY 2: Authentication & Identity APIs

### 2.1 Auth API (`/api/auth`)

| Method | Endpoint             | Description       | SQL Operation |
| ------ | -------------------- | ----------------- | ------------- |
| POST   | `/api/auth/register` | Register new user | CREATE        |
| POST   | `/api/auth/login`    | Login user        | READ          |
| POST   | `/api/auth/refresh`  | Refresh token     | READ          |

### 2.2 Identity API (`/api/identity`)

| Method | Endpoint                                 | Description            | SQL Operation |
| ------ | ---------------------------------------- | ---------------------- | ------------- |
| GET    | `/api/identity/profile/{userId}`         | Get user profile       | READ          |
| PUT    | `/api/identity/profile/{userId}`         | Update profile         | UPDATE        |
| POST   | `/api/identity/change-password`          | Change password        | UPDATE        |
| POST   | `/api/identity/forgot-password-with-otp` | Request password reset | READ          |
| POST   | `/api/identity/verify-reset-otp`         | Verify OTP             | READ          |
| POST   | `/api/identity/reset-password-with-otp`  | Reset password         | UPDATE        |
| POST   | `/api/identity/staff`                    | Create staff account   | CREATE        |
| PUT    | `/api/identity/users/{userId}/role`      | Update user role       | UPDATE        |

---

## PRIORITY 3: Movies & Cinemas (Business Core)

### 3.1 Movies API - Public (`/api/movies`)

| Method | Endpoint           | Description        | SQL Operation |
| ------ | ------------------ | ------------------ | ------------- |
| GET    | `/api/movies`      | Get movies (paged) | READ          |
| GET    | `/api/movies/{id}` | Get movie details  | READ          |

**Domain Entity**: `Movie`, `MovieGenre`, `MovieCastCrew`, `MovieCertification`

### 3.2 Admin Movies API (`/api/admin/movies`)

| Method | Endpoint                 | Description  | SQL Operation |
| ------ | ------------------------ | ------------ | ------------- |
| POST   | `/api/admin/movies`      | Create movie | CREATE        |
| PUT    | `/api/admin/movies/{id}` | Update movie | UPDATE        |
| DELETE | `/api/admin/movies/{id}` | Delete movie | DELETE        |

### 3.3 Cinemas API - Public (`/api/cinemas`)

| Method | Endpoint       | Description     | SQL Operation |
| ------ | -------------- | --------------- | ------------- |
| GET    | `/api/cinemas` | Get all cinemas | READ          |

**Domain Entity**: `Cinema`

### 3.4 Admin Cinemas API (`/api/admin/cinemas`)

| Method | Endpoint                                               | Description       | SQL Operation |
| ------ | ------------------------------------------------------ | ----------------- | ------------- |
| POST   | `/api/admin/cinemas`                                   | Create cinema     | CREATE        |
| PUT    | `/api/admin/cinemas/{id}`                              | Update cinema     | UPDATE        |
| DELETE | `/api/admin/cinemas/{id}`                              | Delete cinema     | DELETE        |
| POST   | `/api/admin/cinemas/{cinemaId}/screens`                | Create screen     | CREATE        |
| POST   | `/api/admin/cinemas/screens/{screenId}/seats/bulk`     | Create seats bulk | CREATE        |
| POST   | `/api/cinemas/screens/{screenId}/seats/{seatId}/block` | Block seat        | UPDATE        |
| POST   | `/api/cinemas/screens/{screenId}/seats/{seatId}/link`  | Link couple seats | UPDATE        |

**Domain Entity**: `Cinema`, `Screen`, `Seat`

---

## PRIORITY 4: Showtimes

### 4.1 Showtimes API - Public (`/api/showtimes`)

| Method | Endpoint                           | Description             | SQL Operation |
| ------ | ---------------------------------- | ----------------------- | ------------- |
| GET    | `/api/showtimes/movie/{movieId}`   | Get showtimes by movie  | READ          |
| GET    | `/api/showtimes/cinema/{cinemaId}` | Get showtimes by cinema | READ          |
| GET    | `/api/showtimes/{id}/seating-plan` | Get seating plan        | READ          |

**Domain Entity**: `Showtime`, `ShowtimePricing`

### 4.2 Manager Showtimes API (`/api/manager/showtimes`)

| Method | Endpoint                              | Description             | SQL Operation |
| ------ | ------------------------------------- | ----------------------- | ------------- |
| GET    | `/api/manager/showtimes/{cinemaId}`   | Get showtimes by cinema | READ          |
| POST   | `/api/manager/showtimes`              | Create showtime         | CREATE        |
| PUT    | `/api/manager/showtimes/{showtimeId}` | Update showtime         | UPDATE        |
| DELETE | `/api/manager/showtimes/{showtimeId}` | Delete showtime         | DELETE        |

---

## PRIORITY 5: Bookings (Core Business)

### 5.1 Bookings API (`/api/bookings`)

| Method | Endpoint                            | Description       | SQL Operation |
| ------ | ----------------------------------- | ----------------- | ------------- |
| POST   | `/api/bookings`                     | Create booking    | CREATE        |
| GET    | `/api/bookings/callback`            | Payment callback  | UPDATE        |
| POST   | `/api/bookings/{id}/complete`       | Complete booking  | UPDATE        |
| POST   | `/api/bookings/{id}/cancel`         | Cancel booking    | UPDATE        |
| POST   | `/api/bookings/{id}/request-refund` | Request refund    | UPDATE        |
| POST   | `/api/bookings/{id}/approve-refund` | Approve refund    | UPDATE        |
| GET    | `/api/bookings/{id}`                | Get booking by ID | READ          |
| GET    | `/api/bookings/my`                  | Get my bookings   | READ          |
| POST   | `/api/bookings/{id}/check-in`       | Check-in by ID    | UPDATE        |
| POST   | `/api/bookings/check-in`            | Check-in by QR    | UPDATE        |

**Domain Entities**: `Booking`, `BookingTicket`, `Payment`, `Refund`

---

## PRIORITY 6: POS & Concessions

### 6.1 POS API (`/api/pos`)

| Method | Endpoint                    | Description            | SQL Operation |
| ------ | --------------------------- | ---------------------- | ------------- |
| POST   | `/api/pos/bookings/counter` | Counter booking        | CREATE        |
| POST   | `/api/pos/sales/unified`    | Unified POS sale       | CREATE        |
| POST   | `/api/pos/concessions`      | Create concession sale | CREATE        |

### 6.2 Concessions API (`/api/concessions`)

| Method | Endpoint                      | Description | SQL Operation |
| ------ | ----------------------------- | ----------- | ------------- |
| POST   | `/api/concessions`            | Create sale | CREATE        |
| GET    | `/api/concessions/{cinemaId}` | Get sales   | READ          |

**Domain Entities**: `ConcessionSale`, `ConcessionSaleItem`

---

## PRIORITY 7: Inventory

### 7.1 Inventory API (`/api/inventory`)

| Method | Endpoint                    | Description   | SQL Operation |
| ------ | --------------------------- | ------------- | ------------- |
| GET    | `/api/inventory/{cinemaId}` | Get inventory | READ          |
| POST   | `/api/inventory`            | Create item   | CREATE        |
| POST   | `/api/inventory/restock`    | Restock       | UPDATE        |

**Domain Entities**: `InventoryItem`, `InventoryTransaction`

---

## PRIORITY 8: Promotions

### 8.1 Promotions API (`/api/promotions`)

| Method | Endpoint                   | Description           | SQL Operation |
| ------ | -------------------------- | --------------------- | ------------- |
| GET    | `/api/promotions`          | Get active promotions | READ          |
| GET    | `/api/promotions/validate` | Validate promotion    | READ          |

**Domain Entity**: `Promotion`

---

## PRIORITY 9: Staff Management

### 9.1 Admin Staff API (`/api/admin/staff`)

| Method | Endpoint                       | Description  | SQL Operation |
| ------ | ------------------------------ | ------------ | ------------- |
| POST   | `/api/admin/staff/assignments` | Assign staff | CREATE        |

**Domain Entities**: `Staff`, `Shift`, `WorkSchedule`

---

## Testing Sequence Recommendation

```
Phase 1: Setup Foundation Data
├── 1. Genres CRUD
├── 2. Seat Types CRUD
├── 3. Pricing Tiers READ
├── 4. Time Slots READ
└── 5. Roles CRUD

Phase 2: Authentication
├── 1. Register → Login → Get Profile
├── 2. Change Password
├── 3. Forgot Password Flow
└── 4. Create Staff

Phase 3: Core Business Setup
├── 1. Admin: Create Cinema
├── 2. Admin: Create Screen
├── 3. Admin: Create Seats Bulk
├── 4. Admin: Create Movie
├── 5. Public: Get Movies
└── 6. Public: Get Cinemas

Phase 4: Showtimes
├── 1. Manager: Create Showtime
├── 2. Manager: Update Showtime
├── 3. Public: Get Showtimes by Movie
├── 4. Public: Get Showtimes by Cinema
└── 5. Public: Get Seating Plan

Phase 5: Bookings (Critical)
├── 1. Create Booking (Online)
├── 2. Payment Callback
├── 3. Complete Booking
├── 4. Get Booking Details
├── 5. Get My Bookings
├── 6. Cancel Booking
├── 7. Request Refund
├── 8. Approve Refund
└── 9. Check-in

Phase 6: POS Operations
├── 1. Counter Booking
├── 2. Unified POS Sale
└── 3. Concession Sale

Phase 7: Inventory & Promotions
├── 1. Get Inventory
├── 2. Create Inventory Item
├── 3. Restock Inventory
└── 4. Validate Promotion
```

---

## API Summary Statistics

| Category          | Count |
| ----------------- | ----- |
| Total Controllers | 18    |
| Total Endpoints   | ~55   |
| CREATE Operations | ~15   |
| READ Operations   | ~20   |
| UPDATE Operations | ~12   |
| DELETE Operations | ~3    |

---

## Notes

- All endpoints marked with `[Authorize]` require valid JWT token
- Some endpoints have commented-out authorization for development
- Payment callback (`/api/bookings/callback`) is `[AllowAnonymous]`
- Most public read endpoints are `[AllowAnonymous]`
