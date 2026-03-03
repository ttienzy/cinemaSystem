# Exception Handling Analysis & Improvement Plan

## Current Exception Status

### ✅ Exceptions Currently Handled (Map to Proper HTTP Status Codes)

| Exception Type          | HTTP Status | Description             | File Location                                            |
| ----------------------- | ----------- | ----------------------- | -------------------------------------------------------- |
| `NotFoundException`     | 404         | Resource not found      | `Application/Common/Exceptions/NotFoundException.cs`     |
| `ConflictException`     | 409         | Business conflict       | `Application/Common/Exceptions/ConflictException.cs`     |
| `ConcurrencyException`  | 409         | DB concurrency conflict | `Application/Common/Exceptions/ConcurrencyException.cs`  |
| `ForbiddenException`    | 403         | Access denied           | `Application/Common/Exceptions/ForbiddenException.cs`    |
| `UnauthorizedException` | -           | Unauthorized access     | `Application/Common/Exceptions/UnauthorizedException.cs` |
| `ValidationException`   | 422         | Validation failed       | `Application/Common/Exceptions/ValidationException.cs`   |
| `DomainException`       | 400         | Business rule violation | `Domain/Common/DomainException.cs`                       |

### ⚠️ Missing Exception Handlers (Cause 500 Errors)

| Exception Type              | Source                   | Recommended HTTP Status | Action Required               |
| --------------------------- | ------------------------ | ----------------------- | ----------------------------- |
| `SecurityTokenException`    | JWT/Token validation     | 401 Unauthorized        | Add to DomainExceptionHandler |
| `ArgumentNullException`     | Null arguments           | 400 Bad Request         | Add handler                   |
| `ArgumentException`         | Invalid arguments        | 400 Bad Request         | Add handler                   |
| `InvalidOperationException` | Invalid state operations | 400 Bad Request         | Add handler                   |
| `KeyNotFoundException`      | Dictionary/key lookup    | 404 Not Found           | Add handler                   |
| `FormatException`           | Invalid format input     | 400 Bad Request         | Add handler                   |
| `JsonException`             | JSON parsing errors      | 400 Bad Request         | Add handler                   |
| `TimeoutException`          | Operation timeouts       | 504 Gateway Timeout     | Add handler                   |
| `HttpRequestException`      | External API calls       | 502 Bad Gateway         | Add handler                   |

---

## Vấn đề bạn gặp phải

Khi test với token invalid hoặc expired, hệ thống ném `SecurityTokenException` từ:

- [`Infrastructure/Identity/Security/TokenClaimService.cs:98`](Infrastructure/Identity/Security/TokenClaimService.cs)

```csharp
throw new SecurityTokenException("Invalid token");
```

Exception này **KHÔNG** được handler trong [`DomainExceptionHandler.cs`](Api/Middleware/DomainExceptionHandler.cs), nên nó sẽ rơi vào `GlobalExceptionHandler` và trả về **500 Internal Server Error** thay vì **401 Unauthorized**.

---

## Recommended Fix

### Option 1: Update DomainExceptionHandler (Recommended)

Thêm các exception types vào [`DomainExceptionHandler.cs`](Api/Middleware/DomainExceptionHandler.cs:17-25):

```csharp
var (statusCode, title, type) = exception switch
{
    NotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found", "..."),
    ConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency Conflict", "..."),
    ConflictException => (StatusCodes.Status409Conflict, "Conflict Occurred", "..."),
    ForbiddenException => (StatusCodes.Status403Forbidden, "Access Forbidden", "..."),
    DomainException => (StatusCodes.Status400BadRequest, "Business Rule Violation", "..."),

    // ADD THESE:
    SecurityTokenException => (StatusCodes.Status401Unauthorized, "Invalid Token", "https://tools.ietf.org/html/rfc7235#section-3.1"),
    UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", "https://tools.ietf.org/html/rfc7235#section-3.1"),
    ArgumentNullException => (StatusCodes.Status400BadRequest, "Invalid Argument", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
    ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Argument", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
    InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid Operation", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
    KeyNotFoundException => (StatusCodes.Status404NotFound, "Key Not Found", "https://tools.ietf.org/html/rfc7231#section-6.5.4"),

    _ => (0, string.Empty, string.Empty)
};
```

### Option 2: Create Dedicated AuthExceptionHandler

Tạo handler riêng cho authentication exceptions:

```csharp
public class AuthExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (statusCode, title) = exception switch
        {
            SecurityTokenException => (StatusCodes.Status401Unauthorized, "Invalid Token"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ => (0, string.Empty)
        };

        if (statusCode == 0) return false;

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        });

        return true;
    }
}
```

---

## Test Cases for Exception Handling

### Authentication Errors

| Scenario              | Expected Status | Current Status |
| --------------------- | --------------- | -------------- |
| Invalid JWT token     | 401             | 500 ❌         |
| Expired JWT token     | 401             | 500 ❌         |
| Missing token         | 401             | 401 ✅         |
| Invalid refresh token | 401             | 401 ✅         |

### Validation Errors

| Scenario               | Expected Status | Current Status |
| ---------------------- | --------------- | -------------- |
| Missing required field | 422             | 422 ✅         |
| Invalid email format   | 422             | 422 ✅         |
| Invalid enum value     | 422             | 422 ✅         |

### Business Logic Errors

| Scenario                | Expected Status | Current Status |
| ----------------------- | --------------- | -------------- |
| Resource not found      | 404             | 404 ✅         |
| Duplicate resource      | 409             | 409 ✅         |
| Concurrent modification | 409             | 409 ✅         |
| Invalid booking state   | 400             | 400 ✅         |
| Seat already booked     | 409             | 409 ✅         |

---

## Summary

**Current State:**

- ✅ 7 exception types handled properly
- ⚠️ ~10 exception types NOT handled → 500 errors

**Recommended Priority:**

1. **High**: Add `SecurityTokenException` → fixes JWT testing
2. **High**: Add `ArgumentNullException`, `ArgumentException` → common input errors
3. **Medium**: Add `InvalidOperationException` → state machine errors
4. **Low**: Add remaining infrastructure exceptions
