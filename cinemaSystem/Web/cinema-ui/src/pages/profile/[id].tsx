// Booking Detail Page - Details of a specific booking
import { useParams, useNavigate } from 'react-router-dom';
import {
    Card,
    Typography,
    Button,
    Tag,
    Spin,
    Divider,
    Row,
    Col,
    Descriptions,
    message,
    Modal,
    Alert
} from 'antd';
import {
    ArrowLeftOutlined,
    QrcodeOutlined,
    PrinterOutlined,
    CopyOutlined,
    CalendarOutlined,
    UserOutlined,
    EnvironmentOutlined,
    ClockCircleOutlined
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { useBooking, useCancelBooking, useRequestRefund } from '../../features/booking/hooks/useBookings';
import { useBookingStore } from '../../features/booking/store/bookingStore';

const { Title, Text } = Typography;

const statusColors: Record<string, string> = {
    Pending: 'orange',
    Confirmed: 'blue',
    Completed: 'green',
    Cancelled: 'red',
    Refunded: 'purple',
    Expired: 'default',
};

const statusLabels: Record<string, string> = {
    Pending: 'Chờ xác nhận',
    Confirmed: 'Đã xác nhận',
    Completed: 'Hoàn thành',
    Cancelled: 'Đã hủy',
    Refunded: 'Đã hoàn tiền',
    Expired: 'Hết hạn',
};

const paymentStatusColors: Record<string, string> = {
    Pending: 'orange',
    Paid: 'green',
    Failed: 'red',
    Refunded: 'purple',
    PartiallyRefunded: 'orange',
};

const paymentStatusLabels: Record<string, string> = {
    Pending: 'Chờ thanh toán',
    Paid: 'Đã thanh toán',
    Failed: 'Thanh toán thất bại',
    Refunded: 'Đã hoàn tiền',
    PartiallyRefunded: 'Hoàn tiền một phần',
};

const BookingDetailPage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { resetBooking } = useBookingStore();

    // Validate id before making query
    const validId = id && id.trim() !== '' ? id : undefined;
    const { data: booking, isLoading, error } = useBooking(validId || '');
    const cancelBooking = useCancelBooking();
    const requestRefund = useRequestRefund();

    const handleCancelBooking = () => {
        Modal.confirm({
            title: 'Hủy đặt vé',
            content: 'Bạn có chắc chắn muốn hủy đơn đặt vé này?',
            okText: 'Hủy đặt vé',
            okType: 'danger',
            cancelText: 'Không',
            onOk: async () => {
                try {
                    await cancelBooking.mutateAsync(id!);
                    message.success('Hủy đặt vé thành công');
                } catch {
                    message.error('Hủy đặt vé thất bại');
                }
            },
        });
    };

    const handleRequestRefund = () => {
        Modal.confirm({
            title: 'Yêu cầu hoàn tiền',
            content: 'Bạn có chắc chắn muốn yêu cầu hoàn tiền cho đơn này?',
            okText: 'Yêu cầu hoàn tiền',
            okType: 'danger',
            cancelText: 'Không',
            onOk: async () => {
                try {
                    await requestRefund.mutateAsync(id!);
                    message.success('Yêu cầu hoàn tiền đã được gửi');
                } catch {
                    message.error('Yêu cầu hoàn tiền thất bại');
                }
            },
        });
    };

    const handleBackToHistory = () => {
        resetBooking();
        navigate('/profile/history');
    };

    const handleCopyBookingCode = () => {
        if (booking?.bookingCode) {
            navigator.clipboard.writeText(booking.bookingCode);
            message.success('Đã sao chép mã đặt vé');
        }
    };

    if (isLoading) {
        return (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
                <Spin size="large" />
            </div>
        );
    }

    if (error || !booking) {
        return (
            <div style={{ padding: '24px', textAlign: 'center' }}>
                <Title level={4}>Không tìm thấy thông tin đặt vé</Title>
                <Button type="primary" onClick={handleBackToHistory}>
                    Quay lại lịch sử đặt vé
                </Button>
            </div>
        );
    }

    const canCancel = booking.status === 'Pending' || booking.status === 'Confirmed';
    const canRequestRefund = booking.status === 'Completed' && booking.paymentStatus === 'Paid';

    return (
        <div style={{ padding: '24px', background: '#141414', minHeight: '100vh' }}>
            <Button
                type="link"
                icon={<ArrowLeftOutlined />}
                onClick={handleBackToHistory}
                style={{ color: '#fff', padding: 0, marginBottom: '16px' }}
            >
                Quay lại lịch sử đặt vé
            </Button>

            <Title level={2} style={{ color: '#fff', marginBottom: '24px' }}>
                Chi tiết đặt vé
            </Title>

            <Row gutter={[24, 24]}>
                {/* Booking Info */}
                <Col xs={24} lg={16}>
                    <Card style={{ background: '#1f1f1f', borderColor: '#333' }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
                            <div>
                                <Text style={{ color: '#fff', fontSize: '16px' }}>Mã đặt vé: </Text>
                                <Text strong style={{ color: '#ff4d4f', fontSize: '20px' }}>
                                    {booking.bookingCode}
                                </Text>
                                <Button
                                    type="text"
                                    icon={<CopyOutlined />}
                                    onClick={handleCopyBookingCode}
                                    style={{ marginLeft: '8px' }}
                                />
                            </div>
                            <div>
                                <Tag color={statusColors[booking.status]} style={{ fontSize: '14px', padding: '4px 12px' }}>
                                    {statusLabels[booking.status]}
                                </Tag>
                            </div>
                        </div>

                        <Divider style={{ borderColor: '#333' }} />

                        <Descriptions column={1} labelStyle={{ color: '#999' }} contentStyle={{ color: '#fff' }}>
                            <Descriptions.Item label={<><CalendarOutlined /> Ngày đặt</>}>
                                {dayjs(booking.bookingDate).format('DD/MM/YYYY HH:mm')}
                            </Descriptions.Item>
                            <Descriptions.Item label={<><ClockCircleOutlined /> Thời hạn thanh toán</>}>
                                {dayjs(booking.expiresAt).format('DD/MM/YYYY HH:mm')}
                            </Descriptions.Item>
                            <Descriptions.Item label="Phương thức thanh toán">
                                {booking.paymentMethod || 'Chưa thanh toán'}
                            </Descriptions.Item>
                            <Descriptions.Item label="Trạng thái thanh toán">
                                <Tag color={paymentStatusColors[booking.paymentStatus]}>
                                    {paymentStatusLabels[booking.paymentStatus]}
                                </Tag>
                            </Descriptions.Item>
                        </Descriptions>
                    </Card>

                    {/* Showtime Info */}
                    {booking.showtime && (
                        <Card style={{ background: '#1f1f1f', borderColor: '#333', marginTop: '16px' }}>
                            <Title level={4} style={{ color: '#fff', marginBottom: '16px' }}>
                                Thông tin suất chiếu
                            </Title>

                            <Descriptions column={1} labelStyle={{ color: '#999' }} contentStyle={{ color: '#fff' }}>
                                <Descriptions.Item label="Phim">
                                    <Text strong style={{ color: '#fff' }}>{booking.showtime.movieTitle}</Text>
                                </Descriptions.Item>
                                <Descriptions.Item label={<><EnvironmentOutlined /> Rạp</>}>
                                    {booking.showtime.cinemaName}
                                </Descriptions.Item>
                                <Descriptions.Item label="Phòng chiếu">
                                    {booking.showtime.screenName}
                                </Descriptions.Item>
                                <Descriptions.Item label={<><ClockCircleOutlined /> Suất chiếu</>}>
                                    {dayjs(booking.showtime.startTime).format('DD/MM/YYYY HH:mm')}
                                </Descriptions.Item>
                            </Descriptions>
                        </Card>
                    )}

                    {/* Seats Info */}
                    <Card style={{ background: '#1f1f1f', borderColor: '#333', marginTop: '16px' }}>
                        <Title level={4} style={{ color: '#fff', marginBottom: '16px' }}>
                            Ghế đã đặt
                        </Title>

                        <Row gutter={[16, 16]}>
                            {booking.seats.map((seat, index) => (
                                <Col xs={12} sm={8} md={6} key={index}>
                                    <Card size="small" style={{ background: '#2a2a2a', borderColor: '#444' }}>
                                        <div style={{ textAlign: 'center' }}>
                                            <Text style={{ color: '#fff' }}>
                                                <UserOutlined /> {seat.row}{seat.column}
                                            </Text>
                                            <br />
                                            <Text type="secondary" style={{ fontSize: '12px' }}>
                                                {seat.seatType}
                                            </Text>
                                            <br />
                                            <Text strong style={{ color: '#ff4d4f' }}>
                                                {seat.price.toLocaleString('vi-VN')}đ
                                            </Text>
                                        </div>
                                    </Card>
                                </Col>
                            ))}
                        </Row>
                    </Card>
                </Col>

                {/* Payment Summary & QR Code */}
                <Col xs={24} lg={8}>
                    <Card style={{ background: '#1f1f1f', borderColor: '#333' }}>
                        <Title level={4} style={{ color: '#fff', marginBottom: '16px' }}>
                            Thanh toán
                        </Title>

                        <div style={{ marginBottom: '12px', display: 'flex', justifyContent: 'space-between' }}>
                            <Text style={{ color: '#999' }}>Tổng tiền:</Text>
                            <Text style={{ color: '#fff' }}>{booking.totalAmount.toLocaleString('vi-VN')}đ</Text>
                        </div>

                        {booking.discountAmount > 0 && (
                            <div style={{ marginBottom: '12px', display: 'flex', justifyContent: 'space-between' }}>
                                <Text style={{ color: '#999' }}>Giảm giá:</Text>
                                <Text style={{ color: '#52c41a' }}>-{booking.discountAmount.toLocaleString('vi-VN')}đ</Text>
                            </div>
                        )}

                        <Divider style={{ borderColor: '#333' }} />

                        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '24px' }}>
                            <Text strong style={{ color: '#fff', fontSize: '18px' }}>Tổng cộng:</Text>
                            <Text strong style={{ color: '#ff4d4f', fontSize: '24px' }}>
                                {booking.finalAmount.toLocaleString('vi-VN')}đ
                            </Text>
                        </div>

                        {booking.qrCode && (
                            <>
                                <Divider style={{ borderColor: '#333' }} />
                                <div style={{ textAlign: 'center' }}>
                                    <Text style={{ color: '#fff', display: 'block', marginBottom: '12px' }}>
                                        <QrcodeOutlined /> Mã QR Check-in
                                    </Text>
                                    <img
                                        src={booking.qrCode}
                                        alt="QR Code"
                                        style={{ maxWidth: '200px', borderRadius: '8px' }}
                                    />
                                </div>
                            </>
                        )}

                        {/* Action Buttons */}
                        <Divider style={{ borderColor: '#333' }} />

                        {canCancel && (
                            <Button
                                danger
                                block
                                onClick={handleCancelBooking}
                                loading={cancelBooking.isPending}
                                style={{ marginBottom: '12px' }}
                            >
                                Hủy đặt vé
                            </Button>
                        )}

                        {canRequestRefund && (
                            <Button
                                danger
                                type="dashed"
                                block
                                onClick={handleRequestRefund}
                                loading={requestRefund.isPending}
                                style={{ marginBottom: '12px' }}
                            >
                                Yêu cầu hoàn tiền
                            </Button>
                        )}

                        <Button block icon={<PrinterOutlined />}>
                            In vé
                        </Button>

                        {booking.status === 'Pending' && (
                            <Alert
                                message="Chưa thanh toán"
                                description="Vui lòng thanh toán trước thời hạn để giữ ghế"
                                type="warning"
                                showIcon
                                style={{ marginTop: '16px' }}
                            />
                        )}
                    </Card>
                </Col>
            </Row>
        </div>
    );
};

export default BookingDetailPage;
