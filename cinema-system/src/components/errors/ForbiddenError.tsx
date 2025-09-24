// ForbiddenError.tsx
import React from 'react';




const ForbiddenError: React.FC = () => {
    return (
        <div style={{ color: 'red', border: '1px solid red', padding: '10px', margin: '10px 0' }}>
            <h4>Truy cập bị từ chối (403)</h4>
            <p>Bạn không có quyền thực hiện hành động này.</p>
        </div>
    );
};

export default ForbiddenError;