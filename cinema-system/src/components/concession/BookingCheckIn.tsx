// src/components/BookingCheckIn.tsx (or your file path)

import React, { useState, type KeyboardEvent } from 'react';
import { Search, Calendar, Clock, MapPin, Users, CreditCard, CheckCircle, AlertCircle, XCircle, Ticket, Info } from 'lucide-react';

// --- TYPE IMPORTS ---
import type { BookingCheckedInResponse } from '../../types/booking.types';
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { checkInBooking, confirmBookingCheckIn } from '../../store/slices/bookingSlice';

// =================================================================
// MAIN COMPONENT
// =================================================================
const BookingCheckIn: React.FC = () => {
    // --- STATE MANAGEMENT ---
    // Manages the user input for the booking code.
    const [bookingCode, setBookingCode] = useState('');
    // Manages a local error message for input validation.
    const [clientError, setClientError] = useState('');

    // --- REDUX HOOKS ---
    // The dispatch function to call Redux actions.
    const dispatch = useAppDispatch();
    // Selects the relevant state from the Redux store. 'checkedIn' can be null initially.
    // 'loading' is true when an API call is in progress.
    // 'error' holds API-related errors from the slice.
    const { checkedIn, loading, error: apiError } = useAppSelector(state => state.booking);

    // --- EVENT HANDLERS ---

    // Handles the search button click.
    const handleSearch = () => {
        // Simple client-side validation.
        if (!bookingCode.trim()) {
            setClientError('Please enter a booking code to start.');
            return;
        }
        setClientError(''); // Clear previous client errors.
        // Dispatch the Redux action to fetch booking details.
        dispatch(checkInBooking(bookingCode));
    };

    // Handles the "Enter" key press in the input field.
    const handleKeyPress = (e: KeyboardEvent) => {
        if (e.key === 'Enter') {
            handleSearch();
        }
    };

    // Handles the check-in button click.
    const handleCheckIn = () => {
        if (!checkedIn) return; // Safety check.
        // Dispatch the action to confirm the check-in.
        dispatch(confirmBookingCheckIn(bookingCode));
    };

    // --- RENDER ---
    return (
        <div className="p-4 sm:p-6 max-w-4xl mx-auto bg-gray-50 min-h-screen">
            <div className="bg-white p-6 rounded-xl shadow-md">

                {/* Page Header */}
                <div className="text-center border-b border-gray-200 pb-6 mb-6">
                    <h1 className="text-3xl font-bold text-gray-800">Booking Check-In</h1>
                    <p className="text-gray-500 mt-2">Enter a booking code to retrieve details and check-in.</p>
                </div>

                {/* Search Section */}
                <div className="mb-6">
                    <div className="flex flex-col sm:flex-row gap-3">
                        <div className="flex-grow relative">
                            <Ticket className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
                            <input
                                type="text"
                                value={bookingCode}
                                onChange={(e) => setBookingCode(e.target.value.toUpperCase())}
                                onKeyPress={handleKeyPress}
                                placeholder="Enter booking code (e.g., ZN8C2)"
                                className="pl-12 w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-base"
                            />
                        </div>
                        <button
                            onClick={handleSearch}
                            disabled={loading}
                            className="bg-indigo-600 text-white px-6 py-3 rounded-lg hover:bg-indigo-700 disabled:opacity-60 disabled:cursor-not-allowed flex items-center justify-center gap-2 font-semibold transition-colors"
                        >
                            <Search className="h-5 w-5" />
                            {loading ? 'Searching...' : 'Search'}
                        </button>
                    </div>
                    {/* Display client-side or API errors */}
                    {(clientError || apiError) && (
                        <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-center gap-3 text-sm">
                            <XCircle className="h-5 w-5 text-red-500 flex-shrink-0" />
                            <span className="text-red-700">{clientError || apiError}</span>
                        </div>
                    )}
                </div>

                {/* Results Section */}
                <div className="mt-8">
                    {loading && <LoadingSpinner />}
                    {!loading && checkedIn && <BookingDetails data={checkedIn} onCheckIn={handleCheckIn} />}
                    {!loading && !checkedIn && !apiError && <InitialState />}
                </div>
            </div>
        </div>
    );
};


// =================================================================
// CHILD COMPONENTS & HELPERS
// =================================================================

// --- Utility Functions ---

const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
};

const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString('vi-VN', {
        year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit'
    });
};

const getStatusInfo = (status: string) => {
    switch (status) {
        case 'Pending': return { text: 'Pending Payment', color: 'bg-yellow-100 text-yellow-800' };
        case 'Completed': return { text: 'Completed', color: 'bg-green-100 text-green-800' };
        case 'Cancelled': return { text: 'Cancelled', color: 'bg-red-100 text-red-800' };
        default: return { text: 'Unknown', color: 'bg-gray-100 text-gray-800' };
    }
};

// --- Child Components ---

const LoadingSpinner: React.FC = () => (
    <div className="text-center py-10">
        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-600 mx-auto"></div>
        <p className="mt-4 text-gray-600">Retrieving booking details...</p>
    </div>
);

const InitialState: React.FC = () => (
    <div className="text-center py-10 border-2 border-dashed border-gray-300 rounded-lg bg-gray-50">
        <Ticket className="mx-auto h-12 w-12 text-gray-400" />
        <h3 className="mt-4 text-lg font-medium text-gray-800">Ready to find your booking</h3>
        <p className="mt-1 text-sm text-gray-500">
            Enter the code from your confirmation email to get started.
        </p>
    </div>
);

// This is the main component for displaying booking info.
const BookingDetails: React.FC<{ data: BookingCheckedInResponse; onCheckIn: () => void; }> = ({ data, onCheckIn }) => {
    const statusInfo = getStatusInfo(data.status);

    return (
        <div className="border border-gray-200 rounded-xl shadow-sm animate-fade-in-down">
            {/* Header */}
            <div className="p-6 border-b border-gray-200">
                <div className="flex flex-col sm:flex-row justify-between sm:items-center gap-4">
                    <div>
                        <h2 className="text-2xl font-bold text-gray-900">{data.movieTitle}</h2>
                        <p className="text-gray-500">Booking Code: <span className="font-mono font-medium text-gray-700">{data.movieTitle}</span></p>
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
const InfoItem: React.FC<{ icon: React.ElementType, label: string, value: string, isHighlight?: boolean }> = ({ icon: Icon, label, value, isHighlight }) => (
    <div className="flex items-start gap-4">
        <Icon className="h-5 w-5 text-gray-400 mt-0.5 flex-shrink-0" />
        <div>
            <p className="text-sm text-gray-500">{label}</p>
            <p className={`font-semibold ${isHighlight ? 'text-xl text-green-600' : 'text-gray-800'}`}>{value}</p>
        </div>
    </div>
);

// This function contains the logic for what to display in the action box.
// It prioritizes statuses to avoid showing multiple conflicting messages.
const renderActionBoxContent = (booking: BookingCheckedInResponse, onCheckIn: () => void) => {
    const now = new Date();
    const startTime = new Date(booking.actualStartTime);
    const endTime = new Date(booking.actualEndTime);

    // Priority 1: Already checked in (Success state)
    if (booking.isCheckedIn) {
        return <ActionBox icon={CheckCircle} color="green" title="Successfully Checked-In" message="Enjoy the movie!" />;
    }
    // Priority 2: Booking is cancelled (Terminal state)
    if (booking.status === 'Cancelled') {
        return <ActionBox icon={XCircle} color="red" title="Booking Cancelled" message="This booking cannot be used." />;
    }
    // Priority 3: Payment not completed (Terminal state for check-in)
    if (booking.status !== 'Completed') {
        return <ActionBox icon={AlertCircle} color="yellow" title="Payment Not Completed" message="Please complete the payment to enable check-in." />;
    }
    // Priority 4: Showtime has passed (Terminal state)
    if (now > endTime) {
        return <ActionBox icon={XCircle} color="red" title="Showtime Expired" message="This showtime has already ended." />;
    }
    // Priority 5: Ready for check-in (Primary Action)
    if (now >= startTime && now <= endTime) {
        return <ActionBox icon={CheckCircle} color="green" title="Ready for Check-In" message="Please check-in to validate your ticket before entering." buttonText="Check-In Now" onAction={onCheckIn} />;
    }
    // Priority 6: Showtime has not started yet (Info state)
    if (now < startTime) {
        return <ActionBox icon={Info} color="blue" title="Not Yet Time for Check-In" message={`Check-in will be available from ${formatDateTime(booking.actualStartTime)}.`} />;
    }

    // Default fallback (should not be reached with the logic above)
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

export default BookingCheckIn;