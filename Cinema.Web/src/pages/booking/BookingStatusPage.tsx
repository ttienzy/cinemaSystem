import { LoadingOutlined } from '@ant-design/icons';
import { Alert, Button, Result, Skeleton, Space, Steps, Typography } from 'antd';
import { useQuery } from '@tanstack/react-query';
import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { bookingApi } from '../../features/booking/bookingApi';
import { useBookingStore } from '../../features/booking/bookingStore';
import { getApiGatewayBaseUrl } from '../../shared/utils/apiConfig';

const { Text, Title } = Typography;

export default function BookingStatusPage() {
  const { bookingId } = useParams();
  const navigate = useNavigate();
  const clearBookingSession = useBookingStore((state) => state.clearBookingSession);

  useEffect(() => {
    clearBookingSession();
  }, [clearBookingSession]);

  const bookingQuery = useQuery({
    queryKey: ['booking', bookingId],
    queryFn: () => bookingApi.getBookingById(bookingId!),
    enabled: Boolean(bookingId),
  });

  const paymentQuery = useQuery({
    queryKey: ['payment-by-booking', bookingId],
    queryFn: () => bookingApi.getPaymentByBookingId(bookingId!),
    enabled: Boolean(bookingId),
    refetchInterval: (query) => (query.state.data ? false : 1200),
  });

  useEffect(() => {
    if (!paymentQuery.data?.id) return;

    window.location.href = `${getApiGatewayBaseUrl()}/api/v1/payments/${paymentQuery.data.id}/checkout`;
  }, [paymentQuery.data]);

  if (bookingQuery.isLoading) {
    return (
      <main className="page-shell">
        <Skeleton active paragraph={{ rows: 6 }} />
      </main>
    );
  }

  if (!bookingQuery.data?.data) {
    return (
      <main className="page-shell">
        <Result
          extra={<Button onClick={() => navigate('/movies')}>Browse movies</Button>}
          status="warning"
          title="Booking not found"
        />
      </main>
    );
  }

  const booking = bookingQuery.data.data;
  const seats = booking.seats?.map((seat) => `${seat.row}${seat.number}`).join(', ');

  return (
    <main className="page-shell payment-wait-page">
      <Result
        extra={
          <Space>
            <Button onClick={() => paymentQuery.refetch()}>Check again</Button>
            <Button onClick={() => navigate('/')} type="primary">
              Home
            </Button>
          </Space>
        }
        icon={<LoadingOutlined />}
        subTitle="Payment service is creating your checkout session. You will be redirected automatically."
        title="Preparing payment"
      />

      <section className="payment-status-panel">
        <Steps
          current={paymentQuery.data?.id ? 2 : 1}
          items={[
            { title: 'Seats locked' },
            { title: 'Booking created' },
            { title: 'Payment checkout' },
          ]}
        />

        <Alert
          message="If the redirect does not happen after a few seconds, use Check again."
          showIcon
          type="info"
        />

        <div className="summary-line">
          <Text type="secondary">Booking</Text>
          <Text copyable strong>
            {booking.bookingId}
          </Text>
        </div>
        <div className="summary-line">
          <Text type="secondary">Seats</Text>
          <Text>{seats || 'No seat details'}</Text>
        </div>
        <div className="summary-line">
          <Text type="secondary">Total</Text>
          <Title level={4}>{booking.totalPrice.toLocaleString()} VND</Title>
        </div>
      </section>
    </main>
  );
}
