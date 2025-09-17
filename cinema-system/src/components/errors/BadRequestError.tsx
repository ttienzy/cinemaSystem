// BadRequestError.tsx
import React from 'react';
import { ApiError } from '../../configs/api';

interface BadRequestErrorProps {
    error: ApiError;
}

const BadRequestError: React.FC<BadRequestErrorProps> = ({ error }) => {
    return (
        <div style={{ color: 'orange', border: '1px solid orange', padding: '10px', margin: '10px 0' }}>
            <h4>Lỗi yêu cầu không hợp lệ (400)</h4>
            <p>{error.message} - Vui lòng kiểm tra dữ liệu nhập.</p>
        </div>
    );
};

export default BadRequestError;