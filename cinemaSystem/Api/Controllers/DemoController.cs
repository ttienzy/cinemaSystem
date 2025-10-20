using Application.Interfaces.Integrations;
using Application.Interfaces.Persistences.Repo;
using Application.Specifications.MovieSpec;
using Domain.Entities.MovieAggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.MovieDtos;
using Shared.Models.ExtenalModels;
using Shared.Templates;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<DemoController> _logger;
        public DemoController(IEmailService emailService, ILogger<DemoController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("jfg");
            var data = new EmailRequest
            {

                ToEmail = "nguyendientien01062005@gmail.com",
                Subject = BookingConfirmationTemplate.BOOKING_CONFIRMATION_SUBJECT,
                Body = BookingConfirmationTemplate.BookingConfirmation(new EmailConfirmBookingResponse
                {
                    BookingCode = Guid.NewGuid(),
                    MovieTitle = "Avengers: Endgame",
                    Showtime = new DateTime(2025, 9, 15, 19, 30, 0), // 15/09/2025 19:30
                    TimeSlot = "19:30 - 22:00",
                    TotalTickets = 3,
                    TotalAmount = 450_000m, // 450,000 VNĐ
                    ScreenName = "Screen 5",
                    CinemaName = "CGV Vincom Center"

                })
            };
            await _emailService.SendEmailAsync(data);
            return Ok("Send email successful");
        }
        [HttpGet("booking-confirm")]
        public async Task<IActionResult> GetAsync()
        {
            await _emailService.SendBookingConfirmationEmailAsync(
                toEmail: "nguyendientien01062005@gmail.com",
                bookingInfo: new EmailConfirmBookingResponse
                {
                    BookingCode = Guid.NewGuid(),
                    MovieTitle = "Avengers: Endgame",
                    Showtime = DateTime.Now.AddDays(1),
                    TimeSlot = "19:30",
                    CinemaName = "CGV Vincom",
                    ScreenName = "Phòng 3",
                    TotalTickets = 2,
                    SeatsList = new List<string> { "A5", "A6" },
                    TotalAmount = 200000
                }
            );
            return Ok("Success");
        }
    }
}
