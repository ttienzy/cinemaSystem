using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.IdentityModels
{
    public class RegisterRequest
    {
        public required string Username { get; set; }
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Phone number must be 10 or 11 digits")]
        public required string PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between {2} and {1} characters.")]
        [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
        ErrorMessage = "Password must contain at least one lowercase letter, uppercase letter, digit, and non-alphanumeric character.")]
        public required string Password { get; set; }

    }
}
