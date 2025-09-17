interface DateSelectorProps {
    dates: Date[];
    selectedDate: string;
    onDateChange: (date: string) => void;
    formatDate: (date: string) => string;
}

const DateSelector: React.FC<DateSelectorProps> = ({ dates, selectedDate, onDateChange, formatDate }) => {
    return (
        <div className="flex overflow-x-auto space-x-2 mb-6 pb-2">
            {dates.map(date => {
                const dateStr = date.toISOString();
                return (
                    <button
                        key={dateStr}
                        onClick={() => onDateChange(dateStr.split('T')[0])}
                        className={`flex-shrink-0 px-4 py-2 rounded-lg text-sm font-medium ${selectedDate === dateStr.split('T')[0]
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