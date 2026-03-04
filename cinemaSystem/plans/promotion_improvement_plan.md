# Cinema System - Promotion Management Improvement Plan

## Current State

- **Domain**: [`Promotion.cs`](Domain/Entities/PromotionAggregate/Promotion.cs) - Already has basic structure
  - ✅ Code, Name, Description
  - ✅ PromotionType (Percentage/FixedAmount)
  - ✅ Value, MaxDiscountAmount, MinOrderValue
  - ✅ MaxUsageCount, CurrentUsageCount
  - ✅ StartDate, EndDate, IsActive
  - ✅ Evaluate() method for discount calculation
- **API**: [`PromotionsController.cs`](Api/Controllers/PromotionsController.cs) - Only has READ endpoints
  - ✅ GET /api/promotions (get active)
  - ✅ GET /api/promotions/validate
  - ❌ Missing: Create, Update, Delete

## Missing Features

### 1. Promotion CRUD Operations (Admin)

- ❌ Create Promotion API
- ❌ Update Promotion API
- ❌ Delete (Deactivate) Promotion API
- ❌ Get All Promotions (including inactive)

### 2. Advanced Constraints

- ❌ Apply to specific Movies (MovieId)
- ❌ Apply to specific Cinemas (CinemaId)
- ❌ Apply to specific Showtime types
- ❌ User usage limit (per user)

---

## Required Changes

### 1.1 Update Domain - Promotion.cs

Add new fields for specific targeting:

```csharp
// Add to Promotion entity
public Guid? SpecificMovieId { get; private set; }      // Apply to specific movie
public Guid? SpecificCinemaId { get; private set; }     // Apply to specific cinema
public int? MaxUsagePerUser { get; private set; }       // Limit per user

public void UpdateDetails(...)
{
    // Add update method with new fields
}
```

### 1.2 Update Domain - Add PromotionValidation

Update Evaluate() method:

```csharp
public (bool CanApply, decimal DiscountAmount, string Reason) Evaluate(
    decimal orderTotal,
    Guid? movieId = null,
    Guid? cinemaId = null,
    Guid? userId = null,
    int userUsageCount = 0)
{
    // Existing validations...

    // New: Check specific movie
    if (SpecificMovieId.HasValue && movieId != SpecificMovieId.Value)
        return (false, 0, "Promotion not valid for this movie.");

    // New: Check specific cinema
    if (SpecificCinemaId.HasValue && cinemaId != SpecificCinemaId.Value)
        return (false, 0, "Promotion not valid for this cinema.");

    // New: Check per-user limit
    if (MaxUsagePerUser.HasValue && userUsageCount >= MaxUsagePerUser.Value)
        return (false, 0, "You have reached the maximum usage limit for this promotion.");

    // ... rest of existing logic
}
```

### 1.3 Create Promotion DTOs

```csharp
public class PromotionUpsertRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Type { get; set; }  // "Percentage" or "FixedAmount"
    public required decimal Value { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinOrderValue { get; set; }
    public required int MaxUsageCount { get; set; }
    public int? MaxUsagePerUser { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Targeting
    public Guid? SpecificMovieId { get; set; }
    public Guid? SpecificCinemaId { get; set; }
}
```

### 1.4 Create API Endpoints

| Method | Endpoint                     | Description                |
| ------ | ---------------------------- | -------------------------- |
| GET    | `/api/admin/promotions`      | Get all promotions (Admin) |
| GET    | `/api/admin/promotions/{id}` | Get promotion by ID        |
| POST   | `/api/admin/promotions`      | Create new promotion       |
| PUT    | `/api/admin/promotions/{id}` | Update promotion           |
| DELETE | `/api/admin/promotions/{id}` | Deactivate promotion       |

### 1.5 Update IPromotionRepository

```csharp
Task<List<Promotion>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
```

---

## Implementation Priority

### Phase 1: Basic CRUD (High Priority)

1. Create PromotionCommand + Handler
2. Update PromotionCommand + Handler
3. Delete PromotionCommand + Handler
4. Admin API Endpoints

### Phase 2: Advanced Targeting (Medium Priority)

1. Add SpecificMovieId, SpecificCinemaId to Domain
2. Update Evaluate() to check constraints
3. Update Create/Update commands

### Phase 3: Per-User Limits (Lower Priority)

1. Add MaxUsagePerUser
2. Track user usage in Redis or DB

---

## Files to Create/Modify

| File                                                                                 | Action                             |
| ------------------------------------------------------------------------------------ | ---------------------------------- |
| `Domain/Entities/PromotionAggregate/Promotion.cs`                                    | Add new fields & update Evaluate() |
| `Shared/Models/DataModels/PromotionDtos/PromotionUpsertRequest.cs`                   | Create new DTO                     |
| `Application/Features/Promotions/Commands/CreatePromotion/CreatePromotionHandler.cs` | Implement                          |
| `Application/Features/Promotions/Commands/UpdatePromotion/`                          | Create new                         |
| `Application/Features/Promotions/Commands/DeletePromotion/`                          | Create new                         |
| `Api/Controllers/AdminPromotionsController.cs`                                       | Create new controller              |
| `Application/Common/Interfaces/Persistence/IPromotionRepository.cs`                  | Add GetAllAsync                    |

---

## Example API Payloads

### Create Promotion

```json
POST /api/admin/promotions
{
    "code": "SUMMER50",
    "name": "Summer Sale 50%",
    "description": "Get 50% off your booking",
    "type": "Percentage",
    "value": 50,
    "maxDiscountAmount": 50000,
    "minOrderValue": 100000,
    "maxUsageCount": 1000,
    "maxUsagePerUser": 3,
    "startDate": "2025-06-01",
    "endDate": "2025-08-31",
    "specificMovieId": null,
    "specificCinemaId": null
}
```

### Create Movie-Specific Promotion

```json
POST /api/admin/promotions
{
    "code": "AVENGERS20",
    "name": "Avengers Early Bird",
    "type": "Percentage",
    "value": 20,
    "maxUsageCount": 500,
    "startDate": "2025-05-01",
    "endDate": "2025-05-31",
    "specificMovieId": "uuid-of-avengers-movie",
    "specificCinemaId": null
}
```

---

## Testing Scenarios

| Scenario                                  | Expected                                              |
| ----------------------------------------- | ----------------------------------------------------- |
| Create promotion with valid data          | ✅ Success                                            |
| Create promotion with duplicate code      | ❌ Error - "Code already exists"                      |
| Create promotion with endDate < startDate | ❌ Error - "End date must be after start date"        |
| Update promotion                          | ✅ Success                                            |
| Delete (deactivate) promotion             | ✅ Success - IsActive = false                         |
| Apply promotion to non-targeted movie     | ❌ Error - "Promotion not valid for this movie"       |
| Apply promotion exceeding MaxUsageCount   | ❌ Error - "Promotion usage limit reached"            |
| Apply promotion exceeding MaxUsagePerUser | ❌ Error - "You have reached the maximum usage limit" |
