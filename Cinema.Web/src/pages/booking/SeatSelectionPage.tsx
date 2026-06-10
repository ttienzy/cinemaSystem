import { ArrowLeftOutlined, ShoppingCartOutlined } from '@ant-design/icons';
import { Alert, App, Badge, Button, Empty, Skeleton, Space, Statistic, Tag, Typography } from 'antd';
import { useMutation, useQuery } from '@tanstack/react-query';
import dayjs from 'dayjs';
import { useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { bookingApi, type SeatStatusDto } from '../../features/booking/bookingApi';
import { useBookingStore } from '../../features/booking/bookingStore';

const { Title, Text } = Typography;

const seatStatusLabel: Record<number, string> = {
  0: 'Available',
  1: 'Locked',
  2: 'Booked',
  3: 'Unavailable',
};

function getSeatClass(seat: SeatStatusDto | undefined, selected: boolean): string {
  if (!seat) return 'seat-cell seat-empty';
  if (selected) return 'seat-cell seat-selected';
  if (seat.status === 0) return 'seat-cell seat-available';
  if (seat.status === 1) return 'seat-cell seat-locked';
  if (seat.status === 2) return 'seat-cell seat-booked';
  return 'seat-cell seat-unavailable';
}

export default function SeatSelectionPage() {
  const { showtimeId } = useParams();
  const navigate = useNavigate();
  const { message } = App.useApp();
  const setBookingSession = useBookingStore((state) => state.setBookingSession);
  const [selectedSeatIds, setSelectedSeatIds] = useState<string[]>([]);

  const availabilityQuery = useQuery({
    queryKey: ['seat-availability', showtimeId],
    queryFn: () => bookingApi.getSeatAvailability(showtimeId!),
    enabled: Boolean(showtimeId),
    refetchInterval: 10000,
  });

  const availability = availabilityQuery.data?.data;
  const seats = availability?.seats ?? [];

  const rows = useMemo(() => {
    return Array.from(new Set(seats.map((seat) => seat.row))).sort((a, b) => a.localeCompare(b));
  }, [seats]);

  const maxNumber = useMemo(() => Math.max(0, ...seats.map((seat) => seat.number)), [seats]);

  const selectedSeats = useMemo(
    () => seats.filter((seat) => selectedSeatIds.includes(seat.seatId)),
    [seats, selectedSeatIds],
  );

  const total = selectedSeats.reduce((sum, seat) => sum + seat.price, 0);

  const lockMutation = useMutation({
    mutationFn: () =>
      bookingApi.lockSeats(showtimeId!, {
        showtimeId: showtimeId!,
        seatIds: selectedSeatIds,
      }),
    onSuccess: (response) => {
      const lockedUntil = response.data.lockedUntil ?? dayjs().add(10, 'minute').toISOString();
      setBookingSession(showtimeId!, selectedSeats, lockedUntil);
      message.success('Seats locked. Please complete checkout.');
      navigate('/checkout');
    },
  });

  const toggleSeat = (seat: SeatStatusDto | undefined) => {
    if (!seat || seat.status !== 0) return;

    setSelectedSeatIds((current) => {
      if (current.includes(seat.seatId)) {
        return current.filter((id) => id !== seat.seatId);
      }

      if (current.length >= 10) {
        message.warning('You can select up to 10 seats per booking.');
        return current;
      }

      return [...current, seat.seatId];
    });
  };

  const handleContinue = () => {
    if (!showtimeId || selectedSeatIds.length === 0) {
      message.warning('Please select at least one seat.');
      return;
    }

    lockMutation.mutate();
  };

  if (availabilityQuery.isLoading) {
    return (
      <main className="page-shell">
        <Skeleton active paragraph={{ rows: 8 }} />
      </main>
    );
  }

  if (!availability) {
    return (
      <main className="page-shell">
        <Empty description="Seat map is not available" />
      </main>
    );
  }

  return (
    <main className="page-shell booking-shell">
      <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(-1)}>
        Back
      </Button>

      <section className="booking-header">
        <div>
          <Title level={2}>Choose seats</Title>
          <Text type="secondary">{availability.cinemaHallName}</Text>
        </div>
        <Space wrap>
          <Tag color="green">{availability.summary.availableSeats} available</Tag>
          <Tag color="orange">{availability.summary.lockedSeats} locked</Tag>
          <Tag color="red">{availability.summary.bookedSeats} booked</Tag>
        </Space>
      </section>

      <Alert
        type="info"
        showIcon
        message="Selected seats will be locked temporarily when you continue to checkout."
      />

      <section className="screen-marker">Screen</section>

      <section className="seat-map" style={{ gridTemplateColumns: `48px repeat(${maxNumber}, 42px)` }}>
        {rows.map((row) => (
          <div className="seat-row" key={row} style={{ display: 'contents' }}>
            <div className="seat-row-label">{row}</div>
            {Array.from({ length: maxNumber }, (_, index) => {
              const number = index + 1;
              const seat = seats.find((item) => item.row === row && item.number === number);
              const selected = Boolean(seat && selectedSeatIds.includes(seat.seatId));

              return (
                <button
                  className={getSeatClass(seat, selected)}
                  disabled={!seat || seat.status !== 0}
                  key={`${row}-${number}`}
                  onClick={() => toggleSeat(seat)}
                  title={seat ? `${row}${number} - ${seatStatusLabel[seat.status]}` : ''}
                  type="button"
                >
                  {seat ? number : ''}
                </button>
              );
            })}
          </div>
        ))}
      </section>

      <Space className="seat-legend" wrap>
        <Badge color="#22c55e" text="Available" />
        <Badge color="#1677ff" text="Selected" />
        <Badge color="#f59e0b" text="Locked" />
        <Badge color="#ef4444" text="Booked" />
      </Space>

      <aside className="checkout-strip">
        <div>
          <Text type="secondary">Selected</Text>
          <Title level={4}>{selectedSeats.map((seat) => `${seat.row}${seat.number}`).join(', ') || 'None'}</Title>
        </div>
        <Statistic title="Total" value={total} suffix="VND" />
        <Button
          icon={<ShoppingCartOutlined />}
          loading={lockMutation.isPending}
          onClick={handleContinue}
          size="large"
          type="primary"
        >
          Continue
        </Button>
      </aside>
    </main>
  );
}
