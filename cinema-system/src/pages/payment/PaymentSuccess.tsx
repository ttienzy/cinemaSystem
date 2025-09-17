import React, { useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { CheckCircle, Calendar, MapPin, Clock } from 'lucide-react';

const PaymentSuccess: React.FC = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const showtimeId = searchParams.get('showtimeId');

    useEffect(() => {
        // Nếu không có showtimeId, redirect về home
        if (!showtimeId) {
            navigate('/', { replace: true });
        }
    }, [showtimeId, navigate]);

    const handleViewBooking = () => {
        if (showtimeId) {
            navigate(`/seating-plan/${showtimeId}`);
        }
    };

    const handleBackToHome = () => {
        navigate('/');
    };

    return (
        <div className="min-h-screen bg-gradient-to-br from-green-50 to-blue-50 flex items-center justify-center p-4">
            <div className="max-w-md w-full bg-white rounded-2xl shadow-xl p-8 text-center">
                {/* Success Icon */}
                <div className="flex justify-center mb-6">
                    <div className="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center">
                        <CheckCircle className="w-12 h-12 text-green-600" />
                    </div>
                </div>

                {/* Success Message */}
                <h1 className="text-2xl font-bold text-gray-800 mb-2">
                    Payment Successful!
                </h1>
                <p className="text-gray-600 mb-8">
                    Your booking has been confirmed successfully. You will receive a confirmation email shortly.
                </p>

                {/* Booking Info */}
                {showtimeId && (
                    <div className="bg-gray-50 rounded-lg p-4 mb-6 text-left">
                        <h3 className="font-semibold text-gray-800 mb-3 flex items-center">
                            <Calendar className="w-4 h-4 mr-2" />
                            Booking Details
                        </h3>
                        <div className="space-y-2 text-sm text-gray-600">
                            <div className="flex items-center">
                                <Clock className="w-4 h-4 mr-2" />
                                <span>Showtime ID: {showtimeId}</span>
                            </div>
                            <div className="flex items-center">
                                <MapPin className="w-4 h-4 mr-2" />
                                <span>Confirmation email sent</span>
                            </div>
                        </div>
                    </div>
                )}

                {/* Action Buttons */}
                <div className="space-y-3">
                    <button
                        onClick={handleViewBooking}
                        className="w-full bg-green-600 hover:bg-green-700 text-white font-semibold py-3 px-6 rounded-lg transition duration-200 transform hover:scale-105"
                    >
                        View Your Booking
                    </button>

                    <button
                        onClick={handleBackToHome}
                        className="w-full bg-gray-100 hover:bg-gray-200 text-gray-700 font-semibold py-3 px-6 rounded-lg transition duration-200"
                    >
                        Back to Home
                    </button>
                </div>

                {/* Support Message */}
                <p className="text-xs text-gray-500 mt-6">
                    Need help? Contact our support team anytime.
                </p>
            </div>
        </div>
    );
};

export default PaymentSuccess;