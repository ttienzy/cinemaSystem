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
                Subject = "Chào mừng bạn đến với Cinema System - Thông tin tài khoản nhân viên",
                Body = $@"
                    <h2>Chào mừng {fullName},</h2>
                    <p>Tài khoản nhân viên của bạn đã được tạo thành công trên hệ thống Cinema System.</p>
                    <p>Dưới đây là thông tin đăng nhập của bạn:</p>
                    <ul>
                        <li><strong>Email:</strong> {email}</li>
                        <li><strong>Role:</strong> {role}</li>
                        <li><strong>Mật khẩu tạm thời:</strong> <span style='color: blue; font-weight: bold;'>{temporaryPassword}</span></li>
                    </ul>
                    <p>Vui lòng đăng nhập và đổi mật khẩu ngay lập tức để đảm bảo bảo mật.</p>
                    <p>Trân trọng,<br/>Đội ngũ quản trị Cinema System</p>"
            };
        }
    }
}
