// ErrorDisplay.tsx
import React from 'react';
import { ApiError } from '../../configs/api'; // Import từ api.ts (giả sử cùng folder)

interface ErrorDisplayProps {
    error: ApiError;
    onRetry?: () => void; // Optional callback để retry
}

const ErrorDisplay: React.FC<ErrorDisplayProps> = ({ error, onRetry }) => {
    return (
        <div style={{ color: 'red', border: '1px solid red', padding: '10px', margin: '10px 0' }}>
            <h3>Lỗi: {error.statusCode}</h3>
            <p>{error.message}</p>
            {onRetry && <button onClick={onRetry}>Thử lại</button>}
        </div>
    );
};

export default ErrorDisplay;