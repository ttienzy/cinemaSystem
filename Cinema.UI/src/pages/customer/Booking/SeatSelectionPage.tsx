import React, { useState, useMemo, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { Spin, Button, message, Tag, Badge } from 'antd';
import { UserOutlined, WifiOutlined, DisconnectOutlined } from '@ant-design/icons';
import { useAuthStore } from '../../../store/useAuthStore';
import { bookingApi, type SeatStatusDto } from '../../../features/booking/api/bookingApi';
import { useBookingStore } from '../../../features/booking/store/useBookingStore';
import { useSeatRealtime } from '../../../hooks/useSeatRealtime';
import type { SeatStatusChangedNotification } from '../../../services/signalr/seatSignalRService';
import { getApiGatewayBaseUrl } from '../../../utils/apiConfig';
import { getAccessToken as getStoredAccessToken } from '../../../utils/tokenStorage';
import dayjs from '../../../utils/dayjs';

const SeatSelectionPage: React.FC = () => {
  const { user: currentUser } = useAuthStore();
  const { showtimeId } = useParams<{ showtimeId: string }>();
  const navigate = useNavigate();
  const setBookingSession = useBookingStore(state => state.setBookingSession);

  const [selectedSeats, setSelectedSeats] = useState<SeatStatusDto[]>([]);
  const [seatMap, setSeatMap] = useState<Map<string, SeatStatusDto>>(new Map());

  // Fetch initial seat availability
  const { data: res, isLoading } = useQuery({
    queryKey: ['seat-availability', showtimeId],
    queryFn: () => bookingApi.getSeatAvailability(showtimeId!),
    enabled: !!showtimeId,
    refetchInterval: false, // Disable polling - use SignalR instead
  });

  // Initialize seat map when data loads
  React.useEffect(() => {
    if (res?.data?.seats) {
      const map = new Map<string, SeatStatusDto>();
      res.data.seats.forEach(seat => {
        map.set(seat.seatId, seat);
      });
      setSeatMap(map);
    }
  }, [res?.data?.seats]);

  // Update seat status in real-time
  const updateSeatStatus = useCallback((seatIds: string[], status: 0 | 1 | 2, userId?: string, lockedUntil?: string) => {
    setSeatMap(prevMap => {
      const newMap = new Map(prevMap);
      seatIds.forEach(seatId => {
        const seat = newMap.get(seatId);
        if (seat) {
          newMap.set(seatId, {
            ...seat,
            status,
            lockedBy: userId,
            lockedUntil: lockedUntil, // Keep as string, don't convert to Date
          });
        }
      });
      return newMap;
    });

    // Remove from selected seats if they become unavailable
    if (status !== 0) {
      setSelectedSeats(prev => prev.filter(s => !seatIds.includes(s.seatId)));
    }
  }, []);

  // Stable getAccessToken function
  const getSignalRAccessToken = useCallback(() => {
    return getStoredAccessToken();
  }, []);

  // Stable SignalR callbacks
  const handleSeatLocked = useCallback((notification: SeatStatusChangedNotification) => {
    console.log('🔒 Seat locked:', notification);
    updateSeatStatus(
      notification.SeatIds,
      1, // Locked
      notification.UserId,
      notification.LockedUntil
    );

    // Show notification if it's not current user
    const currentUserId = currentUser?.id;
    if (notification.UserId !== currentUserId) {
      message.info(`Ghế ${notification.SeatIds.length} vừa được chọn bởi người khác`, 2);
    }
  }, [updateSeatStatus, currentUser?.id]);

  const handleSeatUnlocked = useCallback((notification: SeatStatusChangedNotification) => {
    console.log('🔓 Seat unlocked:', notification);
    updateSeatStatus(notification.SeatIds, 0); // Available
  }, [updateSeatStatus]);

  const handleSeatBooked = useCallback((notification: SeatStatusChangedNotification) => {
    console.log('✅ Seat booked:', notification);
    updateSeatStatus(notification.SeatIds, 2); // 2 = Booked
  }, [updateSeatStatus]);

  const handleSeatReleased = useCallback((notification: SeatStatusChangedNotification) => {
    console.log('🔄 Seat released:', notification);
    updateSeatStatus(notification.SeatIds, 0); // 0 = Available
    message.success(`${notification.SeatIds.length} ghế vừa được giải phóng!`, 2);
  }, [updateSeatStatus]);

  const handleViewerCountUpdated = useCallback((notification: ViewerCountNotification) => {
    console.log('👥 Viewer count notification received:', notification);
    console.log('👥 Viewer count value:', notification.ViewerCount);
    console.log('👥 Showtime ID:', notification.ShowtimeId);
  }, []);

  const handleConnectionError = useCallback((error: Error) => {
    console.error('SignalR connection error:', error);
    message.error('Mất kết nối real-time. Đang thử kết nối lại...', 3);
  }, []);

  // SignalR real-time connection
  const { isConnected, isConnecting, viewerCount, error: signalRError, reconnect } = useSeatRealtime({
    showtimeId: showtimeId!,
    apiBaseUrl: getApiGatewayBaseUrl(),
    getAccessToken: getSignalRAccessToken,
    enabled: !!showtimeId,
    onSeatLocked: handleSeatLocked,
    onSeatUnlocked: handleSeatUnlocked,
    onSeatBooked: handleSeatBooked,
    onSeatReleased: handleSeatReleased,
    onViewerCountUpdated: handleViewerCountUpdated,
    onConnectionError: handleConnectionError,
  });

  const lockMutation = useMutation({
    mutationFn: (data: { showtimeId: string, seatIds: string[] }) =>
      bookingApi.lockSeats(data.showtimeId, {
        showtimeId: data.showtimeId,
        seatIds: data.seatIds
      }),
    onSuccess: (response) => {
      if (response.success && response.data) {
        const lockedUntil = response.data.lockedUntil || dayjs().add(10, 'minute').toISOString();
        setBookingSession(showtimeId!, selectedSeats, lockedUntil);
        message.success('Đã giữ ghế thành công! Vui lòng thanh toán.');
        navigate('/checkout');
      } else {
        message.error(response.message || 'Không thể giữ ghế. Có thể ai đó đã chọn trước.');
      }
    },
    onError: () => message.error('Lỗi khi kết nối với máy chủ đặt vé.')
  });

  const toggleSeat = (seat: SeatStatusDto) => {
    // Không cho phép tương tác với ghế đã bán hoặc đang bị hold
    if (seat.status !== 0) {
      if (seat.status === 1) {
        message.warning('Ghế này đang được giữ bởi người khác!');
      } else if (seat.status === 2) {
        message.warning('Ghế này đã được đặt!');
      }
      return;
    }

    // Giới hạn tối đa 8 ghế
    if (!selectedSeats.find(s => s.seatId === seat.seatId) && selectedSeats.length >= 8) {
      message.warning('Bạn chỉ được chọn tối đa 8 ghế!');
      return;
    }

    setSelectedSeats(prev => {
      const isSelected = prev.find(s => s.seatId === seat.seatId);
      if (isSelected) {
        return prev.filter(s => s.seatId !== seat.seatId);
      }
      return [...prev, seat];
    });
  };

  const handleContinue = () => {
    if (selectedSeats.length === 0) {
      message.warning('Vui lòng chọn ít nhất 1 ghế');
      return;
    }

    // Check if any selected seats are no longer available
    const unavailableSeats = selectedSeats.filter(seat => {
      const currentSeat = seatMap.get(seat.seatId);
      return currentSeat?.status !== 0;
    });

    if (unavailableSeats.length > 0) {
      message.error('Một số ghế bạn chọn đã không còn khả dụng. Vui lòng chọn lại!');
      setSelectedSeats(prev => prev.filter(s => !unavailableSeats.includes(s)));
      return;
    }

    // Lock seats - userId will be extracted from JWT token by backend
    lockMutation.mutate({
      showtimeId: showtimeId!,
      seatIds: selectedSeats.map(s => s.seatId)
    });
  };

  // Xây dựng ma trận ghế bằng useMemo để tối ưu render
  const { rows, maxCol, seatList } = useMemo(() => {
    const list = Array.from(seatMap.values());
    const rowSet = Array.from(new Set(list.map(s => s.row))).sort();
    const maxColumn = list.length > 0 ? Math.max(...list.map(s => s.number)) : 0;
    return { rows: rowSet, maxCol: maxColumn, seatList: list };
  }, [seatMap]);

  if (isLoading) return <div className="global-spinner"><Spin size="large" /></div>;
  if (!res?.data) return <div>Không tìm thấy dữ liệu phòng chiếu</div>;

  const { cinemaHallName } = res.data;

  const getSeatColor = (seat: SeatStatusDto, isSelected: boolean) => {
    if (isSelected) return '#38bdf8'; // Đang chọn -> Màu xanh Neon
    if (seat.status === 2) return '#ef4444'; // Đã bán -> Màu đỏ
    if (seat.status === 1) return '#f59e0b'; // Đang bị hold -> Màu cam
    if (seat.status === 3) return 'transparent'; // Unavailable

    // Available (status === 0)
    if (seat.seatType === 'VIP') return 'gold';
    if (seat.seatType === 'Couple') return 'pink';
    return '#475569'; // Normal
  };

  const basePrice = res?.data?.seats?.[0]?.price || 0;
  const totalPrice = selectedSeats.length * basePrice;

  return (
    <div className="booking-container" style={{ padding: '40px 10%', backgroundColor: '#0f172a', color: 'white', minHeight: '100vh' }}>
      {/* Header with connection status */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <h2 style={{ color: '#38bdf8', margin: 0 }}>CHỌN GHẾ</h2>

        <div style={{ display: 'flex', gap: 16, alignItems: 'center' }}>
          {/* Viewer count */}
          <Badge
            count={viewerCount}
            showZero
            style={{ backgroundColor: '#38bdf8' }}
            title={`${viewerCount} người đang xem`}
          >
            <UserOutlined style={{ fontSize: 20, color: '#94a3b8' }} />
          </Badge>
          <span style={{ color: '#94a3b8', fontSize: 14 }}>
            {viewerCount} {viewerCount === 1 ? 'người' : 'người'} đang xem
          </span>

          {/* Connection status */}
          {isConnecting && (
            <Tag icon={<Spin size="small" />} color="processing">
              Đang kết nối...
            </Tag>
          )}
          {isConnected && (
            <Tag icon={<WifiOutlined />} color="success">
              Real-time
            </Tag>
          )}
          {signalRError && !isConnecting && (
            <Tag
              icon={<DisconnectOutlined />}
              color="error"
              style={{ cursor: 'pointer' }}
              onClick={reconnect}
            >
              Mất kết nối (Click để kết nối lại)
            </Tag>
          )}
        </div>
      </div>

      <p style={{ textAlign: 'center', color: '#94a3b8', marginBottom: 32 }}>
        Phòng chiếu: {cinemaHallName}
        {isConnected && <span style={{ color: '#10b981', marginLeft: 8 }}>● Cập nhật tự động</span>}
      </p>

      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', overflowX: 'auto' }}>
        <div style={{
          width: '80%', height: 20, background: 'linear-gradient(to bottom, #38bdf8, transparent)',
          borderRadius: '50% 50% 0 0', textAlign: 'center',
          color: 'white', marginBottom: 40, fontWeight: 'bold'
        }}>
          MÀN HÌNH
        </div>

        <div style={{
          display: 'grid',
          gridTemplateColumns: `30px repeat(${maxCol}, 32px) 30px`,
          gap: '8px',
          justifyContent: 'center',
          alignItems: 'center'
        }}>
          {rows.map(row => {
            const rowSeats = seatList.filter(s => s.row === row).sort((a, b) => a.number - b.number);
            return (
              <React.Fragment key={row}>
                <div style={{ fontWeight: 'bold', textAlign: 'center' }}>{row}</div>

                {/* Lặp từ 1 đến maxCol để rải ghế vào đúng tọa độ lưới */}
                {Array.from({ length: maxCol }).map((_, idx) => {
                  const colNumber = idx + 1;
                  const seat = rowSeats.find(s => s.number === colNumber);

                  if (!seat || seat.status === 3) {
                    return <div key={`empty-${row}-${colNumber}`} />; // Khoảng trống lối đi
                  }

                  const isSelected = !!selectedSeats.find(s => s.seatId === seat.seatId);
                  const isCouple = seat.seatType === 'Couple';

                  return (
                    <div
                      key={seat.seatId}
                      onClick={() => toggleSeat(seat)}
                      style={{
                        gridColumn: isCouple ? 'span 2' : 'span 1',
                        height: 32,
                        backgroundColor: getSeatColor(seat, isSelected),
                        borderRadius: isCouple ? 8 : 4,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        cursor: seat.status === 0 ? 'pointer' : 'not-allowed',
                        fontSize: 12,
                        fontWeight: 'bold',
                        opacity: seat.status === 0 ? 1 : 0.6,
                        boxShadow: isSelected ? '0 0 10px #38bdf8' : 'none',
                        transition: 'all 0.3s ease', // Smooth transition for real-time updates
                      }}
                    >
                      {seat.number}
                    </div>
                  );
                })}

                <div style={{ fontWeight: 'bold', textAlign: 'center' }}>{row}</div>
              </React.Fragment>
            );
          })}
        </div>
      </div>

      <div style={{ marginTop: 32, textAlign: 'center', display: 'flex', justifyContent: 'center', gap: 16 }}>
        <Tag color="#475569">Ghế Thường</Tag>
        <Tag color="gold">Ghế VIP</Tag>
        <Tag color="pink">Ghế Couple</Tag>
        <Tag color="#ef4444">Đã bán</Tag>
        <Tag color="#f59e0b">Đang giữ</Tag>
        <Tag color="#38bdf8">Đang chọn</Tag>
      </div>

      <div style={{
        marginTop: 40, padding: 24, background: 'rgba(30,41,59,0.8)',
        borderRadius: 16, display: 'flex', justifyContent: 'space-between', alignItems: 'center'
      }}>
        <div>
          <h3 style={{ margin: 0, color: 'white' }}>
            Ghế đã chọn: {selectedSeats.length > 0 ? selectedSeats.map(s => `${s.row}${s.number}`).join(', ') : 'Chưa chọn'}
          </h3>
          <p style={{ margin: '8px 0 0 0', fontSize: '1.2rem', color: '#38bdf8', fontWeight: 'bold' }}>
            Tổng tiền: {totalPrice.toLocaleString()} VNĐ
          </p>
        </div>
        <Button
          type="primary"
          size="large"
          onClick={handleContinue}
          disabled={selectedSeats.length === 0}
          loading={lockMutation.isPending}
          style={{ background: '#38bdf8', color: '#0f172a', fontWeight: 'bold' }}
        >
          Tiếp tục Thanh toán
        </Button>
      </div>
    </div>
  );
};

export default SeatSelectionPage;
