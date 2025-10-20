using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.InventoryDtos
{
    public class EmployeeCreateRequest
    {
        public Guid CinemaId { get; set; }
        public required string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public required DateTime HireDate { get; set; }
        public required decimal Salary { get; set; }
        public required string Password { get; set; } = string.Empty;
        public required List<string> Roles { get; set; } = new List<string>();
    }
}
