// Booking Page - Chọn ghế và đặt vé
import { useEffect, useState, useMemo } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import {
    Card,
    Typography,
    Button,
    message,
    Spin,
    Row,
    Col,
    Divider,
    Alert
} from 'antd';
import {
    UserOutlined,
    LockOutlined
} from '@ant-design/icons';
import { useBookingStore, type SelectedSeat } from '../../features/booking/store/bookingStore';
import { useSeatingPlan } from '../../features/showtime/hooks/useShowtimes';
import { useCreateBooking } from '../../features/booking/hooks/useBookings';
import dayjs from 'dayjs';

const { Title, Text, Paragraph } = Typography;

const BookingPage = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const showtimeId = searchParams.get('showtimeId');

    const {
        showtime,
        setShowtime,
        selectedSeats,
        toggleSeat,
        clearSelectedSeats,
        totalAmount,
        finalAmount,
        updatePricing,
        resetBooking,
        setCurrentStep,
        setBookingResult,
        setExpiresAt
    } = useBookingStore();

    const { data: seatingPlan, isLoading } = useSeatingPlan(showtimeId || '');
    const createBooking = useCreateBooking();

    // Timer for booking expiration
    const [expiresAt, setExpiresAtLocal] = useState<number>(0);

    useEffect(() => {
        if (seatingPlan) {
            // Calculate total price based on selected seats
            const total = selectedSeats.reduce((sum, seat) => sum + seat.price, 0);
            updatePricing(total);
        }
    }, [selectedSeats, seatingPlan, updatePricing]);

    const handleSeatClick = (seat: {
        id: string;
        row: string;
        column: number;
        seatNumber: string;
        seatTypeId: string;
        isBlocked: boolean;
    }) => {
        if (seat.isBlocked) {
            message.warning('Ghế này đã bị khóa');
            return;
        }

        const seatType = seatingPlan?.seatTypes.find(st => st.id === seat.seatTypeId);

        const selectedSeat: SelectedSeat = {
            id: seat.id,
            seatNumber: seat.seatNumber,
            row: seat.row,
            column: seat.column,
            seatType: seatType?.name || 'Standard',
            price: seatType?.basePrice || seatingPlan?.seatTypes[0]?.basePrice || 0,
        };

        toggleSeat(selectedSeat);
    };

    const isSeatSelected = (seatId: string) => {
        return selectedSeats.some(s => s.id === seatId);
    };

    const isSeatBooked = (seatId: string) => {
        return seatingPlan?.bookedSeats?.some(bs => bs.seatId === seatId);
    };

    const isSeatLocked = (seatId: string) => {
        return seatingPlan?.lockedSeats?.some(ls => ls.seatId === seatId);
    };

    const handleBooking = async () => {
        if (!showtimeId || selectedSeats.length === 0) {
            message.error('Vui lòng chọn ít nhất 1 ghế');
            return;
        }

        try {
            const result = await createBooking.mutateAsync({
                showtimeId,
                seatIds: selectedSeats.map(s => s.id),
            });

            setBookingResult(result.booking.id, result.booking.bookingCode);

            if (result.paymentUrl) {
                // Redirect to payment
                window.location.href = result.paymentUrl;
            } else {
                // No payment needed, go to success
                navigate(`/booking/success?bookingId=${result.booking.id}`);
            }
        } catch (error) {
            message.error('Đặt vé thất bại. Vui lòng thử lại.');
        }
    };

    // Get unique rows
    const rows = useMemo(() => {
        if (!seatingPlan?.seats) return [];
        const rowSet = new Set(seatingPlan.seats.map(s => s.row));
        return Array.from(rowSet).sort();
    }, [seatingPlan]);

    if (isLoading) {
        return (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
                <Spin size="large" />
            </div>
        );
    }

    if (!showtimeId) {
        return (
            <div style={{ padding: '24px', textAlign: 'center' }}>
                <Title level={4}>Không tìm thấy thông tin suất chiếu</Title>
                <Button type="primary" onClick={() => navigate('/')}>
                    Về trang chủ
                </Button>
            </div>
        );
    }

    const seatTypeColors: Record<string, string> = {};
    seatingPlan?.seatTypes?.forEach(st => {
        seatTypeColors[st.id] = st.color || '#1890ff';
    });

    return (
        <div style={{ padding: '24px', background: '#141414', minHeight: '100vh' }}>
            <Title level={2} style={{ color: '#fff', marginBottom: '24px' }}>
                Đặt Vé Xem Phim
            </Title>

            <Row gutter={[24, 24]}>
                {/* Seat Map */}
                <Col xs={24} lg={16}>
                    <Card
                        style={{ background: '#1f1f1f', borderColor: '#333' }}
                        title={
                            <div style={{ color: '#fff' }}>
                                <Text style={{ color: '#fff' }}>{seatingPlan?.screenName}</Text>
                            </div>
                        }
                    >
                        {/* Screen indicator */}
                        <div style={{
                            textAlign: 'center',
                            marginBottom: '24px',
                            padding: '8px',
                            background: 'linear-gradient(to bottom, rgba(255,255,255,0.1), transparent)',
                            borderRadius: '4px'
                        }}>
                            <Text style={{ color: '#666' }}>MÀN HÌNH</Text>
                        </div>

                        {/* Seat Legend */}
                        <div style={{ display: 'flex', gap: '16px', justifyContent: 'center', marginBottom: '24px', flexWrap: 'wrap' }}>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                                <div style={{ width: 24, height: 24, background: '#52c41a', borderRadius: '4px' }}></div>
                                <Text style={{ color: '#ccc' }}>Ghế trống</Text>
                            </div>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                                <div style={{ width: 24, height: 24, background: '#ff4d4f', borderRadius: '4px' }}></div>
                                <Text style={{ color: '#ccc' }}>Đã đặt</Text>
                            </div>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                                <div style={{ width: 24, height: 24, background: '#1890ff', borderRadius: '4px' }}></div>
                                <Text style={{ color: '#ccc' }}>Đang chọn</Text>
                            </div>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                                <LockOutlined style={{ color: '#666' }} />
                                <Text style={{ color: '#ccc' }}>Đang giữ</Text>
                            </div>
                        </div>

                        {/* Seat Grid */}
                        <div style={{
                            display: 'flex',
                            flexDirection: 'column',
                            gap: '4px',
                            overflowX: 'auto',
                            padding: '0 16px'
                        }}>
                            {rows.map(row => (
                                <div key={row} style={{ display: 'flex', justifyContent: 'center', gap: '4px' }}>
                                    <Text style={{ color: '#666', width: 24, textAlign: 'center' }}>{row}</Text>
                                    {seatingPlan?.seats
                                        ?.filter(s => s.row === row)
                                        .sort((a, b) => a.column - b.column)
                                        .map(seat => {
                                            const isSelected = isSeatSelected(seat.id);
                                            const isBooked = isSeatBooked(seat.id);
                                            const isLocked = isSeatLocked(seat.id);

                                            let bgColor = '#52c41a'; // Available
                                            if (isBooked) bgColor = '#ff4d4f';
                                            else if (isSelected) bgColor = '#1890ff';
                                            else if (isLocked) bgColor = '#666';
                                            else if (seatTypeColors[seat.seatTypeId]) bgColor = seatTypeColors[seat.seatTypeId];

                                            return (
                                                <Button
                                                    key={seat.id}
                                                    disabled={isBooked || isLocked}
                                                    onClick={() => handleSeatClick(seat)}
                                                    style={{
                                                        width: 28,
                                                        height: 28,
                                                        padding: 0,
                                                        background: bgColor,
                                                        borderColor: bgColor,
                                                        color: '#fff',
                                                        cursor: isBooked || isLocked ? 'not-allowed' : 'pointer',
                                                    }}
                                                >
                                                    {isLocked ? <LockOutlined /> : seat.column}
                                                </Button>
                                            );
                                        })}
                                    <Text style={{ color: '#666', width: 24, textAlign: 'center' }}>{row}</Text>
                                </div>
                            ))}
                        </div>
                    </Card>
                </Col>

                {/* Booking Summary */}
                <Col xs={24} lg={8}>
                    <Card
                        style={{ background: '#1f1f1f', borderColor: '#333', position: 'sticky', top: 24 }}
                    >
                        <Title level={4} style={{ color: '#fff', marginBottom: '16px' }}>
                            Tóm Tắt Đặt Vé
                        </Title>

                        <Divider style={{ borderColor: '#333', margin: '16px 0' }} />

                        <div style={{ marginBottom: '16px' }}>
                            <Text style={{ color: '#999' }}>Suất chiếu</Text>
                            <br />
                            <Text style={{ color: '#fff', fontWeight: 'bold' }}>
                                {dayjs(showtime?.startTime).format('HH:mm')} - {dayjs(showtime?.endTime).format('HH:mm')}
                            </Text>
                            <br />
                            <Text style={{ color: '#ccc' }}>{dayjs(showtime?.startTime).format('DD/MM/YYYY')}</Text>
                        </div>

                        <Divider style={{ borderColor: '#333', margin: '16px 0' }} />

                        <div>
                            <Text style={{ color: '#999' }}>Ghế đã chọn</Text>
                            {selectedSeats.length === 0 ? (
                                <Paragraph style={{ color: '#666', marginTop: '8px' }}>
                                    Chưa chọn ghế nào
                                </Paragraph>
                            ) : (
                                <div style={{ marginTop: '8px' }}>
                                    {selectedSeats.map(seat => (
                                        <div key={seat.id} style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
                                            <Text style={{ color: '#ccc' }}>
                                                <UserOutlined /> {seat.row}{seat.column} ({seat.seatType})
                                            </Text>
                                            <Text style={{ color: '#fff' }}>{seat.price.toLocaleString('vi-VN')}đ</Text>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>

                        <Divider style={{ borderColor: '#333', margin: '16px 0' }} />

                        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                            <Text style={{ color: '#999' }}>Tổng cộng</Text>
                            <Text style={{ color: '#fff', fontSize: '18px', fontWeight: 'bold' }}>
                                {totalAmount.toLocaleString('vi-VN')}đ
                            </Text>
                        </div>

                        <Button
                            type="primary"
                            danger
                            size="large"
                            block
                            disabled={selectedSeats.length === 0}
                            loading={createBooking.isPending}
                            onClick={handleBooking}
                            style={{ marginTop: '16px' }}
                        >
                            Đặt Vé Ngay
                        </Button>

                        {selectedSeats.length > 0 && (
                            <Alert
                                message="Lưu ý"
                                description="Ghế sẽ được giữ trong 5 phút. Vui lòng hoàn tất thanh toán trước khi hết thời gian."
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

export default BookingPage;
