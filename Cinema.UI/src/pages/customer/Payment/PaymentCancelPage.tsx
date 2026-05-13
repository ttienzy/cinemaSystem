import { useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Result, Button } from 'antd';
import { InfoCircleOutlined } from '@ant-design/icons';
import { getAccessToken } from '../../../utils/tokenStorage';
import { getApiGatewayBaseUrl } from '../../../utils/apiConfig';

const PaymentCancelPage = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const bookingId = searchParams.get('bookingId');

    useEffect(() => {
        if (!bookingId || !getAccessToken()) {
            return;
        }

        void fetch(`${getApiGatewayBaseUrl()}/api/bookings/${bookingId}/cancel`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                Authorization: `Bearer ${getAccessToken()}`
            },
            body: JSON.stringify({
                userId: 'current-user',
                cancellationReason: 'Payment cancelled by customer'
            })
        }).catch(() => undefined);
    }, [bookingId]);

    return (
        <div style={{ padding: '60px 20px', maxWidth: 600, margin: '0 auto' }}>
            <Result
                icon={<InfoCircleOutlined style={{ color: '#faad14' }} />}
                status="warning"
                title="Đã hủy thanh toán"
                subTitle="Bạn đã hủy giao dịch thanh toán. Ghế đã giữ sẽ được tự động hủy sau thời gian hết hạn."
                extra={[
                    <Button
                        type="primary"
                        key="home"
                        onClick={() => navigate('/')}
                    >
                        Về trang chủ
                    </Button>,
                    <Button
                        key="movies"
                        onClick={() => navigate('/')}
                    >
                        Chọn phim khác
                    </Button>,
                ]}
            />
            {bookingId && (
                <p style={{ textAlign: 'center', marginTop: 20, color: '#999' }}>
                    Mã booking: {bookingId}
                </p>
            )}
        </div>
    );
};

export default PaymentCancelPage;
