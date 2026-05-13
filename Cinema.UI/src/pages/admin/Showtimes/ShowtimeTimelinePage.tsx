import React, { useState, useMemo } from 'react';
import {
  Card, Select, DatePicker, Row, Col, Tooltip, Tag, Progress, Spin, Empty,
  Typography, Space, Button, Modal, Descriptions,
} from 'antd';
import {
  CalendarOutlined, VideoCameraOutlined,
  CheckCircleOutlined, LockOutlined,
  ReloadOutlined, InfoCircleOutlined,
} from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { cinemaApi } from '../../../features/cinemas/api/cinemaApi';
import {
  showtimeTimelineApi,
  type TimelineShowtimeDto,
  type TimelineRoomDto,
} from '../../../features/showtimes/api/showtimeTimelineApi';
import dayjs from '../../../utils/dayjs';
import { toLocalDateTime } from '../../../utils/dateTime';

const { Title, Text } = Typography;

// ---- Constants ----
const PIXELS_PER_MINUTE = 2.5;
const TIMELINE_START_HOUR = 8;  // 08:00
const TIMELINE_END_HOUR = 26;   // 02:00 next day
const TOTAL_MINUTES = (TIMELINE_END_HOUR - TIMELINE_START_HOUR) * 60;
const TIMELINE_WIDTH = TOTAL_MINUTES * PIXELS_PER_MINUTE;
const ROOM_LABEL_WIDTH = 140;

// ---- Helpers ----
const getOccupancyColor = (rate: number) => {
  if (rate >= 80) return '#f5222d';
  if (rate >= 50) return '#faad14';
  return '#52c41a';
};

const minutesSinceStart = (isoTime: string, baseDate: dayjs.Dayjs): number => {
  const t = toLocalDateTime(isoTime);
  const base = baseDate.hour(TIMELINE_START_HOUR).minute(0).second(0);
  return t.diff(base, 'minute');
};

// ---- Showtime Block Component ----
const ShowtimeBlock: React.FC<{
  showtime: TimelineShowtimeDto;
  baseDate: dayjs.Dayjs;
  onSelect: (s: TimelineShowtimeDto) => void;
}> = ({ showtime, baseDate, onSelect }) => {
  const leftMin = minutesSinceStart(showtime.start, baseDate);
  const widthMin = showtime.durationMinutes;
  const cleaningMin = showtime.cleaningBufferMinutes;
  const left = Math.max(0, leftMin * PIXELS_PER_MINUTE);
  const width = widthMin * PIXELS_PER_MINUTE;
  const cleaningWidth = cleaningMin * PIXELS_PER_MINUTE;

  const bgColor = showtime.hasBookings
    ? 'linear-gradient(135deg, #1677ff 0%, #4096ff 100%)'
    : 'linear-gradient(135deg, #91caff 0%, #bae0ff 100%)';
  const textColor = showtime.hasBookings ? '#fff' : '#003a8c';

  return (
    <>
      {/* Main block */}
      <Tooltip
        title={
          <div style={{ fontSize: 12 }}>
            <div><b>{showtime.movieTitle}</b></div>
            <div>{toLocalDateTime(showtime.start).format('HH:mm')} – {toLocalDateTime(showtime.end).format('HH:mm')}</div>
            <div>Ghế: {showtime.bookedSeats}/{showtime.totalSeats} ({showtime.occupancyRate.toFixed(0)}%)</div>
            <div>Giá: {showtime.price.toLocaleString()}đ</div>
            {showtime.hasBookings && <div style={{ color: '#ffc53d' }}>🔒 Đã có vé bán</div>}
            {!showtime.canReschedule && <div style={{ color: '#ff4d4f' }}>Không thể dời lịch</div>}
          </div>
        }
      >
        <div
          onClick={() => onSelect(showtime)}
          style={{
            position: 'absolute',
            left, top: 4,
            width: Math.max(width, 30),
            height: 52,
            background: bgColor,
            borderRadius: 6,
            padding: '4px 8px',
            cursor: 'pointer',
            overflow: 'hidden',
            border: '1px solid rgba(0,0,0,0.06)',
            boxShadow: '0 1px 4px rgba(0,0,0,0.1)',
            transition: 'transform 0.15s, box-shadow 0.15s',
            zIndex: 2,
          }}
          onMouseEnter={(e) => {
            (e.currentTarget as HTMLDivElement).style.transform = 'scale(1.03)';
            (e.currentTarget as HTMLDivElement).style.boxShadow = '0 4px 12px rgba(0,0,0,0.2)';
          }}
          onMouseLeave={(e) => {
            (e.currentTarget as HTMLDivElement).style.transform = 'scale(1)';
            (e.currentTarget as HTMLDivElement).style.boxShadow = '0 1px 4px rgba(0,0,0,0.1)';
          }}
        >
          <div style={{ color: textColor, fontSize: 12, fontWeight: 600, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
            {showtime.movieTitle}
          </div>
          <div style={{ color: textColor, fontSize: 10, opacity: 0.85 }}>
            {toLocalDateTime(showtime.start).format('HH:mm')}–{toLocalDateTime(showtime.end).format('HH:mm')}
          </div>
          {/* Occupancy bar */}
          <div style={{
            position: 'absolute', bottom: 0, left: 0, right: 0, height: 4,
            background: 'rgba(0,0,0,0.15)', borderRadius: '0 0 6px 6px',
          }}>
            <div style={{
              height: '100%',
              width: `${showtime.occupancyRate}%`,
              background: getOccupancyColor(showtime.occupancyRate),
              borderRadius: '0 0 6px 6px',
            }} />
          </div>
          {showtime.hasBookings && (
            <LockOutlined style={{ position: 'absolute', top: 4, right: 6, color: textColor, fontSize: 10, opacity: 0.7 }} />
          )}
        </div>
      </Tooltip>

      {/* Cleaning buffer */}
      {cleaningMin > 0 && (
        <Tooltip title={`Dọn dẹp: ${cleaningMin} phút`}>
          <div style={{
            position: 'absolute',
            left: left + width, top: 4,
            width: cleaningWidth, height: 52,
            background: 'repeating-linear-gradient(45deg, transparent, transparent 4px, rgba(0,0,0,0.06) 4px, rgba(0,0,0,0.06) 8px)',
            borderRadius: '0 6px 6px 0',
            border: '1px dashed #d9d9d9',
            borderLeft: 'none',
            zIndex: 1,
          }} />
        </Tooltip>
      )}
    </>
  );
};

// ---- Room Row Component ----
const RoomRow: React.FC<{
  room: TimelineRoomDto;
  baseDate: dayjs.Dayjs;
  onSelectShowtime: (s: TimelineShowtimeDto) => void;
}> = ({ room, baseDate, onSelectShowtime }) => (
  <div style={{ display: 'flex', borderBottom: '1px solid #f0f0f0' }}>
    {/* Room label */}
    <div style={{
      width: ROOM_LABEL_WIDTH, minWidth: ROOM_LABEL_WIDTH,
      padding: '12px 16px', background: '#fafafa',
      borderRight: '1px solid #f0f0f0',
      display: 'flex', flexDirection: 'column', justifyContent: 'center',
    }}>
      <Text strong style={{ fontSize: 13 }}>{room.roomName}</Text>
      <Text type="secondary" style={{ fontSize: 11 }}>{room.totalSeats} ghế</Text>
    </div>
    {/* Timeline area */}
    <div style={{ position: 'relative', width: TIMELINE_WIDTH, height: 60, flexShrink: 0 }}>
      {room.showtimes.map((st) => (
        <ShowtimeBlock key={st.id} showtime={st} baseDate={baseDate} onSelect={onSelectShowtime} />
      ))}
    </div>
  </div>
);

// ---- Time Header ----
const TimeHeader: React.FC = () => {
  const hours = [];
  for (let h = TIMELINE_START_HOUR; h < TIMELINE_END_HOUR; h++) {
    const displayHour = h >= 24 ? h - 24 : h;
    hours.push(
      <div key={h} style={{
        width: 60 * PIXELS_PER_MINUTE, flexShrink: 0,
        textAlign: 'left', paddingLeft: 4,
        fontSize: 11, color: '#8c8c8c', fontWeight: 500,
        borderLeft: '1px solid #f0f0f0',
      }}>
        {String(displayHour).padStart(2, '0')}:00
      </div>
    );
  }
  return (
    <div style={{ display: 'flex', marginLeft: ROOM_LABEL_WIDTH, borderBottom: '2px solid #e8e8e8', paddingBottom: 4 }}>
      {hours}
    </div>
  );
};

// ---- Main Page ----
const ShowtimeTimelinePage: React.FC = () => {
  const [selectedCinemaId, setSelectedCinemaId] = useState<string | null>(null);
  const [selectedDate, setSelectedDate] = useState(dayjs());
  const [detailShowtime, setDetailShowtime] = useState<TimelineShowtimeDto | null>(null);

  // Fetch cinemas
  const { data: cinemasRes, isLoading: loadingCinemas } = useQuery({
    queryKey: ['cinemas-timeline'],
    queryFn: () => cinemaApi.getCinemas(1, 100),
  });
  const cinemas = cinemasRes?.data?.items || [];

  // Fetch timeline
  const { data: timelineRes, isLoading: loadingTimeline, refetch } = useQuery({
    queryKey: ['showtime-timeline', selectedCinemaId, selectedDate.format('YYYY-MM-DD')],
    queryFn: () => showtimeTimelineApi.getTimeline(selectedCinemaId!, selectedDate.format('YYYY-MM-DD')),
    enabled: !!selectedCinemaId,
  });
  const timeline = timelineRes?.data;

  // Stats
  const stats = useMemo(() => {
    if (!timeline?.rooms) return { totalShowtimes: 0, avgOccupancy: 0, conflictCount: 0 };
    const allShowtimes = timeline.rooms.flatMap(r => r.showtimes);
    const total = allShowtimes.length;
    const avg = total > 0 ? allShowtimes.reduce((s, st) => s + st.occupancyRate, 0) / total : 0;
    return { totalShowtimes: total, avgOccupancy: avg, conflictCount: 0 };
  }, [timeline]);

  return (
    <div>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 24 }}>
        <div>
          <Title level={3} style={{ margin: 0 }}>📅 Timeline Lịch Chiếu</Title>
          <Text type="secondary">Quản lý suất chiếu theo trục thời gian — phát hiện xung đột lịch nhanh chóng</Text>
        </div>
        <Button icon={<ReloadOutlined />} onClick={() => refetch()} disabled={!selectedCinemaId}>Làm mới</Button>
      </div>

      {/* Filters */}
      <Card style={{ marginBottom: 24, borderRadius: 12 }}>
        <Row gutter={16} align="middle">
          <Col>
            <Space>
              <VideoCameraOutlined style={{ color: '#1677ff' }} />
              <Text strong>Cụm rạp:</Text>
              <Select
                placeholder="Chọn cụm rạp"
                style={{ width: 280 }}
                loading={loadingCinemas}
                value={selectedCinemaId}
                onChange={setSelectedCinemaId}
                options={cinemas.map(c => ({ label: c.name, value: c.id }))}
              />
            </Space>
          </Col>
          <Col>
            <Space>
              <CalendarOutlined style={{ color: '#1677ff' }} />
              <Text strong>Ngày:</Text>
              <DatePicker
                value={selectedDate}
                onChange={(d) => d && setSelectedDate(d)}
                format="DD/MM/YYYY"
                allowClear={false}
              />
            </Space>
          </Col>
          {timeline && (
            <Col flex="auto" style={{ textAlign: 'right' }}>
              <Space size={24}>
                <span><Tag color="blue">{stats.totalShowtimes}</Tag> suất chiếu</span>
                <span>Lấp đầy TB: <Tag color={getOccupancyColor(stats.avgOccupancy)}>{stats.avgOccupancy.toFixed(0)}%</Tag></span>
                <span>Buffer: <Tag>{timeline.cleaningBufferMinutes}p</Tag></span>
              </Space>
            </Col>
          )}
        </Row>
      </Card>

      {/* Legend */}
      <div style={{ marginBottom: 16, display: 'flex', gap: 20, flexWrap: 'wrap' }}>
        <Space><div style={{ width: 20, height: 12, background: 'linear-gradient(135deg, #1677ff, #4096ff)', borderRadius: 3 }} /><Text type="secondary" style={{ fontSize: 12 }}>Đã mở bán</Text></Space>
        <Space><div style={{ width: 20, height: 12, background: 'linear-gradient(135deg, #91caff, #bae0ff)', borderRadius: 3 }} /><Text type="secondary" style={{ fontSize: 12 }}>Chưa có vé</Text></Space>
        <Space><div style={{ width: 20, height: 12, background: 'repeating-linear-gradient(45deg, #fff, #fff 3px, #f0f0f0 3px, #f0f0f0 6px)', borderRadius: 3, border: '1px dashed #d9d9d9' }} /><Text type="secondary" style={{ fontSize: 12 }}>Dọn dẹp</Text></Space>
        <Space><div style={{ width: 6, height: 6, borderRadius: '50%', background: '#52c41a' }} /><Text type="secondary" style={{ fontSize: 12 }}>Còn nhiều</Text></Space>
        <Space><div style={{ width: 6, height: 6, borderRadius: '50%', background: '#faad14' }} /><Text type="secondary" style={{ fontSize: 12 }}>Sắp hết</Text></Space>
        <Space><div style={{ width: 6, height: 6, borderRadius: '50%', background: '#f5222d' }} /><Text type="secondary" style={{ fontSize: 12 }}>Cháy vé</Text></Space>
      </div>

      {/* Timeline Grid */}
      {!selectedCinemaId ? (
        <Card style={{ borderRadius: 12, textAlign: 'center', padding: 60 }}>
          <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Chọn cụm rạp để xem timeline" />
        </Card>
      ) : loadingTimeline ? (
        <Card style={{ borderRadius: 12, textAlign: 'center', padding: 60 }}><Spin size="large" /></Card>
      ) : !timeline || timeline.rooms.length === 0 ? (
        <Card style={{ borderRadius: 12, textAlign: 'center', padding: 60 }}>
          <Empty description="Không có suất chiếu nào trong ngày này" />
        </Card>
      ) : (
        <Card style={{ borderRadius: 12, overflow: 'hidden', padding: 0 }} styles={{ body: { padding: 0 } }}>
          <div style={{ overflowX: 'auto', overflowY: 'hidden' }}>
            <div style={{ minWidth: TIMELINE_WIDTH + ROOM_LABEL_WIDTH }}>
              <TimeHeader />
              {timeline.rooms.map((room) => (
                <RoomRow key={room.roomId} room={room} baseDate={selectedDate} onSelectShowtime={setDetailShowtime} />
              ))}
            </div>
          </div>
        </Card>
      )}

      {/* Detail Modal */}
      <Modal
        title={<Space><InfoCircleOutlined />Chi tiết suất chiếu</Space>}
        open={!!detailShowtime}
        onCancel={() => setDetailShowtime(null)}
        footer={null}
        width={550}
      >
        {detailShowtime && (
          <div>
            <div style={{
              padding: '12px 16px', borderRadius: 8, marginBottom: 16,
              background: detailShowtime.hasBookings ? '#e6f4ff' : '#f6ffed',
              border: `1px solid ${detailShowtime.hasBookings ? '#91caff' : '#b7eb8f'}`,
            }}>
              <Space>
                {detailShowtime.hasBookings
                  ? <Tag color="blue" icon={<LockOutlined />}>Đã có vé bán</Tag>
                  : <Tag color="success" icon={<CheckCircleOutlined />}>Có thể dời lịch</Tag>
                }
                <Text strong style={{ fontSize: 16 }}>{detailShowtime.movieTitle}</Text>
              </Space>
            </div>

            <Descriptions bordered column={2} size="small">
              <Descriptions.Item label="Giờ chiếu">{toLocalDateTime(detailShowtime.start).format('HH:mm')}</Descriptions.Item>
              <Descriptions.Item label="Kết thúc">{toLocalDateTime(detailShowtime.end).format('HH:mm')}</Descriptions.Item>
              <Descriptions.Item label="Thời lượng">{detailShowtime.durationMinutes} phút</Descriptions.Item>
              <Descriptions.Item label="Buffer dọn dẹp">{detailShowtime.cleaningBufferMinutes} phút</Descriptions.Item>
              <Descriptions.Item label="Giá vé">{detailShowtime.price.toLocaleString()} đ</Descriptions.Item>
              <Descriptions.Item label="Lấp đầy">
                <Progress
                  percent={detailShowtime.occupancyRate}
                  size="small"
                  strokeColor={getOccupancyColor(detailShowtime.occupancyRate)}
                  format={() => `${detailShowtime.bookedSeats}/${detailShowtime.totalSeats}`}
                />
              </Descriptions.Item>
            </Descriptions>
          </div>
        )}
      </Modal>
    </div>
  );
};

export default ShowtimeTimelinePage;
