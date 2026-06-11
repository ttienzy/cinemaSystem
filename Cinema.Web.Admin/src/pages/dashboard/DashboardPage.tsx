import { useQuery } from '@tanstack/react-query';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useEffect } from 'react';
import {
  CalendarOutlined,
  DollarOutlined,
  PercentageOutlined,
  ReloadOutlined,
  TagOutlined,
  VideoCameraOutlined,
} from '@ant-design/icons';
import { Button, Card, Col, Empty, Progress, Row, Space, Statistic, Table, Tag, Typography } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { dashboardApi, type RevenuePoint } from '../../features/dashboard/dashboardApi';
import { getAccessToken } from '../../shared/auth/tokenStorage';
import { getApiGatewayBaseUrl } from '../../shared/utils/apiConfig';
import { formatDateTime, formatMoney } from '../../shared/utils/format';

const { Text } = Typography;

export default function DashboardPage() {
  const summaryQuery = useQuery({
    queryKey: ['dashboard-summary'],
    queryFn: dashboardApi.getSummary,
    refetchInterval: 60_000,
  });

  const kpiQuery = useQuery({
    queryKey: ['dashboard-kpi-snapshot'],
    queryFn: dashboardApi.getKpiSnapshot,
    refetchInterval: 30_000,
  });

  const summary = summaryQuery.data?.data;
  const kpi = kpiQuery.data?.data ?? summary?.kpi;
  const loading = summaryQuery.isLoading || kpiQuery.isLoading;

  useEffect(() => {
    const token = getAccessToken();
    if (!token) return;

    const connection = new HubConnectionBuilder()
      .withUrl(`${getApiGatewayBaseUrl()}/hubs/admin-dashboard`, {
        accessTokenFactory: () => getAccessToken() ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    const refreshDashboard = () => {
      void summaryQuery.refetch();
      void kpiQuery.refetch();
    };

    connection.on('NewBooking', refreshDashboard);

    void connection
      .start()
      .then(() => connection.invoke('JoinDashboard'))
      .catch(() => {
        // Polling remains the fallback if realtime cannot connect.
      });

    return () => {
      void connection
        .invoke('LeaveDashboard')
        .catch(() => undefined)
        .finally(() => {
          void connection.stop();
        });
    };
  }, [kpiQuery.refetch, summaryQuery.refetch]);

  const revenueColumns: ColumnsType<RevenuePoint> = [
    { title: 'Period', dataIndex: 'label' },
    { title: 'Bookings', dataIndex: 'bookingsCount', width: 110 },
    { title: 'Tickets', dataIndex: 'ticketsSold', width: 100 },
    {
      title: 'Revenue',
      dataIndex: 'revenue',
      width: 180,
      render: (value: number) => formatMoney(value),
    },
  ];

  return (
    <Space direction="vertical" size={20} style={{ width: '100%' }}>
      <div className="page-header">
        <div>
          <h1 className="page-title">Dashboard</h1>
          <div className="page-subtitle">Cinema operations overview</div>
        </div>
        <Button
          icon={<ReloadOutlined />}
          onClick={() => {
            void summaryQuery.refetch();
            void kpiQuery.refetch();
          }}
        />
      </div>

      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card className="metric-card" loading={loading}>
            <Statistic title="Today revenue" value={formatMoney(kpi?.todayRevenue)} prefix={<DollarOutlined />} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card className="metric-card" loading={loading}>
            <Statistic title="Tickets sold" value={kpi?.todayTicketsSold ?? 0} prefix={<TagOutlined />} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card className="metric-card" loading={loading}>
            <Statistic title="Showtimes today" value={kpi?.todayShowtimesCount ?? 0} prefix={<CalendarOutlined />} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card className="metric-card" loading={loading}>
            <Statistic title="Showing movies" value={kpi?.showingMoviesCount ?? 0} prefix={<VideoCameraOutlined />} />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]}>
        <Col xs={24} lg={8}>
          <Card title="Occupancy" loading={loading}>
            <Progress
              type="dashboard"
              percent={Math.round(kpi?.occupancyRate ?? 0)}
              format={(value) => `${value ?? 0}%`}
            />
            <div style={{ marginTop: 12 }}>
              <Space direction="vertical" size={2}>
                <Text type="secondary">Hot movie</Text>
                <Text strong>{kpi?.hotMovie?.title ?? 'No data'}</Text>
                <Text>{kpi?.hotMovie ? formatMoney(kpi.hotMovie.revenue) : '-'}</Text>
              </Space>
            </div>
          </Card>
        </Col>
        <Col xs={24} lg={16}>
          <Card title="Weekly revenue" loading={summaryQuery.isLoading}>
            <Table<RevenuePoint>
              size="small"
              rowKey="date"
              pagination={false}
              dataSource={summary?.revenueChart.weekly ?? []}
              columns={revenueColumns}
              locale={{ emptyText: <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]}>
        <Col xs={24} xl={12}>
          <Card title="Top movies" loading={summaryQuery.isLoading}>
            <Table
              size="small"
              rowKey="movieId"
              pagination={false}
              dataSource={summary?.topMovies ?? []}
              columns={[
                { title: '#', dataIndex: 'rank', width: 64 },
                { title: 'Movie', dataIndex: 'title' },
                { title: 'Tickets', dataIndex: 'ticketsSold', width: 100 },
                {
                  title: 'Revenue',
                  dataIndex: 'revenue',
                  width: 160,
                  render: (value: number) => formatMoney(value),
                },
                {
                  title: 'Occupancy',
                  dataIndex: 'occupancyRate',
                  width: 120,
                  render: (value: number) => `${Math.round(value)}%`,
                },
              ]}
            />
          </Card>
        </Col>
        <Col xs={24} xl={12}>
          <Card title="Recent activities" loading={summaryQuery.isLoading}>
            <Table
              size="small"
              rowKey="bookingId"
              pagination={false}
              dataSource={summary?.recentActivities ?? []}
              columns={[
                { title: 'Movie', dataIndex: 'movieTitle' },
                { title: 'Customer', dataIndex: 'customerName', width: 150 },
                {
                  title: 'Status',
                  dataIndex: 'status',
                  width: 120,
                  render: (value: string) => <Tag color="blue">{value}</Tag>,
                },
                {
                  title: 'Amount',
                  dataIndex: 'amount',
                  width: 150,
                  render: (value: number) => formatMoney(value),
                },
                {
                  title: 'Time',
                  dataIndex: 'occurredAtUtc',
                  width: 150,
                  render: (value: string) => formatDateTime(value),
                },
              ]}
            />
          </Card>
        </Col>
      </Row>
    </Space>
  );
}
