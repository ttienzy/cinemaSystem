import React from 'react';
import { Button, Result } from 'antd';
import { useNavigate } from 'react-router-dom';

const Unauthorized: React.FC = () => {
    const navigate = useNavigate();

    return (
        <div style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            height: '100vh',
            background: '#f0f2f5'
        }}>
            <Result
                status="403"
                title="403"
                subTitle="Xin lỗi, bạn không có quyền truy cập trang này."
                extra={
                    <div style={{ display: 'flex', gap: 8, justifyContent: 'center' }}>
                        <Button type="primary" onClick={() => navigate('/')}>
                            Về trang chủ
                        </Button>
                        <Button onClick={() => navigate(-1)}>
                            Quay lại
                        </Button>
                    </div>
                }
            />
        </div>
    );
};

export default Unauthorized;
