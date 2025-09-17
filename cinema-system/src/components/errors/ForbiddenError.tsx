// ForbiddenError.tsx
import React from 'react';
import { ApiError } from '../../configs/api';

interface ForbiddenErrorProps {
    error: ApiError;
}

const ForbiddenError: React.FC<ForbiddenErrorProps> = ({ error }) => {
    return (
        <div style={{ color: 'red', border: '1px solid red', padding: '10px', margin: '10px 0' }}>
            <h4>Truy cập bị từ chối (403)</h4>
            <p>{error.message} - Bạn không có quyền thực hiện hành động này.</p>
        </div>
    );
};

export default ForbiddenError;