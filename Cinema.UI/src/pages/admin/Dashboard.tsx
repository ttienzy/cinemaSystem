// ============================================================
// Admin Dashboard - Trung tâm Chỉ huy (Command Center)
// ============================================================
// Trang tổng quan giúp Admin nắm bắt "sức khỏe" của hệ thống.
// Sử dụng getSummary 1 lần khi load.
// Polling getKpiSnapshot mỗi 60s để update số liệu nhanh.
// Kết nối SignalR để nhận real-time hoạt động gần đây (NewBooking).
// ============================================================

import React, { useState, useEffect, useMemo, useRef } from 'react';
import {
  Card,
  Row,
  Col,
  Statistic,
  Table,
  Tag,
  Avatar,
  Timeline,
  Typography,
  Space,
  Badge,
  Progress,
  Tooltip,
  Spin,
} from 'antd';
import {
  DollarOutlined,
  ShoppingCartOutlined,
  BarChartOutlined,
  VideoCameraOutlined,
  RiseOutlined,
  FallOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  FireOutlined,
} from '@ant-design/icons';
import { Line, Pie } from '@ant-design/charts';
import { useQuery } from '@tanstack/react-query';
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import dayjs from '../../utils/dayjs';
import { toLocalDateTime } from '../../utils/dateTime';
import { dashboardApi, type NewBookingPayload, type RecentActivity, type TopMovie } from '../../api/dashboardApi';
import { useAuth } from '../../hooks/useAuth';
import { getApiGatewayBaseUrl } from '../../utils/apiConfig';
import { getAccessToken } from '../../utils/tokenStorage';

const { Title, Text } = Typography;

const asNumber = (value: unknown, fallback = 0) => {
  return typeof value === 'number' && Number.isFinite(value) ? value : fallback;
};

const formatMoney = (value: unknown) => `${asNumber(value).toLocaleString('vi-VN')} đ`;

const normalizeRecentActivity = (activity: NewBookingPayload): RecentActivity => {
  return {
    bookingId: activity.bookingId ?? activity.BookingId ?? crypto.randomUUID(),
    showtimeId: activity.showtimeId ?? activity.ShowtimeId ?? '',
    movieId: activity.movieId ?? activity.MovieId ?? '',
    movieTitle: activity.movieTitle ?? activity.MovieTitle ?? 'Không rõ phim',
    customerName: activity.customerName ?? activity.CustomerName ?? 'Khách hàng',
    amount: asNumber(activity.amount ?? activity.Amount),
    seatsCount: asNumber(activity.seatsCount ?? activity.SeatsCount),
    status: activity.status ?? activity.Status ?? 'Completed',
    occurredAtUtc: activity.occurredAtUtc ?? activity.OccurredAtUtc ?? new Date().toISOString(),
  };
};

type RevenueTooltipDatum = {
  revenue?: number;
};

const Dashboard: React.FC = () => {
  const { user } = useAuth();
  
  // 1. Lấy dữ liệu tổng hợp lần đầu
  const { data: summaryRes, isLoading } = useQuery({
    queryKey: ['dashboard-summary'],
    queryFn: () => dashboardApi.getSummary(),
  });

  // 2. Polling KPI snapshot mỗi 60s
  const { data: kpiSnapshotRes } = useQuery({
    queryKey: ['dashboard-kpi'],
    queryFn: () => dashboardApi.getKpiSnapshot(),
    refetchInterval: 60000, // 60 seconds
  });

  const summary = summaryRes?.data;
  // Ưu tiên dùng KPI từ snapshot (vì nó được polling), fallback về KPI của summary
  const kpi = kpiSnapshotRes?.data || summary?.kpi;

  // 3. Quản lý trạng thái Real-time (Recent Activities)
  const [realtimeActivities, setRealtimeActivities] = useState<RecentActivity[]>([]);
  const [isSignalRConnected, setIsSignalRConnected] = useState(false);
  const summaryActivities = useMemo(() => summary?.recentActivities ?? [], [summary?.recentActivities]);
  const summaryActivitiesRef = useRef<RecentActivity[]>([]);
  const displayedActivities = realtimeActivities.length > 0 ? realtimeActivities : summaryActivities;

  useEffect(() => {
    summaryActivitiesRef.current = summaryActivities;
  }, [summaryActivities]);

  // 4. Kết nối SignalR
  useEffect(() => {
    const gatewayUrl = getApiGatewayBaseUrl();
    const hubUrl = `${gatewayUrl}/hubs/admin-dashboard`;
    
    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => getAccessToken() || '',
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    connection.on('NewBooking', (activity: NewBookingPayload) => {
      console.log('🔔 Mới có booking:', activity);
      setRealtimeActivities((prev) => {
        const currentActivities = prev.length > 0 ? prev : summaryActivitiesRef.current;
        const normalizedActivity = normalizeRecentActivity(activity);
        if (normalizedActivity.status !== 'Completed') {
          return currentActivities;
        }

        const newActivities = [normalizedActivity, ...currentActivities];
        return newActivities.slice(0, 10);
      });
    });

    connection.onreconnecting((error) => {
      console.warn('SignalR reconnecting...', error);
      setIsSignalRConnected(false);
    });

    connection.onreconnected(async () => {
      try {
        await connection.invoke('JoinDashboard');
        setIsSignalRConnected(true);
      } catch (error) {
        console.error('Failed to rejoin dashboard group', error);
        setIsSignalRConnected(false);
      }
    });

    connection.onclose((error) => {
      console.error('Admin dashboard SignalR closed', error);
      setIsSignalRConnected(false);
    });

    connection.start()
      .then(async () => {
        console.log('✅ Connected to Admin Dashboard SignalR Hub');
        await connection.invoke('JoinDashboard');
        setIsSignalRConnected(true);
      })
      .catch((err) => {
        console.error('❌ SignalR Connection Error: ', err);
        setIsSignalRConnected(false);
      });

    return () => {
      connection.off('NewBooking');
      if (connection.state === HubConnectionState.Connected) {
        connection.invoke('LeaveDashboard').catch(e => console.error(e));
      }
      connection.stop();
    };
  }, []);

  // ---- Dữ liệu Biểu đồ ----
  
  const revenueChartConfig = {
    data: summary?.revenueChart.weekly || [],
    xField: 'label',
    yField: 'revenue',
    smooth: true,
    area: {
      style: {
        fillOpacity: 0.15,
        fill: 'l(270) 0:#ffffff00 1:#1677ff',
      },
    },
    point: {
      size: 4,
      shape: 'circle',
      style: {
        fill: 'white',
        stroke: '#1677ff',
        lineWidth: 2,
      },
    },
    tooltip: {
      formatter: (datum: RevenueTooltipDatum) => ({
        name: 'Doanh thu',
        value: formatMoney(datum.revenue),
      }),
    },
  };

  const occupancyData = useMemo(() => {
    const rate = kpi?.occupancyRate || 0;
    return [
      { type: 'Đã đặt', value: rate },
      { type: 'Còn trống', value: Math.max(0, 100 - rate) },
    ];
  }, [kpi?.occupancyRate]);

  const donutConfig = {
    data: occupancyData,
    angleField: 'value',
    colorField: 'type',
    radius: 0.8,
    innerRadius: 0.6,
    color: ['#1677ff', '#f0f0f0'],
    statistic: {
      title: false,
      content: {
        style: { whiteSpace: 'pre-wrap', overflow: 'hidden', textOverflow: 'ellipsis', fontSize: '24px' },
        content: `${(kpi?.occupancyRate || 0).toFixed(1)}%`,
      },
    },
    legend: false,
  };

  // ---- Bảng Top Phim ----
  const topMovieColumns = [
    { title: 'Hạng', dataIndex: 'rank', key: 'rank', width: 70, align: 'center' as const, render: (r: number) => <Text strong>{r}</Text> },
    {
      title: 'Phim', key: 'movie',
      render: (_: unknown, record: TopMovie) => (
        <Space>
          <Avatar src={record.posterUrl} shape="square" icon={<VideoCameraOutlined />} />
          <Text strong>{record.title}</Text>
        </Space>
      ),
    },
    { title: 'Vé bán', dataIndex: 'ticketsSold', key: 'ticketsSold', render: (val: number) => asNumber(val).toLocaleString('vi-VN') },
    {
      title: 'Doanh thu', dataIndex: 'revenue', key: 'revenue', align: 'right' as const,
      render: (val: number) => <Text strong style={{ color: '#1677ff' }}>{formatMoney(val)}</Text>,
    },
    {
      title: 'Lấp đầy', dataIndex: 'occupancyRate', key: 'occupancyRate',
      render: (rate: number) => (
        <Tooltip title={`${rate.toFixed(1)}%`}>
          <Progress percent={rate} size="small" showInfo={false} strokeColor={rate > 80 ? '#f5222d' : rate > 50 ? '#faad14' : '#52c41a'} />
        </Tooltip>
      ),
    },
    {
      title: 'Xu hướng', dataIndex: 'trendDirection', key: 'trendDirection', align: 'center' as const,
      render: (trend: string) => {
        if (trend === 'up') return <RiseOutlined style={{ color: '#52c41a', fontSize: 18 }} />;
        if (trend === 'down') return <FallOutlined style={{ color: '#ff4d4f', fontSize: 18 }} />;
        return <Text type="secondary">—</Text>;
      },
    },
  ];

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '60vh' }}>
        <Spin size="large" tip="Đang tải dữ liệu Command Center..." />
      </div>
    );
  }

  return (
    <div>
      {/* Header Area */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 24 }}>
        <div>
          <Title level={3} style={{ margin: 0 }}>
            Hệ thống Quản trị rạp phim
          </Title>
          <Text type="secondary">
            Tổng quan hiệu suất hoạt động kinh doanh (Cập nhật lúc: {dayjs().format('HH:mm')})
          </Text>
        </div>
        <div>
          <Space size="large">
            <Badge
              status={isSignalRConnected ? 'processing' : 'default'}
              text={isSignalRConnected ? 'SignalR: Connected' : 'SignalR: Disconnected'}
            />
            <Text type="secondary">Xin chào, <strong>{user?.fullName || user?.email}</strong>!</Text>
          </Space>
        </div>
      </div>

      {/* KPI Cards Row */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card bordered={false} style={{ borderRadius: 12, boxShadow: '0 2px 8px rgba(0,0,0,0.04)' }}>
            <Statistic
              title={<span style={{ color: '#8c8c8c' }}>Doanh thu hôm nay</span>}
              value={kpi?.todayRevenue || 0}
              precision={0}
              valueStyle={{ color: '#1677ff', fontWeight: 'bold' }}
              prefix={<DollarOutlined />}
              suffix="đ"
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card bordered={false} style={{ borderRadius: 12, boxShadow: '0 2px 8px rgba(0,0,0,0.04)' }}>
            <Statistic
              title={<span style={{ color: '#8c8c8c' }}>Vé bán hôm nay</span>}
              value={kpi?.todayTicketsSold || 0}
              valueStyle={{ color: '#52c41a', fontWeight: 'bold' }}
              prefix={<ShoppingCartOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card bordered={false} style={{ borderRadius: 12, boxShadow: '0 2px 8px rgba(0,0,0,0.04)' }}>
            <Statistic
              title={<span style={{ color: '#8c8c8c' }}>Tỷ lệ lấp đầy</span>}
              value={kpi?.occupancyRate || 0}
              precision={1}
              valueStyle={{ color: '#faad14', fontWeight: 'bold' }}
              prefix={<BarChartOutlined />}
              suffix="%"
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card bordered={false} style={{ borderRadius: 12, boxShadow: '0 2px 8px rgba(0,0,0,0.04)' }}>
            <Statistic
              title={<span style={{ color: '#8c8c8c' }}>Phim đang chiếu</span>}
              value={kpi?.showingMoviesCount || 0}
              valueStyle={{ color: '#722ed1', fontWeight: 'bold' }}
              prefix={<VideoCameraOutlined />}
              suffix={`/ ${kpi?.todayShowtimesCount || 0} suất`}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={16} style={{ marginBottom: 24 }}>
        {/* Main Chart Column */}
        <Col span={16}>
          <Card
            title={
              <Space>
                <BarChartOutlined />
                <Text strong>Doanh thu 7 ngày qua</Text>
              </Space>
            }
            bordered={false}
            style={{ borderRadius: 12, boxShadow: '0 2px 8px rgba(0,0,0,0.04)' }}
          >
            {summary?.revenueChart.weekly.length ? (
              <Line {...revenueChartConfig} height={280} />
            ) : (
              <div style={{ height: 280, display: 'flex', alignItems: 'center', justifyContent: 'center' }}><Text type="secondary">Chưa có dữ liệu doanh thu</Text></div>
            )}
          </Card>
        </Col>

        {/* Occupancy Donut Column */}
        <Col span={8}>
          <Card
            title={
              <Space>
                <FireOutlined style={{ color: '#ff4d4f' }} />
                <Text strong>Tỷ lệ lấp đầy hôm nay</Text>
              </Space>
            }
            bordered={false}
            style={{ borderRadius: 12, boxShadow: '0 2px 8px rgba(0,0,0,0.04)', height: '100%' }}
          >
            <Pie {...donutConfig} height={200} />
            
            {kpi?.hotMovie && (
              <div style={{ marginTop: 24, textAlign: 'center', background: '#f5f5f5', padding: '12px', borderRadius: 8 }}>
                <Text type="secondary" style={{ fontSize: 12 }}>Phim hot nhất trong ngày</Text>
                <div style={{ marginTop: 4 }}>
                  <Text strong style={{ fontSize: 16 }}>{kpi.hotMovie.title}</Text>
                </div>
                <div style={{ marginTop: 8 }}>
                  <Tag color="orange">{kpi.hotMovie.ticketsSold} vé</Tag>
                  <Tag color="blue">{(asNumber(kpi.hotMovie.revenue) / 1000).toFixed(0)}K đ</Tag>
                </div>
              </div>
            )}
          </Card>
        </Col>
      </Row>

      <Row gutter={16}>
        {/* Top Movies Table */}
        <Col span={16}>
          <Card
            title={<Text strong>Top Phim Thịnh Hành (7 ngày)</Text>}
            bordered={false}
            style={{ borderRadius: 12, boxShadow: '0 2px 8px rgba(0,0,0,0.04)' }}
          >
            <Table
              columns={topMovieColumns}
              dataSource={summary?.topMovies || []}
              pagination={false}
              rowKey="movieId"
              size="small"
            />
          </Card>
        </Col>

        {/* Recent Activity Timeline */}
        <Col span={8}>
          <Card
            title={<Text strong>Giao dịch Real-time</Text>}
            bordered={false}
            style={{ borderRadius: 12, boxShadow: '0 2px 8px rgba(0,0,0,0.04)', height: '100%' }}
          >
            <div style={{ maxHeight: 350, overflowY: 'auto', paddingRight: 8 }}>
              {displayedActivities.length > 0 ? (
                <Timeline>
                  {displayedActivities.map((act) => (
                    <Timeline.Item
                      key={act.bookingId}
                      color={act.status === 'Completed' ? 'green' : 'blue'}
                      dot={act.status === 'Completed' ? <CheckCircleOutlined /> : <ClockCircleOutlined />}
                    >
                      <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Text strong>{act.customerName}</Text>
                        <Text type="secondary" style={{ fontSize: 12 }}>
                          {toLocalDateTime(act.occurredAtUtc).format('HH:mm')}
                        </Text>
                      </div>
                      <div style={{ fontSize: 13 }}>
                        Đặt <Text strong>{act.seatsCount} vé</Text> — <Text italic>{act.movieTitle}</Text>
                      </div>
                      <div style={{ color: '#1677ff', fontWeight: 500, fontSize: 13, marginTop: 4 }}>
                        +{formatMoney(act.amount)}
                      </div>
                    </Timeline.Item>
                  ))}
                </Timeline>
              ) : (
                <div style={{ textAlign: 'center', marginTop: 50 }}>
                  <Text type="secondary">Chưa có giao dịch nào gần đây</Text>
                </div>
              )}
            </div>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default Dashboard;
