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
        public const string BOOKING_CONFIRMATION_SUBJECT = "✅ Booking Confirmed Successfully";

        public static string BookingConfirmation(EmailConfirmBookingResponse bookingInfo)
        {
            byte[] qrCodeBytes = QrCodeHelper.GenerateQrCodeBytes(bookingInfo.BookingCode.ToString());
            string qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);
            return $@"
        <div style='font-family: Arial, sans-serif; max-width: 700px; margin: 0 auto; background: #fff;'>
            <!-- Header -->
            <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px 20px; text-align: center;'>
                <h1 style='color: #fff; margin: 0; font-size: 28px;'>🎬 Booking Successful!</h1>
                <p style='color: #fff; margin: 10px 0 0 0; font-size: 16px;'>Thank you for trusting and using our services</p>
            </div>

            <!-- Booking Information -->
            <div style='padding: 30px 20px;'>
                <h2 style='color: #333; margin-bottom: 25px; border-bottom: 2px solid #667eea; padding-bottom: 10px;'>
                    📋 Booking Information
                </h2>
                
                <div style='background: #f8f9ff; border-radius: 10px; padding: 25px; margin-bottom: 25px;'>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555; width: 40%;'>
                                🎭 Movie Title:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #333; font-weight: 600; font-size: 16px;'>
                                {bookingInfo.MovieTitle}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555;'>
                                📅 Show Date:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #333;'>
                                {bookingInfo.Showtime.ToString("dd/MM/yyyy")}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555;'>
                                ⏰ Show Time:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #333; font-weight: 600;'>
                                {bookingInfo.TimeSlot}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555;'>
                                🏢 Cinema:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #333;'>
                                {bookingInfo.CinemaName}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555;'>
                                📺 Screen:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #333;'>
                                {bookingInfo.ScreenName}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555;'>
                                🎫 Total Tickets:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #333; font-weight: 600;'>
                                {bookingInfo.TotalTickets} ticket(s)
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555;'>
                                💺 Seats:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #333; font-weight: 600;'>
                                {string.Join(", ", bookingInfo.SeatsList ?? new List<string>())}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; font-weight: bold; color: #555; width: 40%;'>
                                🔢 Booking Code:
                            </td>
                            <td style='padding: 12px 0; border-bottom: 1px solid #e0e6ff; color: #667eea; font-weight: bold; font-size: 16px; font-family: monospace;'>
                                {bookingInfo.BookingCode}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 15px 0 0 0; font-weight: bold; color: #667eea; font-size: 16px;'>
                                💰 Total Amount:
                            </td>
                            <td style='padding: 15px 0 0 0; color: #667eea; font-weight: bold; font-size: 20px;'>
                                {bookingInfo.TotalAmount:N0} VND
                            </td>
                        </tr>
                    </table>
                </div>
                <!-- QR Code Section -->
                <div style='background: #fff; border: 2px solid #667eea; border-radius: 10px; padding: 25px; margin-bottom: 25px; text-align: center;'>
                    <h3 style='color: #667eea; margin: 0 0 15px 0; font-size: 18px;'>📱 QR Code for Tickets</h3>
                    <p style='color: #666; margin: 0 0 20px 0; font-size: 14px;'>Scan this QR code at the counter to quickly receive your tickets</p>
                    <div style='display: inline-block; padding: 15px; background: #fff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                        <img src='cid:qrcode' alt='QR Code' style='width: 200px; height: 200px; display: block;' />
                    </div>
                    <p style='color: #667eea; margin: 15px 0 0 0; font-size: 14px; font-weight: 600;'>
                        Booking Code: {bookingInfo.BookingCode}
                    </p>
                </div>
                <!-- Important Notes -->
                <div style='background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 8px; padding: 20px; margin-bottom: 25px;'>
                    <h3 style='color: #856404; margin: 0 0 15px 0; font-size: 18px;'>⚠️ Important Notes:</h3>
                    <ul style='color: #856404; margin: 0; padding-left: 20px; line-height: 1.6;'>
                        <li>Please arrive at the cinema at least <strong>15 minutes</strong> before showtime</li>
                        <li>Bring this email or booking code to receive your tickets at the counter</li>
                        <li>No refunds are allowed after successful booking</li>
                        <li>Contact our hotline for support: <strong>1900-xxxx</strong></li>
                    </ul>
                </div>


                <!-- Footer -->
                <div style='text-align: center; padding-top: 20px; border-top: 1px solid #e0e6ff;'>
                    <p style='color: #888; font-size: 14px; margin: 0 0 10px 0;'>
                        Wishing you a wonderful entertainment experience! 🍿🎬
                    </p>
                    <p style='color: #666; font-size: 12px; margin: 0;'>
                        This is an automated email. Please do not reply.
                        <br>
                        If you have any questions, please contact our customer support team.
                    </p>
                </div>
            </div>
        </div>
    ";
        }
    }
}
