import { ArrowLeftOutlined, CreditCardOutlined } from '@ant-design/icons';
import { Alert, App, Button, Form, Input, Result, Space, Statistic, Typography } from 'antd';
import { useMutation } from '@tanstack/react-query';
import dayjs from 'dayjs';
import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { bookingApi } from '../../features/booking/bookingApi';
import { useBookingStore } from '../../features/booking/bookingStore';
import { useAuthStore } from '../../features/auth/authStore';

const { Title, Text } = Typography;
const { Countdown } = Statistic;

interface CheckoutFormValues {
  contactName: string;
  contactEmail: string;
  contactPhone: string;
}

export default function CheckoutPage() {
  const navigate = useNavigate();
  const { message } = App.useApp();
  const user = useAuthStore((state) => state.user);
  const hydrateBookingSession = useBookingStore((state) => state.hydrateBookingSession);
  const clearBookingSession = useBookingStore((state) => state.clearBookingSession);
  const { showtimeId, selectedSeats, lockedUntil } = useBookingStore();
  const [expired, setExpired] = useState(false);

  useEffect(() => {
    hydrateBookingSession();
  }, [hydrateBookingSession]);

  useEffect(() => {
    if (lockedUntil && dayjs(lockedUntil).isBefore(dayjs())) {
      setExpired(true);
    }
  }, [lockedUntil]);

  const total = useMemo(() => selectedSeats.reduce((sum, seat) => sum + seat.price, 0), [selectedSeats]);

  const createBookingMutation = useMutation({
    mutationFn: (values: CheckoutFormValues) =>
      bookingApi.createBooking({
        showtimeId: showtimeId!,
        seatIds: selectedSeats.map((seat) => seat.seatId),
        ...values,
      }),
    onSuccess: (response) => {
      clearBookingSession();
      message.success('Booking created. Preparing payment checkout.');
      navigate(`/booking-status/${response.data.bookingId}`);
    },
  });

  if (!showtimeId || selectedSeats.length === 0 || !lockedUntil) {
    return (
      <main className="page-shell">
        <Result
          extra={
            <Button onClick={() => navigate('/movies')} type="primary">
              Browse movies
            </Button>
          }
          status="warning"
          subTitle="Please choose seats before checkout."
          title="No active booking session"
        />
      </main>
    );
  }

  if (expired) {
    return (
      <main className="page-shell">
        <Result
          extra={
            <Button onClick={() => navigate(`/booking/${showtimeId}`)} type="primary">
              Choose seats again
            </Button>
          }
          status="warning"
          subTitle="The temporary seat lock has expired."
          title="Session expired"
        />
      </main>
    );
  }

  return (
    <main className="page-shell checkout-page">
      <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(`/booking/${showtimeId}`)}>
        Back to seats
      </Button>

      <section className="checkout-layout">
        <div className="checkout-form-panel">
          <Title level={2}>Checkout</Title>
          <Alert
            message="Complete your booking before the seat lock expires."
            showIcon
            type="info"
          />

          <Form
            initialValues={{
              contactName: user?.fullName ?? '',
              contactEmail: user?.email ?? '',
              contactPhone: '',
            }}
            layout="vertical"
            onFinish={(values) => createBookingMutation.mutate(values)}
          >
            <Form.Item
              label="Full name"
              name="contactName"
              rules={[{ message: 'Full name is required', required: true }]}
            >
              <Input placeholder="Nguyen Van A" />
            </Form.Item>
            <Form.Item
              label="Email"
              name="contactEmail"
              rules={[
                { message: 'Email is required', required: true },
                { message: 'Email is invalid', type: 'email' },
              ]}
            >
              <Input placeholder="customer@example.com" />
            </Form.Item>
            <Form.Item
              label="Phone"
              name="contactPhone"
              rules={[{ message: 'Phone is required', required: true }]}
            >
              <Input placeholder="0900000000" />
            </Form.Item>

            <Button
              block
              htmlType="submit"
              icon={<CreditCardOutlined />}
              loading={createBookingMutation.isPending}
              size="large"
              type="primary"
            >
              Create booking and pay
            </Button>
          </Form>
        </div>

        <aside className="checkout-summary">
          <Countdown
            format="mm:ss"
            onFinish={() => setExpired(true)}
            title="Seat lock expires in"
            value={dayjs(lockedUntil).valueOf()}
          />
          <div className="summary-line">
            <Text type="secondary">Seats</Text>
            <Text strong>{selectedSeats.map((seat) => `${seat.row}${seat.number}`).join(', ')}</Text>
          </div>
          <div className="summary-line">
            <Text type="secondary">Quantity</Text>
            <Text>{selectedSeats.length}</Text>
          </div>
          <Space className="summary-total" direction="vertical" size={0}>
            <Text type="secondary">Total</Text>
            <Title level={3}>{total.toLocaleString()} VND</Title>
          </Space>
        </aside>
      </section>
    </main>
  );
}
