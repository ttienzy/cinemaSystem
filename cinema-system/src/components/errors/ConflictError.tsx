// ConflictError.tsx
import React from 'react';
import { ApiError } from '../../configs/api';

interface ConflictErrorProps {
    error: ApiError;
}

const ConflictError: React.FC<ConflictErrorProps> = ({ error }) => {
    return (
        <div style={{ color: 'purple', border: '1px solid purple', padding: '10px', margin: '10px 0' }}>
            <h4>Xung đột (409)</h4>
            <p>{error.message} - Dữ liệu đã tồn tại hoặc xung đột.</p>
        </div>
    );
};

export default ConflictError;