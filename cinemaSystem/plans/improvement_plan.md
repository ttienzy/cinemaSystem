# Cinema System - Improvement Plan

## Task 1: Fix Couple Seat Linking (Bidirectional Validation + Unlink)

### Current Issue

API [`LinkSeat`](Api/Controllers/CinemasController.cs:26-32) chỉ link 1 chiều:

- Gọi `seat.LinkWithSeat(partnerSeatNumber)`
- **KHÔNG** kiểm tra partner seat có cùng SeatTypeId (ví dụ: ghế couple)
- **KHÔNG** tự động link ngược lại
- **KHÔNG** có API để unlink/hủy link

### User Requirements

1. Khi link phải kiểm tra SeatTypeId của cả 2 ghế - phải giống nhau
2. Validate adjacency (phải là ghế kế nhau: A1-A2, không phải A1-A3)
3. Thêm API UnlinkCouple để hủy link

### Required Changes

#### 1.1 Update Domain - Seat.cs

Add method to check if seat can be couple-linked:

```csharp
// In Seat.cs - Add validation method
public bool CanLinkAsCouple(int partnerSeatNumber)
{
    // Check if partner is adjacent (difference = 1)
    return Math.Abs(partnerSeatNumber - Number) == 1;
}
```

#### 1.2 Update LinkSeatHandler

Enhanced logic:

```csharp
// Step 1: Get seat by seatId
var seat = screen.Seats.FirstOrDefault(s => s.Id == request.SeatId);

// Step 2: Get partner seat by number
var partnerSeat = screen.Seats.FirstOrDefault(s => s.Number == request.PartnerSeatNumber);

// Step 3: Validate partner exists
if (partnerSeat == null)
    throw new NotFoundException("Partner seat not found");

// Step 4: CRITICAL - Validate SeatTypeId match (both must be same seat type)
if (seat.SeatTypeId != partnerSeat.SeatTypeId)
    throw new DomainException(
        $"Cannot link seats with different types. Seat {seat.SeatLabel} is type '{seat.SeatTypeId}' but seat {partnerSeat.SeatLabel} is type '{partnerSeat.SeatTypeId}'.");

// Step 5: Validate adjacency (must be adjacent seats)
if (!seat.CanLinkAsCouple(request.PartnerSeatNumber))
    throw new DomainException("Couple seats must be adjacent (consecutive numbers).");

// Step 6: Check if either seat is already linked
if (seat.LinkedSeatNumber.HasValue)
    throw new DomainException($"Seat {seat.SeatLabel} is already linked to seat {seat.LinkedSeatNumber}.");
if (partnerSeat.LinkedSeatNumber.HasValue)
    throw new DomainException($"Seat {partnerSeat.SeatLabel} is already linked to seat {partnerSeat.LinkedSeatNumber}.");

// Step 7: Bidirectional link
seat.LinkWithSeat(request.PartnerSeatNumber);
partnerSeat.LinkWithSeat(seat.Number);
```

#### 1.3 Create UnlinkCoupleSeat API

Add new endpoint to cancel/unlink couple seats:

```csharp
// In CinemasController
[HttpPost("screens/{screenId}/seats/{seatId}/unlink")]
public async Task<IActionResult> UnlinkSeat(Guid screenId, Guid seatId)
{
    await Mediator.Send(new UnlinkSeatCommand(screenId, seatId));
    return NoContent();
}
```

Handler:

```csharp
public class UnlinkSeatHandler : IRequestHandler<UnlinkSeatCommand, Unit>
{
    public async Task<Unit> Handle(UnlinkSeatCommand request, CancellationToken ct)
    {
        var screen = await cinemaRepo.GetScreenWithSeatsAsync(request.ScreenId, ct);
        var seat = screen.Seats.FirstOrDefault(s => s.Id == request.SeatId)
            ?? throw new NotFoundException("Seat", request.SeatId);

        if (!seat.LinkedSeatNumber.HasValue)
            throw new DomainException("Seat is not linked to any partner.");

        // Find and unlink partner
        var partnerSeat = screen.Seats.FirstOrDefault(s => s.Number == seat.LinkedSeatNumber);
        if (partnerSeat != null)
        {
            partnerSeat.UnlinkCoupleSeat();
        }

        seat.UnlinkCoupleSeat();
        await uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
```

#### 1.4 Validation Scenarios

| Scenario                                            | Expected                                            |
| --------------------------------------------------- | --------------------------------------------------- |
| Link A1 with A2 (adjacent, same SeatTypeId)         | ✅ Success - bidirectional link                     |
| Link A1 with A3 (not adjacent)                      | ❌ Error - "Couple seats must be adjacent"          |
| Link A2 with A3 (adjacent but different SeatTypeId) | ❌ Error - "Cannot link seats with different types" |
| Link A1 (already linked to A2) with A4              | ❌ Error - "Seat A1 is already linked to seat 2"    |
| Link non-existent seat                              | ❌ Error - 404 Not Found                            |

### Files to Modify

| File                                                                | Action                                    |
| ------------------------------------------------------------------- | ----------------------------------------- |
| `Domain/Entities/CinemaAggregate/Seat.cs`                           | Add `CanLinkAsCouple()` method            |
| `Application/Features/Cinemas/Commands/LinkSeat/LinkSeatHandler.cs` | Enhanced validation with SeatTypeId check |
| `Api/Controllers/CinemasController.cs`                              | Add Unlink endpoint                       |
| New: `Application/Features/Cinemas/Commands/UnlinkSeat/`            | New command + handler                     |

---

## Task 2: Create Movie with Full Details (Single API Call)

### Current Issue

API [`CreateMovie`](Api/Controllers/AdminMoviesController.cs:13-17) hiện tại chỉ tạo Movie cơ bản:

- Chỉ nhận `MovieUpsertRequest` với basic fields
- **KHÔNG** bao gồm: CastCrew, Certifications, Copyrights, Genres
- Phải gọi nhiều API riêng biệt để thêm từng loại

### Required Changes

#### 2.1 Update MovieUpsertRequest (DTO)

Add new fields:

```csharp
public class MovieUpsertRequest
{
    // Existing fields
    public required string Title { get; set; }
    public required int DurationMinutes { get; set; }
    public required DateTime ReleaseDate { get; set; }
    public required string PosterUrl { get; set; }
    public string? Description { get; set; }
    public required RatingStatus RatingStatus { get; set; }
    public required string Trailer { get; set; }
    public required MovieStatus Status { get; set; }

    // NEW: Related data
    public List<CastCrewItem>? CastCrews { get; set; }
    public List<CertificationItem>? Certifications { get; set; }
    public List<CopyrightItem>? Copyrights { get; set; }
    public List<Guid>? GenreIds { get; set; }
}

public class CastCrewItem
{
    public required string PersonName { get; set; }
    public required string RoleType { get; set; } // actor, director, producer
}

public class CertificationItem
{
    public required string CertificationBody { get; set; }
    public required string Rating { get; set; } // P, T13, T16, T18
    public required DateTime IssueDate { get; set; }
}

public class CopyrightItem
{
    public required string DistributorCompany { get; set; }
    public required DateTime LicenseStartDate { get; set; }
    public required DateTime LicenseEndDate { get; set; }
    public required MovieCopyrightStatus Status { get; set; }
}
```

#### 2.2 Update CreateMovieHandler

Process all related data in single transaction:

```csharp
public async Task<Guid> Handle(CreateMovieCommand request, CancellationToken ct)
{
    // 1. Create Movie
    var movie = new Movie(...);
    await movieRepo.AddAsync(movie, ct);

    // 2. Add Genres (if provided)
    if (request.Request.GenreIds?.Any() == true)
    {
        var genres = request.Request.GenreIds.Select(g => new MovieGenre(g, movie.Id)).ToList();
        movie.AddRangeGenres(genres);
    }

    // 3. Add CastCrew (if provided)
    if (request.Request.CastCrews?.Any() == true)
    {
        var castCrews = request.Request.CastCrews
            .Select(c => new MovieCastCrew(movie.Id, c.PersonName, c.RoleType))
            .ToList();
        movie.AddRangeCastCrew(castCrews);
    }

    // 4. Add Certifications (if provided)
    if (request.Request.Certifications?.Any() == true)
    {
        var certs = request.Request.Certifications
            .Select(c => new MovieCertification(movie.Id, c.CertificationBody, c.Rating, c.IssueDate))
            .ToList();
        movie.AddRangeCertifications(certs);
    }

    // 5. Add Copyrights (if provided)
    if (request.Request.Copyrights?.Any() == true)
    {
        var copyrights = request.Request.Copyrights
            .Select(c => new MovieCopyright(movie.Id, c.DistributorCompany, c.LicenseStartDate, c.LicenseEndDate, c.Status))
            .ToList();
        movie.AddRangeCopyrights(copyrights);
    }

    await unitOfWork.SaveChangesAsync(ct);
    return movie.Id;
}
```

#### 2.3 Validation Rules

- GenreIds: Must be valid Genre IDs (check against Genre table)
- CastCrew: PersonName and RoleType required
- Certification: CertificationBody, Rating, IssueDate required
- Copyright: DistributorCompany, LicenseStartDate, LicenseEndDate, Status required
- LicenseEndDate > LicenseStartDate

### Files to Create/Modify

| File                                                                     | Action                             |
| ------------------------------------------------------------------------ | ---------------------------------- |
| `Shared/Models/DataModels/MovieDtos/MovieUpsertRequest.cs`               | Add new DTO fields                 |
| `Application/Features/Movies/Commands/CreateMovie/CreateMovieHandler.cs` | Update to process all related data |

---

## Summary

| Task | Description                                                                        | Priority |
| ---- | ---------------------------------------------------------------------------------- | -------- |
| 1    | Fix Couple Seat Linking - bidirectional validation + SeatTypeId check + Unlink API | High     |
| 2    | Create Movie with Full Details - single API                                        | Medium   |

### Testing Scenarios

#### Task 1 - Couple Seat

| Scenario                                            | Expected                                            |
| --------------------------------------------------- | --------------------------------------------------- |
| Link A1 with A2 (adjacent, same SeatTypeId)         | ✅ Success - bidirectional link                     |
| Link A1 with A3 (not adjacent)                      | ❌ Error - "Couple seats must be adjacent"          |
| Link A2 with A3 (adjacent but different SeatTypeId) | ❌ Error - "Cannot link seats with different types" |
| Link A1 (already linked to A2) with A4              | ❌ Error - "Seat A1 is already linked to seat 2"    |
| Link non-existent seat                              | ❌ Error - 404 Not Found                            |
| Unlink A1 (linked to A2)                            | ✅ Success - both seats unlinked                    |

#### Task 2 - Movie Creation

| Scenario                                     | Expected                                                  |
| -------------------------------------------- | --------------------------------------------------------- |
| Create movie with all fields                 | Success - Movie + Cast + Cert + Copyright + Genre created |
| Create movie with only basic fields          | Success - Movie created only                              |
| Create movie with invalid GenreId            | Error - "Genre not found"                                 |
| Create movie with LicenseEndDate < StartDate | Error - "End date must be after start date"               |
