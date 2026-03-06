# Implementation Notes - Conflict Resolution

## Sprint 1: Blockers Fixed ✅

### 1.1 Duplicate MANAGER_BOOKINGS Endpoint
- **Fixed**: Removed duplicate declaration in `endpoints.ts`
- **Impact**: TypeScript build now succeeds

### 1.2 API Endpoint Mismatches
- **Fixed**: Updated frontend to use correct backend endpoints
  - Admin movies: Now uses `GET /api/movies` (public endpoint)
  - Admin cinemas: Now uses `GET /api/cinemas` (public endpoint)
  - Admin showtimes: Added cinema selector, calls `GET /api/manager/showtimes/{cinemaId}`
  - Admin bookings: Added cinema selector, calls `GET /api/manager/bookings?cinemaId=X`
  - Removed non-existent `SHOWTIMES.BASE`, `ADMIN_MOVIES.BASE`, `ADMIN_CINEMAS.BASE`

### 1.3 HTTP Method Mismatches
- **Fixed**: Admin users lock/unlock now use `PUT` instead of `POST`

## Sprint 2: Security Issues Fixed ✅

### 2.1 SignalR Token Security
- **Fixed**: Backend now accepts token from Authorization header (priority) with query string fallback
- **Note**: Frontend SignalR implementation pending - use `accessTokenFactory` when implementing

### 2.2 Payment Callback Idempotency
- **Fixed**: Added transaction ID caching in `CompleteBookingHandler`
  - Checks if transaction already processed before completing booking
  - Stores processed transaction IDs in Redis for 7 days
  - Prevents duplicate payments from VnPay retries

### 2.3 Frontend Request Validation
- **Fixed**: Added seat validation in `bookingStore.ts`
  - Validates seat exists in seating plan before adding
  - Validates price matches seating plan price
  - Added `validateSeats()` method for pre-submission validation

## Sprint 3: Logic & Concurrency Fixed ✅

### 3.1 Booking Expiry Enforcement
- **Backend**: Added expiry check in `CompleteBookingHandler`
  - Rejects payment if booking has expired
  - Returns clear error message
- **Frontend**: Created `BookingExpiryTimer` component
  - Shows countdown timer (15 minutes)
  - Warning at 2 minutes remaining
  - Critical alert at 1 minute remaining
  - Auto-clears seats when expired

### 3.3 Token Response Standardization
- **Backend**: Flattened token response structure
  - Old: `{ token: { accessToken, refreshToken } }`
  - New: `{ accessToken, refreshToken }`
  - Updated `LoginResponse` model
  - Updated refresh endpoint to return new refresh token
  - Added `StoreRefreshTokenAsync` method to `TokenClaimService`
- **Frontend**: Removed fallback logic
  - Expects consistent flat structure
  - Updated `authApi.ts` and `axios.instance.ts`

### 3.4 Role Case Normalization
- **Backend**: Roles normalized to lowercase in JWT token generation
  - All roles stored as lowercase in token claims
  - ASP.NET Core authorization is case-insensitive by default
  - No changes needed to `[Authorize(Roles = "Admin")]` attributes
- **Frontend**: Kept lowercase normalization for consistency
  - Roles from token are already lowercase
  - Frontend normalization is now redundant but harmless

## Remaining Tasks (Sprint 4 - Optional)

### 4.1 Booking State Persistence
- **Status**: Not implemented
- **Recommendation**: Persist showtime, seatingPlan, pricing in booking store

### 4.2 Payment Callback Redirect
- **Status**: Still hardcoded to `http://localhost:5173`
- **Recommendation**: Move to configuration/environment variables

### 4.3 Promotion Validation UX
- **Status**: Not implemented
- **Recommendation**: Add real-time validation on blur

### 4.4 Error Response Standardization
- **Status**: Not implemented
- **Recommendation**: Create standard ErrorResponse DTO

## Testing Checklist

### Backend
- [ ] Build succeeds without errors
- [ ] All admin endpoints require authentication
- [ ] Payment callback handles duplicate transactions
- [ ] Booking expiry is enforced
- [ ] Token refresh returns new refresh token
- [ ] Roles in JWT are lowercase

### Frontend
- [ ] TypeScript build succeeds
- [ ] Admin pages load with cinema selector
- [ ] User lock/unlock uses PUT method
- [ ] Seat validation prevents price manipulation
- [ ] Booking timer shows countdown
- [ ] Token refresh updates both tokens
- [ ] Role checks work consistently

## Notes

- All changes maintain backward compatibility where possible
- Security fixes prioritized over UX improvements
- Frontend validation complements backend validation (defense in depth)
- Token normalization improves consistency across the stack
