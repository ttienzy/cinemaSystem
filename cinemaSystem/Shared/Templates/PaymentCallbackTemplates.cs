using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Templates
{
    public class PaymentCallbackTemplates
    {
        public static string PaymentCanceled(Guid showtimeId)
        {
            return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='text-align: center; margin-bottom: 30px;'>
                    <h2 style='color: #dc3545; margin-bottom: 10px;'>Payment Canceled</h2>
                    <div style='width: 60px; height: 4px; background: #dc3545; margin: 0 auto;'></div>
                </div>
                
                <div style='background: #fff3cd; border: 1px solid #ffeaa7; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                    <p style='margin: 0; color: #856404;'>
                        <strong>Your payment has been canceled successfully.</strong>
                    </p>
                </div>
                
                <p style='color: #666; line-height: 1.6;'>
                    Don't worry, no charges have been made to your account. 
                    If you'd like to complete your booking, you can try again.
                </p>
                
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='http://localhost:5173/seating-plan/{showtimeId}' 
                       style='background: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                        Try Booking Again
                    </a>
                </div>
                
                <p style='color: #999; font-size: 12px; text-align: center; margin-top: 30px;'>
                    If you have any questions, please contact our support team.
                </p>
            </div>
        ";
        }
        public static string PaymentCompleted(Guid showtimeId)
        {
            return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='text-align: center; margin-bottom: 30px;'>
                    <h2 style='color: #28a745; margin-bottom: 10px;'>Payment Completed Successfully!</h2>
                    <div style='width: 60px; height: 4px; background: #28a745; margin: 0 auto;'></div>
                </div>
                
                <div style='background: #d4edda; border: 1px solid #c3e6cb; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                    <p style='margin: 0; color: #155724;'>
                        <strong>✓ Your payment has been processed successfully!</strong>
                    </p>
                </div>
                
                <p style='color: #666; line-height: 1.6;'>
                    Thank you for your purchase. Your booking has been confirmed and 
                    you should receive a confirmation email shortly with your ticket details.
                </p>
                
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='http://localhost:5173/seating-plan/{showtimeId}' 
                       style='background: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                        View Your Booking
                    </a>
                </div>
                
                <p style='color: #999; font-size: 12px; text-align: center; margin-top: 30px;'>
                    Need help? Contact our support team anytime.
                </p>
            </div>
        ";
        }
    }
}
