using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.IdentityModels.Otps
{
    public class PasswordResetOtp
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string Email { get; set; }
        public string OtpCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10); // 10 phút hết hạn
        public bool IsUsed { get; set; } = false;
        public int AttemptCount { get; set; } = 0;
    }
}
