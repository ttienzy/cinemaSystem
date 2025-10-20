using Shared.Common.QRCode;
using Shared.Models.ExtenalModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Templates
{
    public class BookingConfirmationTemplate
    {   
        public const string BOOKING_CONFIRMATION_SUBJECT = "✅ Xác nhận đặt vé thành công";

        public static string BookingConfirmation(EmailConfirmBookingResponse bookingInfo)
        {
            byte[] qrCodeBytes = QrCodeHelper.GenerateQrCodeBytes(bookingInfo.BookingCode.ToString());
            string qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);
            return $@"
        <div style='font-family: Arial, sans-serif; max-width: 700px; margin: 0 auto; background: #fff;'>
            <!-- Header -->
            <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px 20px; text-align: center;'>
                <h1 style='color: #fff; margin: 0; font-size: 28px;'>🎬 Đặt vé thành công!</h1>
                <p style='color: #fff; margin: 10px 0 0 0; font-size: 16px;'>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi</p>
            </div>

            <!-- Thông tin đặt vé -->
            <div style='padding: 30px 20px;'>
                <h2 style='color: #333; margin-bottom: 25px; border-bottom: 2px solid #667eea; padding-bottom: 10px;'>
                    📋 Thông tin đặt vé
                </h2>
                
                <div style='background: #f8f9ff; border-radius: 10px; padding: 25px; margin-bottom: 25px;'>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555; width: 40%;'>
                                🎭 Tên phim:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #333; font-weight: 600; font-size: 16px;'>
                                {bookingInfo.MovieTitle}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555;'>
                                📅 Ngày chiếu:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #333;'>
                                {bookingInfo.Showtime.ToString("dd/MM/yyyy")}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555;'>
                                ⏰ Giờ chiếu:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #333; font-weight: 600;'>
                                {bookingInfo.TimeSlot}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555;'>
                                🏢 Rạp chiếu:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #333;'>
                                {bookingInfo.CinemaName}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555;'>
                                📺 Phòng chiếu:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #333;'>
                                {bookingInfo.ScreenName}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555;'>
                                🎫 Số lượng vé:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #333; font-weight: 600;'>
                                {bookingInfo.TotalTickets} vé
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555;'>
                                💺 Ghế ngồi:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #333; font-weight: 600;'>
                                {string.Join(", ", bookingInfo.SeatsList ?? new List<string>())}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555; width: 40%;'>
                                🔢 Mã đặt vé:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #667eea; font-weight: bold; font-size: 16px; font-family: monospace;'>
                                {bookingInfo.BookingCode}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 15px 0 0 0; font-weight: bold; color: #667eea; font-size: 16px;'>
                                💰 Tổng tiền:
                            </td>
                            <td style='padding: 15px 0 0 0; color: #667eea; font-weight: bold; font-size: 20px;'>
                                {bookingInfo.TotalAmount:N0} VNĐ
                            </td>
                        </tr>
                    </table>
                </div>
                <!-- QR Code Section -->
                <div style='background: #fff; border: 2px solid #667eea; border-radius: 10px; padding: 25px; margin-bottom: 25px; text-align: center;'>
                    <h3 style='color: #667eea; margin: 0 0 15px 0; font-size: 18px;'>📱 Mã QR để nhận vé</h3>
                    <p style='color: #666; margin: 0 0 20px 0; font-size: 14px;'>Quét mã QR này tại quầy để nhận vé nhanh chóng</p>
                    <div style='display: inline-block; padding: 15px; background: #fff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                        <img src='cid:qrcode' alt='QR Code' style='width: 200px; height: 200px; display: block;' />
                    </div>
                    <p style='color: #667eea; margin: 15px 0 0 0; font-size: 14px; font-weight: 600;'>
                        Mã đặt vé: {bookingInfo.BookingCode}
                    </p>
                </div>
                <!-- Lưu ý quan trọng -->
                <div style='background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 8px; padding: 20px; margin-bottom: 25px;'>
                    <h3 style='color: #856404; margin: 0 0 15px 0; font-size: 18px;'>⚠️ Lưu ý quan trọng:</h3>
                    <ul style='color: #856404; margin: 0; padding-left: 20px; line-height: 1.6;'>
                        <li>Vui lòng có mặt tại rạp chiếu trước giờ chiếu ít nhất <strong>15 phút</strong></li>
                        <li>Mang theo email này hoặc mã đặt vé để nhận vé tại quầy</li>
                        <li>Không được hoàn tiền sau khi đã đặt vé thành công</li>
                        <li>Liên hệ hotline nếu cần hỗ trợ: <strong>1900-xxxx</strong></li>
                    </ul>
                </div>


                <!-- Footer -->
                <div style='text-align: center; padding-top: 20px; border-top: 1px solid #e0e6ff;'>
                    <p style='color: #888; font-size: 14px; margin: 0 0 10px 0;'>
                        Chúc bạn có những phút giây giải trí tuyệt vời! 🍿🎬
                    </p>
                    <p style='color: #666; font-size: 12px; margin: 0;'>
                        Email này được gửi tự động. Vui lòng không trả lời email này.
                        <br>
                        Nếu bạn có thắc mắc, vui lòng liên hệ bộ phận chăm sóc khách hàng.
                    </p>
                </div>
            </div>
        </div>
    ";
        }
    }
}
