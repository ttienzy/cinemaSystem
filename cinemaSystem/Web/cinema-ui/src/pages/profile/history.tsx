// Booking History Page - List of user's bookings
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    Card,
    Typography,
    List,
    Tag,
    Button,
    Select,
    Spin,
    Empty,
    Pagination,
    DatePicker,
    Row,
    Col
} from 'antd';
import { EyeOutlined, CalendarOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { useMyBookings } from '../../features/booking/hooks/useBookings';
import type { Booking, BookingStatus } from '../../features/booking/types/booking.types';

const { Title, Text, Paragraph } = Typography;
const { Option } = Select;
const { RangePicker } = DatePicker;

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

const BookingHistoryPage = () => {
    const navigate = useNavigate();
    const [pageIndex, setPageIndex] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [status, setStatus] = useState<BookingStatus | undefined>(undefined);
    const [dateRange, setDateRange] = useState<[dayjs.Dayjs | null, dayjs.Dayjs | null]>([null, null]);

    // Fetch bookings
    const { data: bookingsData, isLoading } = useMyBookings({
        pageIndex,
        pageSize,
        status,
        fromDate: dateRange[0]?.format('YYYY-MM-DD'),
        toDate: dateRange[1]?.format('YYYY-MM-DD'),
    });

    const bookings: Booking[] = bookingsData?.items || [];
    const totalCount = bookingsData?.totalCount || 0;

    const handleViewDetails = (bookingId: string) => {
        navigate(`/profile/booking/${bookingId}`);
    };

    const handlePageChange = (page: number, size: number) => {
        setPageIndex(page);
        setPageSize(size);
    };

    const handleStatusChange = (value: string | undefined) => {
        setStatus(value as BookingStatus | undefined);
        setPageIndex(1);
    };

    const handleDateRangeChange = (dates: [dayjs.Dayjs | null, dayjs.Dayjs | null] | null) => {
        setDateRange(dates || [null, null]);
        setPageIndex(1);
    };

    if (isLoading) {
        return (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
                <Spin size="large" />
            </div>
        );
    }

    return (
        <div style={{ padding: '24px', background: '#141414', minHeight: '100vh' }}>
            <Title level={2} style={{ color: '#fff', marginBottom: '24px' }}>
                Lịch sử đặt vé
            </Title>

            {/* Filters */}
            <Card style={{ background: '#1f1f1f', borderColor: '#333', marginBottom: '24px' }}>
                <Row gutter={[16, 16]} align="middle">
                    <Col xs={24} sm={8}>
                        <Text style={{ color: '#fff' }}>Trạng thái:</Text>
                        <Select
                            style={{ width: '100%', marginTop: '8px' }}
                            placeholder="Tất cả trạng thái"
                            allowClear
                            value={status}
                            onChange={handleStatusChange}
                        >
                            <Option value="Pending">Chờ xác nhận</Option>
                            <Option value="Confirmed">Đã xác nhận</Option>
                            <Option value="Completed">Hoàn thành</Option>
                            <Option value="Cancelled">Đã hủy</Option>
                            <Option value="Refunded">Đã hoàn tiền</Option>
                        </Select>
                    </Col>
                    <Col xs={24} sm={16}>
                        <Text style={{ color: '#fff' }}>Ngày đặt:</Text>
                        <RangePicker
                            style={{ width: '100%', marginTop: '8px' }}
                            onChange={handleDateRangeChange}
                            format="DD/MM/YYYY"
                        />
                    </Col>
                </Row>
            </Card>

            {/* Bookings List */}
            {bookings.length === 0 ? (
                <Empty description="Bạn chưa có đơn đặt vé nào" />
            ) : (
                <>
                    <List
                        grid={{ gutter: 16, xs: 1, sm: 1, md: 1, lg: 1 }}
                        dataSource={bookings}
                        renderItem={(booking) => (
                            <List.Item>
                                <Card
                                    style={{ background: '#1f1f1f', borderColor: '#333' }}
                                    actions={[
                                        <Button
                                            type="link"
                                            icon={<EyeOutlined />}
                                            onClick={() => handleViewDetails(booking.id)}
                                        >
                                            Xem chi tiết
                                        </Button>
                                    ]}
                                >
                                    <Card.Meta
                                        title={
                                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                                <Text style={{ color: '#fff' }}>
                                                    Mã đặt vé: <Text strong style={{ color: '#ff4d4f' }}>{booking.bookingCode}</Text>
                                                </Text>
                                                <Tag color={statusColors[booking.status]}>
                                                    {statusLabels[booking.status]}
                                                </Tag>
                                            </div>
                                        }
                                        description={
                                            <div>
                                                <Paragraph style={{ color: '#ccc', marginBottom: '8px' }}>
                                                    <CalendarOutlined /> Ngày đặt: {dayjs(booking.bookingDate).format('DD/MM/YYYY HH:mm')}
                                                </Paragraph>

                                                {booking.showtime && (
                                                    <div style={{ marginBottom: '8px' }}>
                                                        <Text style={{ color: '#fff' }}>Phim: </Text>
                                                        <Text style={{ color: '#ccc' }}>{booking.showtime.movieTitle}</Text>
                                                        <br />
                                                        <Text style={{ color: '#fff' }}>Rạp: </Text>
                                                        <Text style={{ color: '#ccc' }}>{booking.showtime.cinemaName} - {booking.showtime.screenName}</Text>
                                                        <br />
                                                        <Text style={{ color: '#fff' }}>Suất chiếu: </Text>
                                                        <Text style={{ color: '#ccc' }}>
                                                            {dayjs(booking.showtime.startTime).format('DD/MM/YYYY HH:mm')}
                                                        </Text>
                                                    </div>
                                                )}

                                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: '12px' }}>
                                                    <div>
                                                        <Text style={{ color: '#fff' }}>Ghế: </Text>
                                                        <Text style={{ color: '#ccc' }}>
                                                            {booking.seats.map(s => `${s.row}${s.column}`).join(', ')}
                                                        </Text>
                                                    </div>
                                                    <div>
                                                        <Text style={{ color: '#fff', fontSize: '18px', fontWeight: 'bold' }}>
                                                            {booking.finalAmount.toLocaleString('vi-VN')}đ
                                                        </Text>
                                                    </div>
                                                </div>
                                            </div>
                                        }
                                    />
                                </Card>
                            </List.Item>
                        )}
                    />

                    <div style={{ marginTop: '24px', textAlign: 'center' }}>
                        <Pagination
                            current={pageIndex}
                            pageSize={pageSize}
                            total={totalCount}
                            onChange={handlePageChange}
                            showSizeChanger
                            showTotal={(total) => `Tổng ${total} đơn đặt vé`}
                        />
                    </div>
                </>
            )}
        </div>
    );
};

export default BookingHistoryPage;
