using Booking.API.Application.DTOs;
using Booking.API.Domain.Entities;
using BookingEmailSeatDto = Booking.API.Application.DTOs.BookingSeatDto;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Api.Endpoints;

public static class EmailTestEndpoints
{
    public static void MapEmailTestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/email-test")
            .WithTags("Email Test")
            .WithOpenApi();

        // Test basic email sending
        group.MapPost("/send-basic", SendBasicTestEmail)
            .WithName("SendBasicTestEmail")
            .WithSummary("Send a basic test email")
            .WithDescription("Send a simple test email to verify SMTP configuration");

        // Test booking confirmation email with mock data
        group.MapPost("/send-booking-confirmation", SendBookingConfirmationTest)
            .WithName("SendBookingConfirmationTest")
            .WithSummary("Send booking confirmation test email")
            .WithDescription("Send a booking confirmation email with mock booking data");

        // Test all email types
        group.MapPost("/send-all-types", SendAllEmailTypes)
            .WithName("SendAllEmailTypes")
            .WithSummary("Send all email types for testing")
            .WithDescription("Send booking confirmation, cancellation, and reminder emails");
    }

    private static async Task<IResult> SendBasicTestEmail(
        [FromServices] IEmailService emailService,
        [FromQuery] string? email = null)
    {
        try
        {
            var targetEmail = email ?? "tester@example.com";

            var htmlBody = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Test Email</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #2c3e50; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .success { color: #27ae60; font-weight: bold; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎬 Cinema System Test Email</h1>
        </div>
        <div class='content'>
            <p class='success'>SMTP Configuration is working!</p>
            <p>This is a test email from Cinema Booking System.</p>
            <p><strong>Timestamp:</strong> " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"</p>
            <p><strong>Environment:</strong> " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") + @"</p>
            <p>If you received this email, the email service is configured correctly.</p>
        </div>
    </div>
</body>
</html>";

            var plainTextBody = $@"
CINEMA SYSTEM TEST EMAIL

SMTP Configuration is working!

This is a test email from Cinema Booking System.
Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}

If you received this email, the email service is configured correctly.
";

            var success = await emailService.SendEmailAsync(
                targetEmail,
                "🎬 Cinema System - SMTP Test Email",
                htmlBody,
                plainTextBody);

            if (success)
            {
                return Results.Ok(new
                {
                    success = true,
                    message = $"Test email sent successfully to {targetEmail}",
                    timestamp = DateTime.Now
                });
            }
            else
            {
                return Results.BadRequest(new
                {
                    success = false,
                    message = "Failed to send test email",
                    timestamp = DateTime.Now
                });
            }
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                title: "Email sending failed",
                statusCode: 500);
        }
    }

    private static async Task<IResult> SendBookingConfirmationTest(
        [FromServices] IEmailService emailService,
        [FromQuery] string? email = null)
    {
        try
        {
            var targetEmail = email ?? "tester@example.com";

            // Create mock booking data
            var mockBooking = CreateMockBooking(targetEmail);

            var success = await emailService.SendBookingConfirmationAsync(mockBooking);

            if (success)
            {
                return Results.Ok(new
                {
                    success = true,
                    message = $"Booking confirmation test email sent to {targetEmail}",
                    bookingCode = mockBooking.BookingCode,
                    timestamp = DateTime.Now
                });
            }
            else
            {
                return Results.BadRequest(new
                {
                    success = false,
                    message = "Failed to send booking confirmation email",
                    timestamp = DateTime.Now
                });
            }
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                title: "Booking confirmation email failed",
                statusCode: 500);
        }
    }

    private static async Task<IResult> SendAllEmailTypes(
        [FromServices] IEmailService emailService,
        [FromQuery] string? email = null)
    {
        try
        {
            var targetEmail = email ?? "tester@example.com";
            var results = new List<object>();

            // Create mock booking
            var mockBooking = CreateMockBooking(targetEmail);

            // 1. Send confirmation email
            var confirmationSuccess = await emailService.SendBookingConfirmationAsync(mockBooking);
            results.Add(new
            {
                type = "confirmation",
                success = confirmationSuccess,
                message = confirmationSuccess ? "Sent successfully" : "Failed to send"
            });

            // Wait a bit between emails
            await Task.Delay(1000);

            // 2. Send cancellation email
            var cancellationSuccess = await emailService.SendBookingCancellationAsync(
                mockBooking, "This is a test cancellation - your booking is still valid!");
            results.Add(new
            {
                type = "cancellation",
                success = cancellationSuccess,
                message = cancellationSuccess ? "Sent successfully" : "Failed to send"
            });

            // Wait a bit between emails
            await Task.Delay(1000);

            // 3. Send reminder email
            var reminderSuccess = await emailService.SendBookingReminderAsync(mockBooking);
            results.Add(new
            {
                type = "reminder",
                success = reminderSuccess,
                message = reminderSuccess ? "Sent successfully" : "Failed to send"
            });

            var allSuccess = results.All(r => (bool)r.GetType().GetProperty("success")!.GetValue(r)!);

            return Results.Ok(new
            {
                success = allSuccess,
                message = $"Sent {results.Count} test emails to {targetEmail}",
                results = results,
                bookingCode = mockBooking.BookingCode,
                timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                title: "Email sending failed",
                statusCode: 500);
        }
    }

    private static BookingEmailDto CreateMockBooking(string email)
    {
        var showtime = DateTime.Now.AddDays(1).Date.AddHours(19).AddMinutes(30); // Tomorrow 7:30 PM

        var booking = new BookingEmailDto
        {
            Id = Guid.NewGuid(),
            BookingCode = $"TEST{DateTime.Now:yyyyMMddHHmmss}",
            CustomerName = "Test Customer",
            CustomerEmail = email,
            CustomerPhone = "0000000000",
            MovieTitle = "Avengers: Endgame",
            MoviePoster = "https://example.com/poster.jpg",
            CinemaName = "CGV Vincom Center",
            CinemaAddress = "191 Ba Trieu, Hai Ba Trung, Ha Noi",
            CinemaHallName = "Hall 1",
            ShowtimeDate = showtime,
            TotalAmount = 150000,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now,
            BookingSeats = new List<BookingEmailSeatDto>
            {
                new BookingEmailSeatDto
                {
                    Id = Guid.NewGuid(),
                    SeatNumber = "A1",
                    SeatPrice = 75000
                },
                new BookingEmailSeatDto
                {
                    Id = Guid.NewGuid(),
                    SeatNumber = "A2",
                    SeatPrice = 75000
                }
            }
        };

        return booking;
    }
}

