import { Button, Descriptions, Result, Skeleton, Space } from 'antd';
import { useQuery } from '@tanstack/react-query';
import dayjs from 'dayjs';
import { useNavigate, useParams } from 'react-router-dom';
import { bookingApi } from '../../features/booking/bookingApi';

export default function BookingSuccessPage() {
  const { bookingId } = useParams();
  const navigate = useNavigate();

  const bookingQuery = useQuery({
    queryKey: ['booking-success', bookingId],
    queryFn: () => bookingApi.getBookingById(bookingId!),
    enabled: Boolean(bookingId),
  });

  if (bookingQuery.isLoading) {
    return (
      <main className="page-shell">
        <Skeleton active paragraph={{ rows: 5 }} />
      </main>
    );
  }

  const booking = bookingQuery.data?.data;

  return (
    <main className="page-shell">
      <Result
        extra={
          <Space>
            <Button onClick={() => navigate('/movies')}>Book more tickets</Button>
            <Button onClick={() => navigate('/')} type="primary">
              Back home
            </Button>
          </Space>
        }
        status="success"
        subTitle="Please keep this booking information for check-in."
        title="Booking confirmed"
      />

      {booking ? (
        <section className="payment-status-panel">
          <Descriptions bordered column={1}>
            <Descriptions.Item label="Booking ID">{booking.bookingId}</Descriptions.Item>
            <Descriptions.Item label="Movie">
              {booking.showtimeDetails?.movieTitle ?? 'Movie details updating'}
            </Descriptions.Item>
            <Descriptions.Item label="Cinema">
              {booking.showtimeDetails?.cinemaName ?? 'Cinema details updating'}
            </Descriptions.Item>
            <Descriptions.Item label="Showtime">
              {booking.showtimeDetails?.startTime
                ? dayjs(booking.showtimeDetails.startTime).format('DD/MM/YYYY HH:mm')
                : 'Updating'}
            </Descriptions.Item>
            <Descriptions.Item label="Seats">
              {booking.seats.map((seat) => `${seat.row}${seat.number}`).join(', ')}
            </Descriptions.Item>
            <Descriptions.Item label="Total">{booking.totalPrice.toLocaleString()} VND</Descriptions.Item>
          </Descriptions>
        </section>
      ) : null}
    </main>
  );
}
