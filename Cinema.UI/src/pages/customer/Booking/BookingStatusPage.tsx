import React, { useEffect, useMemo, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Button, Card, Spin, message } from 'antd';
import { LoadingOutlined } from '@ant-design/icons';
import { bookingApi } from '../../../features/booking/api/bookingApi';
import { getPaymentApiBaseUrl } from '../../../utils/apiConfig';

const maxAttempts = 30;
const pollIntervalMs = 1000;

const BookingStatusPage: React.FC = () => {
  const { bookingId } = useParams<{ bookingId: string }>();
  const navigate = useNavigate();
  const [attempts, setAttempts] = useState(0);
  const [hasFailed, setHasFailed] = useState(false);

  const paymentApiBaseUrl = useMemo(() => getPaymentApiBaseUrl(), []);

  useEffect(() => {
    if (!bookingId) {
      message.error('Booking ID không hợp lệ');
      navigate('/');
      return;
    }

    if (hasFailed || attempts >= maxAttempts) {
      setHasFailed(true);
      return;
    }

    let cancelled = false;

    const timer = window.setTimeout(async () => {
      try {
        const payment = await bookingApi.getPaymentByBookingId(bookingId);
        if (cancelled) {
          return;
        }

        const paymentId = payment?.paymentId || payment?.id;
        if (paymentId) {
          message.success('Đang chuyển đến trang thanh toán...');
          window.location.href = `${paymentApiBaseUrl}/api/payments/${paymentId}/checkout`;
          return;
        }
      } catch {
        if (!cancelled && attempts + 1 >= maxAttempts) {
          message.error('Không thể tạo liên kết thanh toán. Vui lòng thử lại sau.');
        }
      }

      if (!cancelled) {
        setAttempts((current) => current + 1);
      }
    }, attempts === 0 ? 300 : pollIntervalMs);

    return () => {
      cancelled = true;
      window.clearTimeout(timer);
    };
  }, [attempts, bookingId, hasFailed, navigate, paymentApiBaseUrl]);

  const polling = !hasFailed && attempts < maxAttempts;

  return (
    <div style={{
      padding: '40px 10%',
      backgroundColor: '#0f172a',
      minHeight: '100vh',
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center'
    }}>
      <Card
        style={{
          background: 'rgba(30,41,59,0.8)',
          border: 'none',
          textAlign: 'center',
          minWidth: 400
        }}
      >
        {polling ? (
          <>
            <Spin indicator={<LoadingOutlined style={{ fontSize: 48, color: '#38bdf8' }} spin />} />
            <h2 style={{ color: 'white', marginTop: 24 }}>Đang tạo liên kết thanh toán...</h2>
            <p style={{ color: '#94a3b8' }}>Vui lòng chờ trong giây lát</p>
            <p style={{ color: '#64748b', fontSize: '0.9rem' }}>
              Đang kiểm tra ({attempts}/{maxAttempts})
            </p>
          </>
        ) : (
          <>
            <h2 style={{ color: '#ef4444' }}>Không thể tạo liên kết thanh toán</h2>
            <p style={{ color: '#94a3b8', marginBottom: 24 }}>
              Booking đã được tạo nhưng Payment.API chưa sẵn sàng. Vui lòng kiểm tra lại sau.
            </p>
            <Button
              type="primary"
              size="large"
              onClick={() => navigate('/')}
              style={{ background: '#38bdf8' }}
            >
              Quay về trang chủ
            </Button>
          </>
        )}
      </Card>
    </div>
  );
};

export default BookingStatusPage;
