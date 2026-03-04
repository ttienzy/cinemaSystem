# Cinema System - Showtime Scheduling Improvement Plan (UPDATED)

## Issues Identified & Fixes Required

### 1. GetShowtimeDetailsHandler - CRITICAL ISSUES ❌

**Problems:**

- Hardcoded values: `ScreenName`, `SlotName`, `PricingTierName`, `SeatTypeName`
- Not querying actual tables for Screen, Slot, PricingTier, SeatType
- This defeats the purpose of the endpoint

**Fix Required:**

- Load actual Screen from repository
- Load actual TimeSlot for SlotName
- Load actual PricingTier for PricingTierName/Multiplier
- Load actual SeatTypes for ShowtimePricingResponse

### 2. RBAC in Handlers - REMOVE ❌

**Problems:**

- RBAC logic in handlers is redundant
- Controller already has [Authorize] (commented for testing)
- Duplicate exception classes in each handler

**Fix Required:**

- Remove all RBAC checks from handlers
- Let Controller handle authorization via [Authorize] attribute
- Remove duplicate exception classes from handlers (use existing ones)

### 3. BulkCreateShowtimesHandler - PERFORMANCE ISSUES ❌

**Problems:**

- Querying seatTypes inside a loop (N+1 problem)
- Multiple individual queries for each showtime creation
- Very inefficient for bulk operations

**Fix Required:**

- Batch query all SeatTypes ONCE before the loop
- Use in-memory dictionary lookups instead of queries in loop
- Pre-fetch all TimeSlots upfront

---

## Current Implementation Status

### Completed (Needs Fix):

| Feature             | Status | Issues                                 |
| ------------------- | ------ | -------------------------------------- |
| BulkCreateShowtimes | ⚠️     | Performance - N+1 queries              |
| GetShowtimeDetails  | ❌     | Hardcoded values, not real data        |
| ConfirmShowtime     | ❌     | Duplicate exceptions, unnecessary RBAC |

---

## Required Fixes

### Fix 1: GetShowtimeDetailsHandler

```csharp
// Instead of hardcoded values, need to:
// 1. Load Screen from cinema.GetScreenById() or repository
// 2. Load TimeSlot from ITimeSlotRepository
// 3. Load PricingTier from repository
// 4. Load SeatType names for ShowtimePricing
```

### Fix 2: Remove RBAC from All Handlers

- Delete RBAC checks from:
  - `BulkCreateShowtimesHandler`
  - `GetShowtimeDetailsHandler`
  - `ConfirmShowtimeHandler`
- Remove duplicate exception classes

### Fix 3: Optimize BulkCreateShowtimesHandler

```csharp
// OPTIMIZED APPROACH:
// 1. Batch query ALL seat types ONCE
var allSeatTypes = await seatTypeRepo.GetAllAsync(ct);
var seatTypeDict = allSeatTypes.ToDictionary(s => s.Id);

// 2. Use dictionary in loop - NO additional queries
foreach (...) {
    var seatType = seatTypeDict[spReq.SeatTypeId]; // O(1) lookup
    // ...
}
```

---

## Additional Features to Add

### 1. GetShowtimeById Query

Already exists via GetShowtimeDetails, but needs fixing.

### 2. List All Showtimes (Admin)

```http
GET /api/admin/showtimes? cinemaId=&dateFrom=&dateTo=&status=
```

### 3. Get Available Seats Per Showtime

```http
GET /api/manager/showtimes/{id}/availability
```

---

## Files to Fix

| File                            | Action                                   |
| ------------------------------- | ---------------------------------------- |
| `GetShowtimeDetailsHandler.cs`  | Fix to load real data from DB            |
| `BulkCreateShowtimesHandler.cs` | Remove RBAC, optimize queries            |
| `ConfirmShowtimeCommand.cs`     | Remove RBAC, remove duplicate exceptions |

---

## Recommendations

1. **Use [Authorize] at Controller level** - Let ASP.NET Core handle authorization
2. **Batch queries** - Always fetch related data in bulk before processing
3. **Use dictionaries** - For O(1) lookups instead of repeated queries
4. **Reuse exceptions** - Import from Application.Common.Exceptions
5. **No hardcoding** - Always load actual entity data from repositories
