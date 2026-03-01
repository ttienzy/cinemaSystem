using System;

namespace Shared.Models.DataModels.StaffDtos
{
    public class StaffAssignmentRequest
    {
        public Guid CinemaId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
    }
}
