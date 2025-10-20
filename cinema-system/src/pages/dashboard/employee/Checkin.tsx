// src/pages/dashboard/employee/Checkin.tsx
import React, { useState } from 'react';
import BookingCheckIn from '../../../components/concession/BookingCheckIn';
import QRCodeScanner from '../../../components/qrcode/QRCodeScanner';

// Định nghĩa kiểu cho các tab
type ActiveTab = 'qrScanner' | 'manualCheckin';

const EmployeeCheckin: React.FC = () => {
    const [activeTab, setActiveTab] = useState<ActiveTab>('qrScanner');

    return (
        <div className="min-h-screen bg-gray-50">
            <div className="mx-auto">

                {/* Main Card */}
                <div className="bg-white rounded-xl shadow-lg overflow-hidden">
                    {/* Tab Navigation */}
                    <div className="border-b border-gray-200">
                        <nav className="flex -mb-px">
                            <button
                                onClick={() => setActiveTab('qrScanner')}
                                className={`
                                    flex-1 py-4 px-6 text-center font-semibold transition-all duration-200
                                    ${activeTab === 'qrScanner'
                                        ? 'border-b-3 border-indigo-600 text-indigo-600 bg-indigo-50'
                                        : 'text-gray-600 hover:text-gray-900 hover:bg-gray-50'
                                    }
                                `}
                            >
                                <div className="flex items-center justify-center gap-2">
                                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v1m6 11h2m-6 0h-2v4m0-11v3m0 0h.01M12 12h4.01M16 20h4M4 12h4m12 0h.01M5 8h2a1 1 0 001-1V5a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1zm12 0h2a1 1 0 001-1V5a1 1 0 00-1-1h-2a1 1 0 00-1 1v2a1 1 0 001 1zM5 20h2a1 1 0 001-1v-2a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1z" />
                                    </svg>
                                    <span>Quét mã QR</span>
                                </div>
                            </button>

                            <button
                                onClick={() => setActiveTab('manualCheckin')}
                                className={`
                                    flex-1 py-4 px-6 text-center font-semibold transition-all duration-200
                                    ${activeTab === 'manualCheckin'
                                        ? 'border-b-3 border-indigo-600 text-indigo-600 bg-indigo-50'
                                        : 'text-gray-600 hover:text-gray-900 hover:bg-gray-50'
                                    }
                                `}
                            >
                                <div className="flex items-center justify-center gap-2">
                                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
                                    </svg>
                                    <span>Check-in thủ công</span>
                                </div>
                            </button>
                        </nav>
                    </div>

                    {/* Content Area */}
                    <div className="p-8">
                        {/* QR Scanner Tab */}
                        {activeTab === 'qrScanner' && (
                            <div className="animate-fadeIn">
                                <QRCodeScanner />
                            </div>
                        )}

                        {/* Manual Check-in Tab */}
                        {activeTab === 'manualCheckin' && (
                            <div className="animate-fadeIn">
                                <BookingCheckIn />
                            </div>
                        )}
                    </div>
                </div>

                {/* Helper Text */}
                <div className="mt-6 bg-blue-50 border border-blue-200 rounded-lg p-4">
                    <div className="flex items-start gap-3">
                        <svg className="w-5 h-5 text-blue-600 mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                            <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                        </svg>
                        <div>
                            <h3 className="text-sm font-semibold text-blue-900 mb-1">
                                Hướng dẫn sử dụng
                            </h3>
                            <ul className="text-sm text-blue-800 space-y-1">
                                <li>• <strong>Quét mã QR:</strong> Sử dụng camera để quét mã QR từ email khách hàng</li>
                                <li>• <strong>Check-in thủ công:</strong> Nhập mã đặt vé để tìm kiếm và xác nhận</li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>

            {/* CSS Animation */}
            <style>{`
                @keyframes fadeIn {
                    from {
                        opacity: 0;
                        transform: translateY(10px);
                    }
                    to {
                        opacity: 1;
                        transform: translateY(0);
                    }
                }
                
                .animate-fadeIn {
                    animation: fadeIn 0.3s ease-out;
                }
                
                .border-b-3 {
                    border-bottom-width: 3px;
                }
            `}</style>
        </div>
    );
};

export default EmployeeCheckin;