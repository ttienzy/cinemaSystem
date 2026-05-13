import React, { useMemo, useState } from 'react';
import {
  Button,
  Card,
  Col,
  DatePicker,
  Descriptions,
  Empty,
  Modal,
  Progress,
  Row,
  Select,
  Space,
  Spin,
  Tag,
  Tooltip,
  Typography,
} from 'antd';
import {
  CalendarOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  InfoCircleOutlined,
  LockOutlined,
  ReloadOutlined,
  VideoCameraOutlined,
} from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { cinemaApi } from '../../../features/cinemas/api/cinemaApi';
import {
  showtimeTimelineApi,
  type TimelineRoomDto,
  type TimelineShowtimeDto,
} from '../../../features/showtimes/api/showtimeTimelineApi';
import dayjs from '../../../utils/dayjs';
import { toLocalDateTime } from '../../../utils/dateTime';

const { Title, Text } = Typography;

const PIXELS_PER_MINUTE = 2.4;
const ROOM_LABEL_WIDTH = 240;
const DEFAULT_START_HOUR = 8;
const DEFAULT_END_HOUR_NEXT_DAY = 2;
const SHOWTIME_HEIGHT = 60;
const ROOM_ROW_HEIGHT = 84;

type TimelineConfig = {
  start: dayjs.Dayjs;
  end: dayjs.Dayjs;
  totalMinutes: number;
  width: number;
  slots: Array<{
    key: string;
    label: string;
    left: number;
    width: number;
    isDayBoundary: boolean;
  }>;
};

const clamp = (value: number, min: number, max: number) => Math.min(Math.max(value, min), max);

const formatCurrency = (value: number) => `${value.toLocaleString('vi-VN')}đ`;

const formatTime = (value: string) => toLocalDateTime(value).format('HH:mm');

const getOccupancyColor = (rate: number) => {
  if (rate >= 80) return '#cf1322';
  if (rate >= 55) return '#d48806';
  return '#389e0d';
};

const getOccupancyLabel = (rate: number) => {
  if (rate >= 80) return 'Cháy vé';
  if (rate >= 55) return 'Bán tốt';
  return 'Còn nhiều';
};

const buildTimelineConfig = (
  timelineStart: string | undefined,
  timelineEnd: string | undefined,
  selectedDate: dayjs.Dayjs,
): TimelineConfig => {
  const fallbackStart = selectedDate.startOf('day').hour(DEFAULT_START_HOUR).minute(0).second(0).millisecond(0);
  const fallbackEnd = selectedDate
    .add(1, 'day')
    .startOf('day')
    .hour(DEFAULT_END_HOUR_NEXT_DAY)
    .minute(0)
    .second(0)
    .millisecond(0);

  const start = timelineStart ? toLocalDateTime(timelineStart) : fallbackStart;
  const end = timelineEnd ? toLocalDateTime(timelineEnd) : fallbackEnd;
  const normalizedEnd = end.isAfter(start) ? end : fallbackEnd;
  const totalMinutes = Math.max(normalizedEnd.diff(start, 'minute'), 60);
  const width = Math.max(totalMinutes * PIXELS_PER_MINUTE, 1080);

  const slots: TimelineConfig['slots'] = [];
  let cursor = start.startOf('hour');

  if (cursor.isAfter(start)) {
    cursor = cursor.subtract(1, 'hour');
  }

  while (cursor.isBefore(normalizedEnd)) {
    const next = cursor.add(1, 'hour');
    const segmentStart = cursor.isBefore(start) ? start : cursor;
    const segmentEnd = next.isAfter(normalizedEnd) ? normalizedEnd : next;
    const left = segmentStart.diff(start, 'minute') * PIXELS_PER_MINUTE;
    const segmentWidth = Math.max(segmentEnd.diff(segmentStart, 'minute') * PIXELS_PER_MINUTE, 0);

    if (segmentWidth > 0) {
      slots.push({
        key: cursor.toISOString(),
        label: cursor.format('HH:mm'),
        left,
        width: segmentWidth,
        isDayBoundary: cursor.hour() === 0,
      });
    }

    cursor = next;
  }

  return {
    start,
    end: normalizedEnd,
    totalMinutes,
    width,
    slots,
  };
};

const getRoomMetrics = (room: TimelineRoomDto) => {
  const totalShowtimes = room.showtimes.length;
  const bookedShowtimes = room.showtimes.filter((showtime) => showtime.hasBookings).length;
  const averageOccupancy = totalShowtimes
    ? room.showtimes.reduce((sum, showtime) => sum + showtime.occupancyRate, 0) / totalShowtimes
    : 0;

  return { totalShowtimes, bookedShowtimes, averageOccupancy };
};

const TimelineSummaryCard: React.FC<{
  label: string;
  value: string;
  accent: string;
  helper: string;
}> = ({ label, value, accent, helper }) => (
  <div
    style={{
      minWidth: 180,
      padding: '14px 16px',
      borderRadius: 14,
      border: `1px solid ${accent}`,
      background: `linear-gradient(180deg, ${accent}14 0%, #ffffff 72%)`,
    }}
  >
    <Text type="secondary" style={{ fontSize: 12 }}>
      {label}
    </Text>
    <div style={{ marginTop: 6, fontSize: 24, fontWeight: 700, color: '#1f1f1f', lineHeight: 1.1 }}>{value}</div>
    <Text type="secondary" style={{ fontSize: 12 }}>
      {helper}
    </Text>
  </div>
);

const ShowtimeBlock: React.FC<{
  showtime: TimelineShowtimeDto;
  timelineConfig: TimelineConfig;
  onSelect: (showtime: TimelineShowtimeDto) => void;
}> = ({ showtime, timelineConfig, onSelect }) => {
  const start = toLocalDateTime(showtime.start);
  const end = toLocalDateTime(showtime.end);
  const startMinutes = start.diff(timelineConfig.start, 'minute');
  const endMinutes = toLocalDateTime(showtime.cleaningEnd).diff(timelineConfig.start, 'minute');
  const visibleStartMinutes = clamp(startMinutes, 0, timelineConfig.totalMinutes);
  const visibleEndMinutes = clamp(end.diff(timelineConfig.start, 'minute'), 0, timelineConfig.totalMinutes);
  const visibleCleaningMinutes = clamp(endMinutes, 0, timelineConfig.totalMinutes);
  const left = visibleStartMinutes * PIXELS_PER_MINUTE;
  const width = Math.max((visibleEndMinutes - visibleStartMinutes) * PIXELS_PER_MINUTE, 88);
  const cleaningWidth = Math.max((visibleCleaningMinutes - visibleEndMinutes) * PIXELS_PER_MINUTE, 0);
  const occupancyColor = getOccupancyColor(showtime.occupancyRate);
  const occupancyLabel = getOccupancyLabel(showtime.occupancyRate);
  const hasWideSpace = width >= 180;
  const hasMediumSpace = width >= 130;
  const blockBackground = showtime.hasBookings
    ? 'linear-gradient(135deg, #0f5ed7 0%, #2878f0 100%)'
    : 'linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%)';
  const textColor = showtime.hasBookings ? '#ffffff' : '#102a43';
  const subTextColor = showtime.hasBookings ? 'rgba(255,255,255,0.82)' : '#486581';

  return (
    <>
      <Tooltip
        title={
          <div style={{ minWidth: 220 }}>
            <div style={{ fontWeight: 700, marginBottom: 6 }}>{showtime.movieTitle}</div>
            <div>{formatTime(showtime.start)} - {formatTime(showtime.end)}</div>
            <div>Thời lượng: {showtime.durationMinutes} phút</div>
            <div>Dọn dẹp: {showtime.cleaningBufferMinutes} phút</div>
            <div>Lấp đầy: {showtime.bookedSeats}/{showtime.totalSeats} ({showtime.occupancyRate.toFixed(0)}%)</div>
            <div>Giá vé: {formatCurrency(showtime.price)}</div>
            <div>{showtime.hasBookings ? 'Đã mở bán vé' : 'Chưa phát sinh vé'}</div>
            {!showtime.canReschedule && <div style={{ color: '#ffccc7' }}>Không thể dời lịch</div>}
          </div>
        }
      >
        <button
          type="button"
          onClick={() => onSelect(showtime)}
          style={{
            position: 'absolute',
            left,
            top: 12,
            width,
            height: SHOWTIME_HEIGHT,
            padding: '10px 12px 12px',
            borderRadius: 14,
            border: `1px solid ${showtime.hasBookings ? 'rgba(255,255,255,0.22)' : '#bfdbfe'}`,
            background: blockBackground,
            boxShadow: showtime.hasBookings
              ? '0 10px 24px rgba(15, 94, 215, 0.22)'
              : '0 8px 18px rgba(37, 99, 235, 0.12)',
            color: textColor,
            cursor: 'pointer',
            textAlign: 'left',
            overflow: 'hidden',
            zIndex: 2,
          }}
        >
          <div
            style={{
              display: 'flex',
              alignItems: 'flex-start',
              justifyContent: 'space-between',
              gap: 8,
              marginBottom: 6,
            }}
          >
            <div style={{ minWidth: 0, flex: 1 }}>
              <div
                style={{
                  fontSize: 13,
                  fontWeight: 700,
                  whiteSpace: 'nowrap',
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                }}
              >
                {showtime.movieTitle}
              </div>
              <div style={{ fontSize: 11, color: subTextColor }}>
                {formatTime(showtime.start)} - {formatTime(showtime.end)}
              </div>
            </div>
            {showtime.hasBookings ? (
              <LockOutlined style={{ fontSize: 12, color: '#ffffff', flexShrink: 0, marginTop: 2 }} />
            ) : (
              <CheckCircleOutlined style={{ fontSize: 12, color: '#1677ff', flexShrink: 0, marginTop: 2 }} />
            )}
          </div>

          {hasWideSpace && (
            <div
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                gap: 10,
                fontSize: 11,
              }}
            >
              <span style={{ color: subTextColor }}>
                {showtime.bookedSeats}/{showtime.totalSeats} ghế
              </span>
              <span
                style={{
                  padding: '2px 8px',
                  borderRadius: 999,
                  background: showtime.hasBookings ? 'rgba(255,255,255,0.18)' : '#ffffff',
                  color: showtime.hasBookings ? '#ffffff' : occupancyColor,
                  fontWeight: 700,
                }}
              >
                {occupancyLabel}
              </span>
            </div>
          )}

          {!hasWideSpace && hasMediumSpace && (
            <div style={{ fontSize: 11, color: subTextColor }}>{showtime.occupancyRate.toFixed(0)}% lấp đầy</div>
          )}

          <div
            style={{
              position: 'absolute',
              left: 0,
              right: 0,
              bottom: 0,
              height: 5,
              background: showtime.hasBookings ? 'rgba(255,255,255,0.16)' : '#dbeafe',
            }}
          >
            <div
              style={{
                width: `${Math.min(showtime.occupancyRate, 100)}%`,
                height: '100%',
                background: showtime.hasBookings ? '#ffd666' : occupancyColor,
              }}
            />
          </div>
        </button>
      </Tooltip>

      {cleaningWidth > 0 && (
        <Tooltip title={`Buffer dọn dẹp: ${showtime.cleaningBufferMinutes} phút`}>
          <div
            style={{
              position: 'absolute',
              left: left + width,
              top: 12,
              width: cleaningWidth,
              height: SHOWTIME_HEIGHT,
              borderRadius: '0 14px 14px 0',
              border: '1px dashed #cbd5e1',
              borderLeft: 'none',
              background:
                'repeating-linear-gradient(135deg, rgba(148,163,184,0.08), rgba(148,163,184,0.08) 6px, rgba(148,163,184,0.2) 6px, rgba(148,163,184,0.2) 12px)',
              zIndex: 1,
            }}
          />
        </Tooltip>
      )}
    </>
  );
};

const TimeHeader: React.FC<{ timelineConfig: TimelineConfig }> = ({ timelineConfig }) => (
  <div
    style={{
      display: 'flex',
      position: 'sticky',
      top: 0,
      zIndex: 4,
      background: '#f8fafc',
      borderBottom: '1px solid #dbe3ef',
    }}
  >
    <div
      style={{
        width: ROOM_LABEL_WIDTH,
        minWidth: ROOM_LABEL_WIDTH,
        position: 'sticky',
        left: 0,
        zIndex: 5,
        padding: '14px 16px',
        borderRight: '1px solid #dbe3ef',
        background: 'linear-gradient(180deg, #f8fafc 0%, #eef4fb 100%)',
      }}
    >
      <Text type="secondary" style={{ fontSize: 12 }}>
        Phòng chiếu
      </Text>
      <div style={{ fontSize: 14, fontWeight: 700, color: '#1f1f1f' }}>Trục thời gian vận hành</div>
    </div>

    <div style={{ position: 'relative', width: timelineConfig.width, height: 62, flexShrink: 0 }}>
      {timelineConfig.slots.map((slot) => (
        <div
          key={slot.key}
          style={{
            position: 'absolute',
            top: 0,
            left: slot.left,
            width: slot.width,
            height: '100%',
            padding: '10px 12px 0',
            borderLeft: `1px solid ${slot.isDayBoundary ? '#91caff' : '#e5e7eb'}`,
            background: slot.isDayBoundary ? 'rgba(22, 119, 255, 0.04)' : undefined,
          }}
        >
          <div style={{ fontSize: 12, fontWeight: 700, color: '#1f2937' }}>{slot.label}</div>
          <div style={{ fontSize: 11, color: '#8c8c8c' }}>{slot.isDayBoundary ? 'Qua ngày' : 'Trong ngày'}</div>
        </div>
      ))}
    </div>
  </div>
);

const RoomRow: React.FC<{
  room: TimelineRoomDto;
  timelineConfig: TimelineConfig;
  onSelectShowtime: (showtime: TimelineShowtimeDto) => void;
}> = ({ room, timelineConfig, onSelectShowtime }) => {
  const metrics = getRoomMetrics(room);

  return (
    <div style={{ display: 'flex', borderBottom: '1px solid #eef2f7' }}>
      <div
        style={{
          width: ROOM_LABEL_WIDTH,
          minWidth: ROOM_LABEL_WIDTH,
          position: 'sticky',
          left: 0,
          zIndex: 3,
          padding: '14px 16px',
          borderRight: '1px solid #eef2f7',
          background: '#fcfdff',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 10 }}>
          <div>
            <div style={{ fontSize: 14, fontWeight: 700, color: '#1f1f1f' }}>{room.roomName}</div>
            <Text type="secondary" style={{ fontSize: 12 }}>
              {room.totalSeats} ghế
            </Text>
          </div>
          <Tag color="blue" style={{ marginInlineEnd: 0 }}>
            {metrics.totalShowtimes} suất
          </Tag>
        </div>

        <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap', marginTop: 12 }}>
          <Tag color="cyan" style={{ marginInlineEnd: 0 }}>
            {metrics.bookedShowtimes} đã mở bán
          </Tag>
          <Tag color={getOccupancyColor(metrics.averageOccupancy)} style={{ marginInlineEnd: 0 }}>
            TB {metrics.averageOccupancy.toFixed(0)}%
          </Tag>
        </div>
      </div>

      <div style={{ position: 'relative', width: timelineConfig.width, height: ROOM_ROW_HEIGHT, flexShrink: 0 }}>
        {timelineConfig.slots.map((slot) => (
          <div
            key={slot.key}
            style={{
              position: 'absolute',
              top: 0,
              left: slot.left,
              width: slot.width,
              height: '100%',
              borderLeft: `1px solid ${slot.isDayBoundary ? '#d6e4ff' : '#f1f5f9'}`,
              background: slot.isDayBoundary ? 'rgba(22, 119, 255, 0.025)' : undefined,
            }}
          />
        ))}

        <div
          style={{
            position: 'absolute',
            top: SHOWTIME_HEIGHT + 14,
            left: 0,
            right: 0,
            height: 1,
            background: '#edf2f7',
          }}
        />

        {room.showtimes.map((showtime) => (
          <ShowtimeBlock
            key={showtime.id}
            showtime={showtime}
            timelineConfig={timelineConfig}
            onSelect={onSelectShowtime}
          />
        ))}
      </div>
    </div>
  );
};

const ShowtimeTimelinePage: React.FC = () => {
  const [selectedCinemaId, setSelectedCinemaId] = useState<string | null>(null);
  const [selectedDate, setSelectedDate] = useState(dayjs());
  const [detailShowtime, setDetailShowtime] = useState<TimelineShowtimeDto | null>(null);

  const { data: cinemasRes, isLoading: loadingCinemas } = useQuery({
    queryKey: ['cinemas-timeline'],
    queryFn: () => cinemaApi.getCinemas(1, 100),
  });
  const cinemas = cinemasRes?.data?.items || [];

  const { data: timelineRes, isLoading: loadingTimeline, refetch } = useQuery({
    queryKey: ['showtime-timeline', selectedCinemaId, selectedDate.format('YYYY-MM-DD')],
    queryFn: () => showtimeTimelineApi.getTimeline(selectedCinemaId!, selectedDate.format('YYYY-MM-DD')),
    enabled: !!selectedCinemaId,
  });
  const timeline = timelineRes?.data;

  const stats = useMemo(() => {
    if (!timeline?.rooms?.length) {
      return {
        totalRooms: 0,
        totalShowtimes: 0,
        avgOccupancy: 0,
        activeRooms: 0,
      };
    }

    const allShowtimes = timeline.rooms.flatMap((room) => room.showtimes);
    const totalShowtimes = allShowtimes.length;
    const avgOccupancy = totalShowtimes
      ? allShowtimes.reduce((sum, showtime) => sum + showtime.occupancyRate, 0) / totalShowtimes
      : 0;

    return {
      totalRooms: timeline.rooms.length,
      totalShowtimes,
      avgOccupancy,
      activeRooms: timeline.rooms.filter((room) => room.showtimes.length > 0).length,
    };
  }, [timeline]);

  const timelineConfig = useMemo(
    () => buildTimelineConfig(timeline?.timelineStart, timeline?.timelineEnd, selectedDate),
    [selectedDate, timeline?.timelineEnd, timeline?.timelineStart],
  );

  return (
    <div>
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'flex-start',
          gap: 16,
          marginBottom: 24,
        }}
      >
        <div>
          <Title level={3} style={{ margin: 0 }}>
            Timeline Lịch Chiếu
          </Title>
          <Text type="secondary">
            Theo dõi toàn bộ suất chiếu theo từng phòng, nhìn nhanh công suất và vùng buffer dọn dẹp.
          </Text>
        </div>
        <Button
          icon={<ReloadOutlined />}
          onClick={() => refetch()}
          disabled={!selectedCinemaId}
          style={{ borderRadius: 10 }}
        >
          Làm mới
        </Button>
      </div>

      <Card
        style={{
          marginBottom: 20,
          borderRadius: 18,
          border: '1px solid #e6eef8',
          boxShadow: '0 10px 28px rgba(15, 23, 42, 0.04)',
        }}
      >
        <Row gutter={[16, 16]} align="middle">
          <Col xs={24} xl={14}>
            <Row gutter={[12, 12]}>
              <Col xs={24} md={12}>
                <Text strong style={{ display: 'block', marginBottom: 8 }}>
                  <Space size={6}>
                    <VideoCameraOutlined style={{ color: '#1677ff' }} />
                    <span>Cụm rạp</span>
                  </Space>
                </Text>
                <Select
                  placeholder="Chọn cụm rạp"
                  style={{ width: '100%' }}
                  loading={loadingCinemas}
                  value={selectedCinemaId}
                  onChange={setSelectedCinemaId}
                  options={cinemas.map((cinema) => ({ label: cinema.name, value: cinema.id }))}
                />
              </Col>
              <Col xs={24} md={12}>
                <Text strong style={{ display: 'block', marginBottom: 8 }}>
                  <Space size={6}>
                    <CalendarOutlined style={{ color: '#1677ff' }} />
                    <span>Ngày vận hành</span>
                  </Space>
                </Text>
                <DatePicker
                  value={selectedDate}
                  onChange={(value) => value && setSelectedDate(value)}
                  format="DD/MM/YYYY"
                  allowClear={false}
                  style={{ width: '100%' }}
                />
              </Col>
            </Row>
          </Col>

          <Col xs={24} xl={10}>
            <div
              style={{
                display: 'flex',
                justifyContent: 'flex-end',
                gap: 12,
                flexWrap: 'wrap',
              }}
            >
              <TimelineSummaryCard
                label="Khung timeline"
                value={`${timelineConfig.start.format('HH:mm')} - ${timelineConfig.end.format('HH:mm')}`}
                accent="#91caff"
                helper={timelineConfig.end.isAfter(timelineConfig.start, 'day') ? 'Bao gồm cả suất sau 00:00' : 'Khung trong ngày'}
              />
              <TimelineSummaryCard
                label="Suất chiếu"
                value={`${stats.totalShowtimes}`}
                accent="#b7eb8f"
                helper={`${stats.activeRooms}/${stats.totalRooms || 0} phòng đang hoạt động`}
              />
              <TimelineSummaryCard
                label="Lấp đầy trung bình"
                value={`${stats.avgOccupancy.toFixed(0)}%`}
                accent="#ffe58f"
                helper={`Buffer dọn dẹp: ${timeline?.cleaningBufferMinutes ?? 0} phút`}
              />
            </div>
          </Col>
        </Row>
      </Card>

      <Card
        style={{
          marginBottom: 20,
          borderRadius: 16,
          border: '1px solid #eef2f7',
          background: 'linear-gradient(180deg, #ffffff 0%, #fbfdff 100%)',
        }}
      >
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 18 }}>
          <Space size={8}>
            <div
              style={{
                width: 18,
                height: 12,
                borderRadius: 4,
                background: 'linear-gradient(135deg, #0f5ed7 0%, #2878f0 100%)',
              }}
            />
            <Text type="secondary" style={{ fontSize: 12 }}>
              Suất đã mở bán
            </Text>
          </Space>
          <Space size={8}>
            <div
              style={{
                width: 18,
                height: 12,
                borderRadius: 4,
                background: 'linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%)',
                border: '1px solid #bfdbfe',
              }}
            />
            <Text type="secondary" style={{ fontSize: 12 }}>
              Suất chưa phát sinh vé
            </Text>
          </Space>
          <Space size={8}>
            <div
              style={{
                width: 18,
                height: 12,
                borderRadius: 4,
                border: '1px dashed #cbd5e1',
                background:
                  'repeating-linear-gradient(135deg, rgba(148,163,184,0.08), rgba(148,163,184,0.08) 6px, rgba(148,163,184,0.2) 6px, rgba(148,163,184,0.2) 12px)',
              }}
            />
            <Text type="secondary" style={{ fontSize: 12 }}>
              Buffer dọn dẹp
            </Text>
          </Space>
          <Space size={8}>
            <div style={{ width: 8, height: 8, borderRadius: '50%', background: '#389e0d' }} />
            <Text type="secondary" style={{ fontSize: 12 }}>
              Công suất thấp
            </Text>
          </Space>
          <Space size={8}>
            <div style={{ width: 8, height: 8, borderRadius: '50%', background: '#d48806' }} />
            <Text type="secondary" style={{ fontSize: 12 }}>
              Bán tốt
            </Text>
          </Space>
          <Space size={8}>
            <div style={{ width: 8, height: 8, borderRadius: '50%', background: '#cf1322' }} />
            <Text type="secondary" style={{ fontSize: 12 }}>
              Gần hết vé
            </Text>
          </Space>
        </div>
      </Card>

      {!selectedCinemaId ? (
        <Card style={{ borderRadius: 16, textAlign: 'center', padding: 60 }}>
          <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Chọn cụm rạp để xem timeline vận hành." />
        </Card>
      ) : loadingTimeline ? (
        <Card style={{ borderRadius: 16, textAlign: 'center', padding: 60 }}>
          <Spin size="large" />
        </Card>
      ) : !timeline || timeline.rooms.length === 0 ? (
        <Card style={{ borderRadius: 16, textAlign: 'center', padding: 60 }}>
          <Empty description="Không có suất chiếu nào trong ngày đã chọn." />
        </Card>
      ) : (
        <Card
          style={{
            borderRadius: 18,
            overflow: 'hidden',
            border: '1px solid #dbe3ef',
            boxShadow: '0 10px 28px rgba(15, 23, 42, 0.04)',
          }}
          styles={{ body: { padding: 0 } }}
        >
          <div style={{ overflowX: 'auto', overflowY: 'hidden', background: '#ffffff' }}>
            <div style={{ minWidth: ROOM_LABEL_WIDTH + timelineConfig.width }}>
              <TimeHeader timelineConfig={timelineConfig} />
              {timeline.rooms.map((room) => (
                <RoomRow
                  key={room.roomId}
                  room={room}
                  timelineConfig={timelineConfig}
                  onSelectShowtime={setDetailShowtime}
                />
              ))}
            </div>
          </div>
        </Card>
      )}

      <Modal
        title={
          <Space>
            <InfoCircleOutlined />
            Chi tiết suất chiếu
          </Space>
        }
        open={!!detailShowtime}
        onCancel={() => setDetailShowtime(null)}
        footer={null}
        width={640}
      >
        {detailShowtime && (
          <div>
            <div
              style={{
                padding: '14px 16px',
                borderRadius: 12,
                marginBottom: 16,
                background: detailShowtime.hasBookings ? '#e6f4ff' : '#f6ffed',
                border: `1px solid ${detailShowtime.hasBookings ? '#91caff' : '#b7eb8f'}`,
              }}
            >
              <div
                style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  gap: 12,
                  flexWrap: 'wrap',
                }}
              >
                <div>
                  <Text strong style={{ fontSize: 16, display: 'block' }}>
                    {detailShowtime.movieTitle}
                  </Text>
                  <Text type="secondary">
                    {formatTime(detailShowtime.start)} - {formatTime(detailShowtime.end)}
                  </Text>
                </div>
                <Space wrap>
                  {detailShowtime.hasBookings ? (
                    <Tag color="blue" icon={<LockOutlined />}>
                      Đã mở bán
                    </Tag>
                  ) : (
                    <Tag color="success" icon={<CheckCircleOutlined />}>
                      Chưa có vé
                    </Tag>
                  )}
                  <Tag color={getOccupancyColor(detailShowtime.occupancyRate)}>
                    {getOccupancyLabel(detailShowtime.occupancyRate)}
                  </Tag>
                </Space>
              </div>
            </div>

            <Descriptions bordered column={2} size="small">
              <Descriptions.Item label="Giờ chiếu">{formatTime(detailShowtime.start)}</Descriptions.Item>
              <Descriptions.Item label="Kết thúc">{formatTime(detailShowtime.end)}</Descriptions.Item>
              <Descriptions.Item label="Thời lượng">{detailShowtime.durationMinutes} phút</Descriptions.Item>
              <Descriptions.Item label="Dọn dẹp">{detailShowtime.cleaningBufferMinutes} phút</Descriptions.Item>
              <Descriptions.Item label="Giá vé">{formatCurrency(detailShowtime.price)}</Descriptions.Item>
              <Descriptions.Item label="Lấp đầy">
                <Progress
                  percent={Math.round(detailShowtime.occupancyRate)}
                  size="small"
                  strokeColor={getOccupancyColor(detailShowtime.occupancyRate)}
                  format={() => `${detailShowtime.bookedSeats}/${detailShowtime.totalSeats}`}
                />
              </Descriptions.Item>
              <Descriptions.Item label="Buffer kết thúc">
                <Space size={6}>
                  <ClockCircleOutlined />
                  <span>{formatTime(detailShowtime.cleaningEnd)}</span>
                </Space>
              </Descriptions.Item>
              <Descriptions.Item label="Điều chỉnh lịch">
                {detailShowtime.canReschedule ? 'Có thể dời lịch' : 'Đang khóa do có ràng buộc bán vé'}
              </Descriptions.Item>
            </Descriptions>
          </div>
        )}
      </Modal>
    </div>
  );
};

export default ShowtimeTimelinePage;
