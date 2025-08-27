using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.IdentityModels
{
    public class ChangePasswordRequest
    {
        public required string Email { get; set; }
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between {2} and {1} characters.")]
        [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
        ErrorMessage = "Password must contain at least one lowercase letter, uppercase letter, digit, and non-alphanumeric character.")]
        public required string OldPassword { get; set; }
        [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
        ErrorMessage = "Password must contain at least one lowercase letter, uppercase letter, digit, and non-alphanumeric character.")]
        public required string NewPassword { get; set; }

    }
}
