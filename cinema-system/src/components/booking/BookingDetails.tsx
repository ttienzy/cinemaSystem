// src/components/booking/BookingDetails.tsx

import React from 'react';
import { CheckCircle, AlertCircle, XCircle, Ticket, Info, MapPin, Clock, Users, CreditCard } from 'lucide-react';
import type { BookingCheckedInResponse } from '../../types/booking.types';
import { formatCurrency, formatDateTime, getStatusInfo } from '../../utils/format'; // Giả sử bạn tạo file này ở bước 1.2

// =================================================================
// MAIN COMPONENT & HELPERS
// =================================================================

// This is the main component for displaying booking info.
export const BookingDetails: React.FC<{ data: BookingCheckedInResponse; onCheckIn: () => void; }> = ({ data, onCheckIn }) => {
    const statusInfo = getStatusInfo(data.status);

    return (
        <div className="border border-gray-200 rounded-xl shadow-sm animate-fade-in-down">
            {/* Header */}
            <div className="p-6 border-b border-gray-200">
                <div className="flex flex-col sm:flex-row justify-between sm:items-center gap-4">
                    <div>
                        <h2 className="text-2xl font-bold text-gray-900">{data.movieTitle}</h2>
                        <p className="text-gray-500">Mã vé: <span className="font-mono font-medium text-gray-700">{data.bookingCode}</span></p>
                    </div>
                    <div className={`inline-flex items-center gap-2 px-3 py-1.5 rounded-full text-sm font-semibold ${statusInfo.color}`}>
                        {statusInfo.text}
                    </div>
                </div>
            </div>

            {/* Details Grid */}
            <div className="p-6 grid md:grid-cols-2 gap-x-8 gap-y-6">
                <InfoItem icon={MapPin} label="Cinema" value={`${data.cinemaName} - ${data.screenName}`} />
                <InfoItem icon={Clock} label="Showtime" value={`${formatDateTime(data.actualStartTime)}`} />
                <InfoItem icon={Users} label="Seats" value={data.seatsList.join(', ')} />
                <InfoItem icon={Ticket} label="Total Tickets" value={`${data.totalTickets}`} />
                <InfoItem icon={CreditCard} label="Total Amount" value={formatCurrency(data.totalAmount)} isHighlight />
            </div>

            {/* Action Box */}
            <div className="p-6 bg-gray-50 rounded-b-xl border-t border-gray-200">
                {renderActionBoxContent(data, onCheckIn)}
            </div>
        </div>
    );
};

// A small component for displaying an item in the details grid.
export const InfoItem: React.FC<{ icon: React.ElementType, label: string, value: string, isHighlight?: boolean }> = ({ icon: Icon, label, value, isHighlight }) => (
    <div className="flex items-start gap-4">
        <Icon className="h-5 w-5 text-gray-400 mt-0.5 flex-shrink-0" />
        <div>
            <p className="text-sm text-gray-500">{label}</p>
            <p className={`font-semibold ${isHighlight ? 'text-xl text-green-600' : 'text-gray-800'}`}>{value}</p>
        </div>
    </div>
);

// This function contains the logic for what to display in the action box.
const renderActionBoxContent = (booking: BookingCheckedInResponse, onCheckIn: () => void) => {
    // ... (Giữ nguyên logic của hàm này)
    const now = new Date();
    const startTime = new Date(booking.actualStartTime);
    const endTime = new Date(booking.actualEndTime);

    if (booking.isCheckedIn) {
        return <ActionBox icon={CheckCircle} color="green" title="Successfully Checked-In" message="Enjoy the movie!" />;
    }
    if (booking.status === 'Cancelled') {
        return <ActionBox icon={XCircle} color="red" title="Booking Cancelled" message="This booking cannot be used." />;
    }
    if (booking.status !== 'Completed') {
        return <ActionBox icon={AlertCircle} color="yellow" title="Payment Not Completed" message="Please complete the payment to enable check-in." />;
    }
    if (now > endTime) {
        return <ActionBox icon={XCircle} color="red" title="Showtime Expired" message="This showtime has already ended." />;
    }
    if (now >= startTime && now <= endTime) {
        return <ActionBox icon={CheckCircle} color="green" title="Ready for Check-In" message="Please check-in to validate your ticket before entering." buttonText="Check-In Now" onAction={onCheckIn} />;
    }
    if (now < startTime) {
        return <ActionBox icon={Info} color="blue" title="Not Yet Time for Check-In" message={`Check-in will be available from ${formatDateTime(booking.actualStartTime)}.`} />;
    }
    return <ActionBox icon={AlertCircle} color="yellow" title="Status Unknown" message="Cannot determine check-in status." />;
};

// A generic component to render the content of the Action Box.
const ActionBox: React.FC<{
    icon: React.ElementType,
    color: 'green' | 'red' | 'yellow' | 'blue',
    title: string,
    message: string,
    buttonText?: string,
    onAction?: () => void,
}> = ({ icon: Icon, color, title, message, buttonText, onAction }) => {
    // ... (Giữ nguyên logic của component này)
    const colors = {
        green: { bg: 'bg-green-50', border: 'border-green-200', icon: 'text-green-600', text: 'text-green-800', button: 'bg-green-600 hover:bg-green-700' },
        red: { bg: 'bg-red-50', border: 'border-red-200', icon: 'text-red-600', text: 'text-red-800', button: 'bg-red-600 hover:bg-red-700' },
        yellow: { bg: 'bg-yellow-50', border: 'border-yellow-200', icon: 'text-yellow-600', text: 'text-yellow-800', button: 'bg-yellow-500 hover:bg-yellow-600' },
        blue: { bg: 'bg-blue-50', border: 'border-blue-200', icon: 'text-blue-600', text: 'text-blue-800', button: 'bg-blue-600 hover:bg-blue-700' },
    };
    const theme = colors[color];

    return (
        <div className={`p-4 rounded-lg flex flex-col sm:flex-row items-center gap-4 ${theme.bg} border ${theme.border}`}>
            <div className="flex-shrink-0">
                <Icon className={`h-8 w-8 ${theme.icon}`} />
            </div>
            <div className="flex-grow text-center sm:text-left">
                <p className={`font-semibold ${theme.text}`}>{title}</p>
                <p className={`text-sm ${theme.text} opacity-90`}>{message}</p>
            </div>
            {buttonText && onAction && (
                <button
                    onClick={onAction}
                    className={`text-white px-6 py-2.5 rounded-lg font-semibold ${theme.button} transition-colors min-w-[150px]`}
                >
                    {buttonText}
                </button>
            )}
        </div>
    );
};