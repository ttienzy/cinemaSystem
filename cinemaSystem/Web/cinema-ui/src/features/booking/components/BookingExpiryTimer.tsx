// Booking Expiry Timer Component
import { useEffect, useState } from 'react';
import { Alert, Progress } from 'antd';
import { ClockCircleOutlined } from '@ant-design/icons';
import { useBookingStore } from '../store/bookingStore';

interface BookingExpiryTimerProps {
    onExpired?: () => void;
}

export const BookingExpiryTimer = ({ onExpired }: BookingExpiryTimerProps) => {
    const { expiresAt, clearSelectedSeats, clearExpiresAt } = useBookingStore();
    const [timeLeft, setTimeLeft] = useState<number>(0);
    const [isExpired, setIsExpired] = useState(false);

    useEffect(() => {
        if (!expiresAt) return;

        const calculateTimeLeft = () => {
            const now = new Date().getTime();
            const expiry = new Date(expiresAt).getTime();
            const diff = expiry - now;
            return Math.max(0, Math.floor(diff / 1000)); // seconds
        };

        // Initial calculation
        setTimeLeft(calculateTimeLeft());

        // Update every second
        const interval = setInterval(() => {
            const remaining = calculateTimeLeft();
            setTimeLeft(remaining);

            if (remaining === 0 && !isExpired) {
                setIsExpired(true);
                clearSelectedSeats();
                clearExpiresAt();
                onExpired?.();
            }
        }, 1000);

        return () => clearInterval(interval);
    }, [expiresAt, isExpired, clearSelectedSeats, clearExpiresAt, onExpired]);

    if (!expiresAt || isExpired) return null;

    const minutes = Math.floor(timeLeft / 60);
    const seconds = timeLeft % 60;
    const totalSeconds = 15 * 60; // 15 minutes
    const percent = (timeLeft / totalSeconds) * 100;

    // Warning states
    const isWarning = timeLeft <= 120; // 2 minutes
    const isCritical = timeLeft <= 60; // 1 minute

    const alertType = isCritical ? 'error' : isWarning ? 'warning' : 'info';
    const progressStatus = isCritical ? 'exception' : isWarning ? 'normal' : 'active';

    return (
        <Alert
            message={
                <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                    <ClockCircleOutlined />
                    <span>
                        Thời gian giữ ghế: <strong>{minutes}:{seconds.toString().padStart(2, '0')}</strong>
                    </span>
                </div>
            }
            description={
                <div>
                    <Progress
                        percent={percent}
                        showInfo={false}
                        status={progressStatus}
                        strokeColor={isCritical ? '#ff4d4f' : isWarning ? '#faad14' : '#1890ff'}
                    />
                    {isWarning && (
                        <div style={{ marginTop: 8, fontSize: 12 }}>
                            {isCritical
                                ? 'Vui lòng hoàn tất thanh toán ngay!'
                                : 'Ghế sẽ được giải phóng sau 2 phút nữa'}
                        </div>
                    )}
                </div>
            }
            type={alertType}
            style={{ marginBottom: 16 }}
        />
    );
};

export default BookingExpiryTimer;
