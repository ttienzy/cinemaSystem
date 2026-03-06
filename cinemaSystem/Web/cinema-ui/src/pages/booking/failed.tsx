// Booking Failed Page - Payment failed
import { useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Result, Button, Card, Typography, Descriptions, Row, Col } from 'antd';
import { CloseCircleOutlined, HomeOutlined, ReloadOutlined } from '@ant-design/icons';
import { useBookingStore } from '../../features/booking/store/bookingStore';

const { Title, Text } = Typography;

const BookingFailedPage = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const bookingId = searchParams.get('bookingId');
    const responseCode = searchParams.get('vnp_ResponseCode');
    const message = searchParams.get('message');

    const { resetBooking } = useBookingStore();

    useEffect(() => {
        // Clear booking store on failure
        resetBooking();
    }, [resetBooking]);

    const handleRetry = () => {
        if (bookingId) {
            navigate(`/profile/booking/${bookingId}`);
        } else {
            navigate('/');
        }
    };

    return (
        <div style={{ padding: '24px', background: '#141414', minHeight: '100vh' }}>
            <Result
                icon={<CloseCircleOutlined style={{ color: '#ff4d4f', fontSize: '72px' }} />}
                title={<span style={{ color: '#fff' }}>Thanh toán thất bại</span>}
                subTitle={<span style={{ color: '#999' }}>Đã có lỗi xảy ra trong quá trình thanh toán. Vui lòng thử lại.</span>}
                extra={
                    <div style={{ display: 'flex', gap: '16px', justifyContent: 'center', flexWrap: 'wrap' }}>
                        <Button
                            type="primary"
                            size="large"
                            icon={<HomeOutlined />}
                            onClick={() => navigate('/')}
                        >
                            Về trang chủ
                        </Button>
                        <Button
                            size="large"
                            icon={<ReloadOutlined />}
                            onClick={handleRetry}
                        >
                            Thử lại
                        </Button>
                    </div>
                }
            />

            <Row justify="center">
                <Col xs={24} md={16} lg={12}>
                    <Card style={{ background: '#1f1f1f', borderColor: '#333' }}>
                        <Title level={4} style={{ color: '#fff', textAlign: 'center', marginBottom: '24px' }}>
                            Thông tin lỗi
                        </Title>

                        <Descriptions column={1} labelStyle={{ color: '#999' }} contentStyle={{ color: '#fff' }}>
                            {bookingId && (
                                <Descriptions.Item label="Mã đặt vé">
                                    <Text strong style={{ color: '#fff' }}>
                                        {bookingId}
                                    </Text>
                                </Descriptions.Item>
                            )}

                            {responseCode && (
                                <Descriptions.Item label="Mã lỗi">
                                    <Text strong style={{ color: '#ff4d4f' }}>
                                        {responseCode}
                                    </Text>
                                </Descriptions.Item>
                            )}

                            {message && (
                                <Descriptions.Item label="Thông báo lỗi">
                                    <Text style={{ color: '#fff' }}>
                                        {message}
                                    </Text>
                                </Descriptions.Item>
                            )}

                            {!responseCode && !message && (
                                <Descriptions.Item label="Nguyên nhân">
                                    <Text style={{ color: '#fff' }}>
                                        Giao dịch không thành công. Vui lòng kiểm tra lại thông tin thanh toán hoặc liên hệ hỗ trợ.
                                    </Text>
                                </Descriptions.Item>
                            )}
                        </Descriptions>

                        <div style={{ marginTop: '24px', textAlign: 'center' }}>
                            <Text type="secondary" style={{ color: '#999' }}>
                                Nếu bạn đã bị trừ tiền nhưng giao dịch thất bại, vui lòng liên hệ hỗ trợ để được hoàn tiền.
                            </Text>
                        </div>
                    </Card>
                </Col>
            </Row>
        </div>
    );
};

export default BookingFailedPage;
