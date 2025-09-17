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
        public DemoController(IEmailService emailService)
        {
            _emailService = emailService;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = new EmailRequest
            {
                ToEmail = "nguyendientien01062005@gmail.com",
                Subject = BookingConfirmationTemplate.BOOKING_CONFIRMATION_SUBJECT,
                Body = BookingConfirmationTemplate.BookingConfirmation(new EmailConfirmBookingResponse
                {
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
    }
}
