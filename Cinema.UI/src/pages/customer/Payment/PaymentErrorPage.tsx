import { useSearchParams, useNavigate } from 'react-router-dom';
import { Result, Button } from 'antd';
import { CloseCircleOutlined } from '@ant-design/icons';

const PaymentErrorPage = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const bookingId = searchParams.get('bookingId');

    return (
        <div style={{ padding: '60px 20px', maxWidth: 600, margin: '0 auto' }}>
            <Result
                icon={<CloseCircleOutlined style={{ color: '#ff4d4f' }} />}
                status="error"
                title="Thanh toán thất bại"
                subTitle="Đã xảy ra lỗi trong quá trình thanh toán. Vui lòng thử lại hoặc liên hệ hỗ trợ."
                extra={[
                    <Button
                        type="primary"
                        key="home"
                        onClick={() => navigate('/')}
                    >
                        Về trang chủ
                    </Button>,
                    <Button
                        key="support"
                        onClick={() => window.location.href = 'mailto:support@cinemahub.vn'}
                    >
                        Liên hệ hỗ trợ
                    </Button>,
                ]}
            />
            {bookingId && (
                <p style={{ textAlign: 'center', marginTop: 20, color: '#999' }}>
                    Mã tham chiếu: {bookingId}
                </p>
            )}
        </div>
    );
};

export default PaymentErrorPage;
