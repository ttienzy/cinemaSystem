using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.EmailTemplates
{
    public class PasswordResetTemplates
    {
        public static EmailRequest VerificationCode(string email, string otpCode)
        {
            return new EmailRequest
            {
                ToEmail = email,
                Subject = "Password Reset Verification Code",
                Body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #333;'>Password Reset Request</h2>
                        <p>Hello {email},</p>
                        <p>You requested to reset your password. Use the verification code below:</p>
                    
                        <div style='background: #f5f5f5; padding: 20px; text-align: center; margin: 20px 0;'>
                            <h1 style='color: #007bff; font-size: 32px; margin: 0; letter-spacing: 5px;'>{otpCode}</h1>
                        </div>
                    
                        <p><strong>This code will expire in 10 minutes.</strong></p>
                        <p style='color: #666; font-size: 14px;'>
                            If you didn't request this, please ignore this email or contact support if you're concerned.
                        </p>
                    </div>
                "
            };
        }
    }
}
