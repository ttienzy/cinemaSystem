import { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Result, Button, Spin } from 'antd';
import { CheckCircleOutlined } from '@ant-design/icons';
import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { getAccessToken } from '../../../utils/tokenStorage';

const BOOKING_HUB_URL = 'http://localhost:5200/hubs/booking';
const VERIFICATION_TIMEOUT = 30000; // 30 seconds

const PaymentSuccessPage = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const bookingId = searchParams.get('bookingId');
    const [isVerifying, setIsVerifying] = useState(true);
    const [isConfirmed, setIsConfirmed] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!bookingId) {
            setIsVerifying(false);
            setError('Không tìm thấy mã booking');
            return;
        }

        console.log('[Payment Success] Connecting to SignalR for booking:', bookingId);

        let timeoutId: ReturnType<typeof setTimeout>;
        let connection: any = null;

        const connectToSignalR = async () => {
            try {
                const token = getAccessToken();
                if (!token) {
                    console.error('[Payment Success] No authentication token found');
                    setError('Vui lòng đăng nhập lại');
                    setIsVerifying(false);
                    return;
                }

                // Create SignalR connection
                connection = new HubConnectionBuilder()
                    .withUrl(BOOKING_HUB_URL, {
                        accessTokenFactory: () => token
                    })
                    .withAutomaticReconnect()
                    .build();

                // Handle BookingConfirmed event
                connection.on('BookingConfirmed', (id: string, status: string) => {
                    console.log('[Payment Success] Booking confirmed via SignalR:', id, status);
                    if (id === bookingId) {
                        setIsConfirmed(true);
                        setIsVerifying(false);
                        clearTimeout(timeoutId);
                    }
                });

                // Handle BookingFailed event
                connection.on('BookingFailed', (id: string, reason: string) => {
                    console.log('[Payment Success] Booking failed:', id, reason);
                    if (id === bookingId) {
                        setError(reason);
                        setIsVerifying(false);
                        clearTimeout(timeoutId);
                    }
                });

                // Handle connection errors
                connection.onclose((error?: Error) => {
                    if (error) {
                        console.error('[Payment Success] SignalR connection closed with error:', error);
                    } else {
                        console.log('[Payment Success] SignalR connection closed');
                    }
                });

                // Start connection
                await connection.start();
                console.log('[Payment Success] SignalR connected successfully');

                // Join booking group
                await connection.invoke('JoinBookingGroup', bookingId);
                console.log('[Payment Success] Joined booking group:', bookingId);

                // Set timeout for verification
                timeoutId = setTimeout(() => {
                    console.log('[Payment Success] Verification timeout reached');
                    setIsVerifying(false);
                    if (!isConfirmed) {
                        setError('Xác nhận thanh toán đang được xử lý. Vui lòng kiểm tra email.');
                    }
                }, VERIFICATION_TIMEOUT);

            } catch (err) {
                console.error('[Payment Success] SignalR connection error:', err);
                setError('Không thể kết nối đến server. Vui lòng kiểm tra email để xác nhận.');
                setIsVerifying(false);
            }
        };

        connectToSignalR();

        // Cleanup function
        return () => {
            console.log('[Payment Success] Cleanup - disconnecting SignalR');
            clearTimeout(timeoutId);
            if (connection && connection.state === HubConnectionState.Connected) {
                connection.invoke('LeaveBookingGroup', bookingId)
                    .catch((err: Error) => console.error('[Payment Success] Error leaving group:', err))
                    .finally(() => {
                        connection.stop()
                            .catch((err: Error) => console.error('[Payment Success] Error stopping connection:', err));
                    });
            }
        };
    }, [bookingId, isConfirmed]);

    if (isVerifying) {
        return (
            <div style={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                minHeight: '60vh',
                padding: '40px 20px'
            }}>
                <Spin size="large" />
                <p style={{ marginTop: 20, fontSize: 16, color: '#666' }}>
                    Đang xác nhận thanh toán...
                </p>
                <p style={{ fontSize: 14, color: '#999' }}>
                    Vui lòng không đóng trang này
                </p>
            </div>
        );
    }

    if (error) {
        return (
            <div style={{ padding: '60px 20px', maxWidth: 600, margin: '0 auto' }}>
                <Result
                    status="warning"
                    title="Đang xử lý thanh toán"
                    subTitle={error}
                    extra={[
                        <Button
                            type="primary"
                            key="view"
                            onClick={() => navigate(`/success/${bookingId}`)}
                        >
                            Xem chi tiết vé
                        </Button>,
                        <Button key="home" onClick={() => navigate('/')}>
                            Về trang chủ
                        </Button>,
                    ]}
                />
            </div>
        );
    }

    return (
        <div style={{ padding: '60px 20px', maxWidth: 600, margin: '0 auto' }}>
            <Result
                icon={<CheckCircleOutlined style={{ color: '#52c41a' }} />}
                status="success"
                title="Thanh toán thành công!"
                subTitle={`Đặt vé thành công. Mã booking: ${bookingId}`}
                extra={[
                    <Button
                        type="primary"
                        key="view"
                        onClick={() => navigate(`/success/${bookingId}`)}
                    >
                        Xem chi tiết vé
                    </Button>,
                    <Button key="home" onClick={() => navigate('/')}>
                        Về trang chủ
                    </Button>,
                ]}
            />
        </div>
    );
};

export default PaymentSuccessPage;
