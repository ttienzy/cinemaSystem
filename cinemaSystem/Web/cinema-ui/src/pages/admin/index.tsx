// Admin Dashboard Page - Statistics and overview
import { useState } from 'react';
import { Row, Col, Card, Typography, Statistic, Spin, DatePicker, Select, Table } from 'antd';
import { DollarOutlined, CalendarOutlined, RiseOutlined, IdcardOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { useAdminDashboardStats, useAdminRevenueReport, useAdminTopMovies } from '../../features/admin/dashboard/hooks/useAdminDashboard';
import { useCinemas } from '../../features/cinemas/hooks/useCinemas';
import type { TopMovie } from '../../features/admin/dashboard/types/adminDashboard.types';

const { Title, Text } = Typography;
const { RangePicker } = DatePicker;

const AdminDashboardPage = () => {
    const [cinemaId, setCinemaId] = useState<string | undefined>(undefined);
    const [dateRange, setDateRange] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([
        dayjs().subtract(30, 'day'),
        dayjs()
    ]);

    // Fetch dashboard stats
    const { data: stats, isLoading: statsLoading } = useAdminDashboardStats(cinemaId);

    // Fetch revenue report
    const { data: revenueData, isLoading: revenueLoading } = useAdminRevenueReport(
        dateRange[0].format('YYYY-MM-DD'),
        dateRange[1].format('YYYY-MM-DD'),
        cinemaId,
        'day'
    );

    // Fetch top movies
    const { data: topMovies, isLoading: topMoviesLoading } = useAdminTopMovies(
        10,
        dateRange[0].format('YYYY-MM-DD'),
        dateRange[1].format('YYYY-MM-DD')
    );

    // Fetch cinemas for filter
    const { data: cinemasData } = useCinemas(1, 100);
    const cinemas = cinemasData?.items || [];

    const handleCinemaChange = (value: string | undefined) => {
        setCinemaId(value);
    };

    const handleDateRangeChange = (dates: unknown) => {
        const d = dates as [dayjs.Dayjs, dayjs.Dayjs] | null;
        if (d && d[0] && d[1]) {
            setDateRange([d[0], d[1]]);
        }
    };

    const topMoviesColumns = [
        {
            title: 'STT',
            dataIndex: 'index',
            key: 'index',
            width: 60,
            render: (_: unknown, __: TopMovie, index: number) => index + 1,
        },
        {
            title: 'Tên phim',
            dataIndex: 'movieTitle',
            key: 'movieTitle',
            render: (text: string) => <Text strong style={{ color: '#fff' }}>{text}</Text>,
        },
        {
            title: 'Số vé',
            dataIndex: 'totalTickets',
            key: 'totalTickets',
            align: 'right' as const,
            render: (value: number) => <Text style={{ color: '#fff' }}>{value.toLocaleString('vi-VN')}</Text>,
        },
        {
            title: 'Số đơn',
            dataIndex: 'totalBookings',
            key: 'totalBookings',
            align: 'right' as const,
            render: (value: number) => <Text style={{ color: '#fff' }}>{value.toLocaleString('vi-VN')}</Text>,
        },
        {
            title: 'Doanh thu',
            dataIndex: 'totalRevenue',
            key: 'totalRevenue',
            align: 'right' as const,
            render: (value: number) => <Text strong style={{ color: '#ff4d4f' }}>{value.toLocaleString('vi-VN')}đ</Text>,
        },
    ];

    if (statsLoading) {
        return (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
                <Spin size="large" />
            </div>
        );
    }

    return (
        <div style={{ padding: '24px', background: '#141414', minHeight: '100vh' }}>
            <Title level={2} style={{ color: '#fff', marginBottom: '24px' }}>
                Dashboard
            </Title>

            {/* Filters */}
            <Card style={{ background: '#1f1f1f', borderColor: '#333', marginBottom: '24px' }}>
                <Row gutter={[16, 16]} align="middle">
                    <Col xs={24} sm={8}>
                        <Text style={{ color: '#fff', display: 'block', marginBottom: '8px' }}>Rạp chiếu:</Text>
                        <Select
                            style={{ width: '100%' }}
                            placeholder="Tất cả rạp"
                            allowClear
                            value={cinemaId}
                            onChange={handleCinemaChange}
                        >
                            {cinemas.map(cinema => (
                                <Select.Option key={cinema.id} value={cinema.id}>
                                    {cinema.name}
                                </Select.Option>
                            ))}
                        </Select>
                    </Col>
                    <Col xs={24} sm={16}>
                        <Text style={{ color: '#fff', display: 'block', marginBottom: '8px' }}>Khoảng thời gian:</Text>
                        <RangePicker
                            style={{ width: '100%' }}
                            value={dateRange}
                            onChange={handleDateRangeChange}
                            format="DD/MM/YYYY"
                        />
                    </Col>
                </Row>
            </Card>

            {/* Stats Cards */}
            <Row gutter={[16, 16]}>
                <Col xs={24} sm={12} lg={6}>
                    <Card style={{ background: '#1f1f1f', borderColor: '#333' }}>
                        <Statistic
                            title={<span style={{ color: '#999' }}>Doanh thu hôm nay</span>}
                            value={stats?.todayRevenue || 0}
                            prefix={<DollarOutlined style={{ color: '#52c41a' }} />}
                            valueStyle={{ color: '#fff' }}
                            formatter={(value) => `${Number(value).toLocaleString('vi-VN')}đ`}
                        />
                    </Card>
                </Col>
                <Col xs={24} sm={12} lg={6}>
                    <Card style={{ background: '#1f1f1f', borderColor: '#333' }}>
                        <Statistic
                            title={<span style={{ color: '#999' }}>Đơn hôm nay</span>}
                            value={stats?.todayBookings || 0}
                            prefix={<CalendarOutlined style={{ color: '#1890ff' }} />}
                            valueStyle={{ color: '#fff' }}
                        />
                    </Card>
                </Col>
                <Col xs={24} sm={12} lg={6}>
                    <Card style={{ background: '#1f1f1f', borderColor: '#333' }}>
                        <Statistic
                            title={<span style={{ color: '#999' }}>Doanh thu tháng</span>}
                            value={stats?.monthlyRevenue || 0}
                            prefix={<RiseOutlined style={{ color: '#52c41a' }} />}
                            valueStyle={{ color: '#fff' }}
                            formatter={(value) => `${Number(value).toLocaleString('vi-VN')}đ`}
                        />
                    </Card>
                </Col>
                <Col xs={24} sm={12} lg={6}>
                    <Card style={{ background: '#1f1f1f', borderColor: '#333' }}>
                        <Statistic
                            title={<span style={{ color: '#999' }}>Tổng vé</span>}
                            value={stats?.totalTickets || 0}
                            prefix={<IdcardOutlined style={{ color: '#ff4d4f' }} />}
                            valueStyle={{ color: '#fff' }}
                        />
                    </Card>
                </Col>
            </Row>

            {/* Additional Stats */}
            <Row gutter={[16, 16]} style={{ marginTop: '16px' }}>
                <Col xs={24} sm={12} lg={6}>
                    <Card style={{ background: '#1f1f1f', borderColor: '#333' }}>
                        <Statistic
                            title={<span style={{ color: '#999' }}>Tổng doanh thu</span>}
                            value={stats?.totalRevenue || 0}
                            valueStyle={{ color: '#fff' }}
                            formatter={(value) => `${Number(value).toLocaleString('vi-VN')}đ`}
                        />
                    </Card>
                </Col>
                <Col xs={24} sm={12} lg={6}>
                    <Card style={{ background: '#1f1f1f', borderColor: '#333' }}>
                        <Statistic
                            title={<span style={{ color: '#999' }}>Tổng đơn</span>}
                            value={stats?.totalBookings || 0}
                            valueStyle={{ color: '#fff' }}
                        />
                    </Card>
                </Col>
                <Col xs={24} sm={12} lg={6}>
                    <Card style={{ background: '#1f1f1f', borderColor: '#333' }}>
                        <Statistic
                            title={<span style={{ color: '#999' }}>Đơn tháng</span>}
                            value={stats?.monthlyBookings || 0}
                            valueStyle={{ color: '#fff' }}
                        />
                    </Card>
                </Col>
                <Col xs={24} sm={12} lg={6}>
                    <Card style={{ background: '#1f1f1f', borderColor: '#333' }}>
                        <Statistic
                            title={<span style={{ color: '#999' }}>Tỷ lệ lấp đầy TB</span>}
                            value={stats?.averageOccupancyRate || 0}
                            precision={1}
                            suffix="%"
                            valueStyle={{ color: '#fff' }}
                        />
                    </Card>
                </Col>
            </Row>

            {/* Top Movies */}
            <Card
                style={{ background: '#1f1f1f', borderColor: '#333', marginTop: '24px' }}
                title={<span style={{ color: '#fff' }}>Top Phim</span>}
            >
                {topMoviesLoading ? (
                    <div style={{ textAlign: 'center', padding: '24px' }}>
                        <Spin />
                    </div>
                ) : (
                    <Table
                        dataSource={topMovies}
                        columns={topMoviesColumns}
                        rowKey="movieId"
                        pagination={false}
                        size="small"
                    />
                )}
            </Card>

            {/* Revenue Chart placeholder */}
            <Card
                style={{ background: '#1f1f1f', borderColor: '#333', marginTop: '24px' }}
                title={<span style={{ color: '#fff' }}>Doanh thu theo thời gian</span>}
            >
                {revenueLoading ? (
                    <div style={{ textAlign: 'center', padding: '24px' }}>
                        <Spin />
                    </div>
                ) : (
                    <div style={{ padding: '24px', textAlign: 'center' }}>
                        <Text type="secondary" style={{ color: '#999' }}>
                            Biểu đồ doanh thu (số liệu từ {dateRange[0].format('DD/MM/YYYY')} đến {dateRange[1].format('DD/MM/YYYY')})
                        </Text>
                        <div style={{ marginTop: '16px' }}>
                            <Text style={{ color: '#fff' }}>
                                Tổng doanh thu: <Text strong style={{ color: '#ff4d4f', fontSize: '24px' }}>
                                    {Array.isArray(revenueData) ? revenueData.reduce((sum, item) => sum + item.revenue, 0).toLocaleString('vi-VN') : '0'}đ
                                </Text>
                            </Text>
                        </div>
                    </div>
                )}
            </Card>
        </div>
    );
};

export default AdminDashboardPage;
