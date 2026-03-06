using Shared.Models.ExtenalModels;
using System;

namespace Shared.Templates
{
    public static class StaffTemplates
    {
        public static EmailRequest WelcomeStaff(string email, string fullName, string temporaryPassword, string role)
        {
            return new EmailRequest
            {
                ToEmail = email,
                Subject = "Welcome to Cinema System - Staff Account Details",
                Body = $@"
                    <h2>Welcome {fullName},</h2>
                    <p>Your staff account has been successfully created in the Cinema System.</p>
                    <p>Below are your login credentials:</p>
                    <ul>
                        <li><strong>Email:</strong> {email}</li>
                        <li><strong>Role:</strong> {role}</li>
                        <li><strong>Temporary Password:</strong> <span style='color: blue; font-weight: bold;'>{temporaryPassword}</span></li>
                    </ul>
                    <p>Please log in and change your password immediately to ensure security.</p>
                    <p>Best regards,<br/>Cinema System Administration Team</p>"
            };
        }
    }
}
