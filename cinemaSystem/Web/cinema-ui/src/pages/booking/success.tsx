// Booking Success Page - Payment successful
import { useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Result, Button, Card, Typography, Spin, Descriptions, Tag, Row, Col } from 'antd';
import { CheckCircleOutlined, HomeOutlined, EyeOutlined } from '@ant-design/icons';
import { useBooking } from '../../features/booking/hooks/useBookings';
import { useBookingStore } from '../../features/booking/store/bookingStore';
import dayjs from 'dayjs';

const { Title, Text } = Typography;

const BookingSuccessPage = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const bookingId = searchParams.get('bookingId') || '';
    const { resetBooking } = useBookingStore();

    // Fetch booking details only if bookingId exists (enabled: !!id handles empty string)
    const { data: booking, isLoading } = useBooking(bookingId);

    useEffect(() => {
        // Clear booking store on success
        resetBooking();
    }, [resetBooking]);

    if (isLoading) {
        return (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
                <Spin size="large" />
            </div>
        );
    }

    return (
        <div style={{ padding: '24px', background: '#141414', minHeight: '100vh' }}>
            <Result
                icon={<CheckCircleOutlined style={{ color: '#52c41a', fontSize: '72px' }} />}
                title={<span style={{ color: '#fff' }}>Đặt vé thành công!</span>}
                subTitle={<span style={{ color: '#999' }}>Cảm ơn bạn đã đặt vé xem phim tại Cinema System</span>}
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
                        {bookingId && (
                            <Button
                                size="large"
                                icon={<EyeOutlined />}
                                onClick={() => navigate(`/profile/booking/${bookingId}`)}
                            >
                                Xem chi tiết vé
                            </Button>
                        )}
                    </div>
                }
            />

            {booking && (
                <Row justify="center">
                    <Col xs={24} md={16} lg={12}>
                        <Card style={{ background: '#1f1f1f', borderColor: '#333' }}>
                            <Title level={4} style={{ color: '#fff', textAlign: 'center', marginBottom: '24px' }}>
                                Thông tin đặt vé
                            </Title>

                            <Descriptions column={1} labelStyle={{ color: '#999' }} contentStyle={{ color: '#fff' }}>
                                <Descriptions.Item label="Mã đặt vé">
                                    <Text strong style={{ color: '#ff4d4f', fontSize: '18px' }}>
                                        {booking.bookingCode}
                                    </Text>
                                </Descriptions.Item>

                                {booking.showtime && (
                                    <>
                                        <Descriptions.Item label="Phim">
                                            {booking.showtime.movieTitle}
                                        </Descriptions.Item>
                                        <Descriptions.Item label="Rạp">
                                            {booking.showtime.cinemaName} - {booking.showtime.screenName}
                                        </Descriptions.Item>
                                        <Descriptions.Item label="Suất chiếu">
                                            {dayjs(booking.showtime.startTime).format('DD/MM/YYYY HH:mm')}
                                        </Descriptions.Item>
                                    </>
                                )}

                                <Descriptions.Item label="Ghế">
                                    {booking.seats.map(s => `${s.row}${s.column}`).join(', ')}
                                </Descriptions.Item>

                                <Descriptions.Item label="Tổng tiền">
                                    <Text strong style={{ color: '#ff4d4f', fontSize: '20px' }}>
                                        {booking.finalAmount.toLocaleString('vi-VN')}đ
                                    </Text>
                                </Descriptions.Item>

                                <Descriptions.Item label="Trạng thái">
                                    <Tag color="green">Đã thanh toán</Tag>
                                </Descriptions.Item>
                            </Descriptions>

                            <div style={{ marginTop: '24px', textAlign: 'center' }}>
                                <Text type="secondary" style={{ color: '#999' }}>
                                    Vui lòng đến rạp trước giờ chiếu 15 phút và mang theo mã đặt vé
                                </Text>
                            </div>
                        </Card>
                    </Col>
                </Row>
            )}

            {!booking && bookingId && (
                <div style={{ textAlign: 'center', marginTop: '24px' }}>
                    <Text type="secondary" style={{ color: '#999' }}>
                        Mã đặt vé của bạn: <Text strong style={{ color: '#fff' }}>{bookingId}</Text>
                    </Text>
                </div>
            )}
        </div>
    );
};

export default BookingSuccessPage;
