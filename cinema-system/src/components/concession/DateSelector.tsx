import React from 'react';

interface DateSelectorProps {
    dates: Date[];
    selectedDate: string;
    onDateChange: (date: string) => void;
    formatDate: (date: string) => string;
}

// Helper function to convert Date to YYYY-MM-DD in local timezone
const getLocalDateString = (date: Date): string => {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
};

const DateSelector: React.FC<DateSelectorProps> = ({
    dates,
    selectedDate,
    onDateChange,
    formatDate
}) => {
    return (
        <div className="flex overflow-x-auto space-x-2 mb-6 pb-2">
            {dates.map(date => {
                const dateStr = getLocalDateString(date);
                const isSelected = selectedDate === dateStr;

                return (
                    <button
                        key={dateStr}
                        onClick={() => onDateChange(dateStr)}
                        className={`flex-shrink-0 px-4 py-2 rounded-lg text-sm font-medium transition-colors ${isSelected
                                ? 'bg-blue-600 text-white'
                                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                            }`}
                    >
                        {formatDate(dateStr)}
                    </button>
                );
            })}
        </div>
    );
};

export default DateSelector;