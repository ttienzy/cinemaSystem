using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.IdentityModels.Otps
{
    public class ResetPasswordWithOtpRequest
    {

        public required string Email { get; set; }
        public required string OtpCode { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmNewPassword { get; set; }
    }
}
