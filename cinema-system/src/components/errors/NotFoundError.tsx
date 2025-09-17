// NotFoundError.tsx
import React from 'react';
import { ApiError } from '../../configs/api';

interface NotFoundErrorProps {
    error: ApiError;
}

const NotFoundError: React.FC<NotFoundErrorProps> = ({ error }) => {
    return (
        <div style={{ color: 'blue', border: '1px solid blue', padding: '10px', margin: '10px 0' }}>
            <h4>Không tìm thấy (404)</h4>
            <p>{error.message} - Tài nguyên không tồn tại.</p>
        </div>
    );
};

export default NotFoundError;