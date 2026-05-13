import { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Result, Button } from 'antd';
import { CheckCircleOutlined } from '@ant-design/icons';

const REDIRECT_DELAY_SECONDS = 5;

const PaymentSuccessPage = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const bookingId = searchParams.get('bookingId');
    const [secondsLeft, setSecondsLeft] = useState(REDIRECT_DELAY_SECONDS);

    useEffect(() => {
        const intervalId = window.setInterval(() => {
            setSecondsLeft((value) => Math.max(value - 1, 0));
        }, 1000);

        const timeoutId = window.setTimeout(() => {
            navigate('/');
        }, REDIRECT_DELAY_SECONDS * 1000);

        return () => {
            window.clearInterval(intervalId);
            window.clearTimeout(timeoutId);
        };
    }, [navigate]);

    const subtitle = bookingId
        ? `Mã booking: ${bookingId}. Bạn sẽ được chuyển về trang chính sau ${secondsLeft} giây.`
        : `Bạn sẽ được chuyển về trang chính sau ${secondsLeft} giây.`;

    return (
        <div style={{ padding: '60px 20px', maxWidth: 600, margin: '0 auto' }}>
            <Result
                icon={<CheckCircleOutlined style={{ color: '#52c41a' }} />}
                status="success"
                title="Thanh toán thành công!"
                subTitle={subtitle}
                extra={[
                    <Button
                        type="primary"
                        key="home"
                        onClick={() => navigate('/')}
                    >
                        Về trang chính
                    </Button>,
                ]}
            />
        </div>
    );
};

export default PaymentSuccessPage;
