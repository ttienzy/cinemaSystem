import React, { useState } from 'react';
import {
  Table, Tag, Card, Row, Col, Statistic, Input, Select, Space,
  Typography, Breadcrumb, Progress, Badge, Tooltip, Button, Empty,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  BankOutlined, HomeOutlined, SearchOutlined, CheckCircleOutlined,
  CloseCircleOutlined, SettingOutlined, AppstoreOutlined,
} from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import {
  cinemaApi,
  type CinemaAdminOverview,
  type CinemaHall,
} from '../../../features/cinemas/api/cinemaApi';

const { Title, Text } = Typography;
const { Search } = Input;

const CinemasPage: React.FC = () => {
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const { data: overviewRes, isLoading } = useQuery({
    queryKey: ['admin-cinemas', search, statusFilter, pageNumber, pageSize],
    queryFn: () => cinemaApi.getAdminOverview({ search, status: statusFilter, pageNumber, pageSize }),
  });
  const { data: summaryRes } = useQuery({
    queryKey: ['admin-cinemas-summary'],
    queryFn: () => cinemaApi.getAdminSummary(),
  });

  const cinemas = overviewRes?.data?.items || [];
  const summary = summaryRes?.data;

  // Expanded row: Halls table
  const hallColumns: ColumnsType<CinemaHall> = [
    { title: 'Phòng chiếu', dataIndex: 'name', key: 'name', render: (n: string) => <Text strong>{n}</Text> },
    {
      title: 'Loại phòng', dataIndex: 'roomType', key: 'roomType',
      render: (t: string) => <Tag color={t === 'IMAX' ? 'volcano' : t === '3D' ? 'purple' : 'blue'}>{t || '2D'}</Tag>,
    },
    { title: 'Tổng ghế', dataIndex: 'totalSeats', key: 'totalSeats', width: 100 },
    {
      title: 'Phân loại ghế', key: 'seatBreakdown',
      render: (_, h) => (
        <Space size={4}>
          {h.normalSeatCount > 0 && <Tag>Thường: {h.normalSeatCount}</Tag>}
          {h.vipSeatCount > 0 && <Tag color="gold">VIP: {h.vipSeatCount}</Tag>}
          {h.coupleSeatCount > 0 && <Tag color="magenta">Couple: {h.coupleSeatCount}</Tag>}
        </Space>
      ),
    },
    {
      title: 'Sơ đồ ghế', dataIndex: 'seatMapConfigured', key: 'seatMap', width: 120,
      render: (v: boolean) => v
        ? <Tag color="success" icon={<CheckCircleOutlined />}>Đã cấu hình</Tag>
        : <Tag color="warning" icon={<SettingOutlined />}>Chưa cấu hình</Tag>,
    },
  ];

  // Main cinema columns
  const columns: ColumnsType<CinemaAdminOverview> = [
    {
      title: 'Cụm rạp', dataIndex: 'name', key: 'name',
      render: (name: string, record) => (
        <div>
          <Text strong style={{ fontSize: 14 }}><BankOutlined style={{ marginRight: 6, color: '#1677ff' }} />{name}</Text>
          <div style={{ fontSize: 12, color: '#8c8c8c', marginTop: 2 }}>{record.address}</div>
          {record.city && <Tag style={{ marginTop: 4, fontSize: 11 }}>{record.city}</Tag>}
        </div>
      ),
    },
    {
      title: 'Trạng thái', dataIndex: 'status', key: 'status', width: 140,
      render: (s: string) => s === 'Active'
        ? <Badge status="success" text={<Tag color="success">Hoạt động</Tag>} />
        : <Badge status="error" text={<Tag color="error">Tạm đóng</Tag>} />,
    },
    {
      title: 'Phòng chiếu', dataIndex: 'totalHalls', key: 'totalHalls', width: 120,
      render: (v: number) => <><AppstoreOutlined style={{ marginRight: 4 }} />{v} phòng</>,
    },
    {
      title: 'Tổng ghế', dataIndex: 'totalSeats', key: 'totalSeats', width: 120,
      render: (v: number) => <Text strong>{v.toLocaleString()}</Text>,
    },
    {
      title: 'Sơ đồ ghế', key: 'seatMapProgress', width: 140,
      render: (_, r) => {
        const configured = r.cinemaHalls.filter(h => h.seatMapConfigured).length;
        const total = r.cinemaHalls.length;
        const pct = total > 0 ? Math.round((configured / total) * 100) : 0;
        return (
          <Tooltip title={`${configured}/${total} phòng đã cấu hình`}>
            <Progress percent={pct} size="small" steps={total > 0 ? total : undefined}
              strokeColor={pct === 100 ? '#52c41a' : '#1677ff'} />
          </Tooltip>
        );
      },
    },
  ];

  return (
    <div>
      <Breadcrumb items={[{ title: 'Admin' }, { title: 'Quản lý Cụm rạp' }]} style={{ marginBottom: 16 }} />

      {/* Summary */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={5}><Card style={{ borderRadius: 8 }}><Statistic title="Tổng cụm rạp" value={summary?.totalCinemas || 0} prefix={<BankOutlined />} /></Card></Col>
        <Col span={5}><Card style={{ borderRadius: 8 }}><Statistic title="Đang hoạt động" value={summary?.activeCinemas || 0} valueStyle={{ color: '#52c41a' }} prefix={<CheckCircleOutlined />} /></Card></Col>
        <Col span={5}><Card style={{ borderRadius: 8 }}><Statistic title="Tạm đóng" value={summary?.inactiveCinemas || 0} valueStyle={{ color: '#ff4d4f' }} prefix={<CloseCircleOutlined />} /></Card></Col>
        <Col span={5}><Card style={{ borderRadius: 8 }}><Statistic title="Tổng phòng chiếu" value={summary?.totalHalls || 0} prefix={<AppstoreOutlined />} /></Card></Col>
        <Col span={4}><Card style={{ borderRadius: 8 }}><Statistic title="Tổng ghế" value={summary?.totalSeats || 0} prefix={<HomeOutlined />} /></Card></Col>
      </Row>

      {/* Toolbar */}
      <Card style={{ marginBottom: 16, borderRadius: 12 }}>
        <Space>
          <Search placeholder="Tìm cụm rạp..." allowClear style={{ width: 260 }}
            onSearch={(v) => { setSearch(v); setPageNumber(1); }} />
          <Select placeholder="Trạng thái" allowClear style={{ width: 160 }}
            value={statusFilter} onChange={(v) => { setStatusFilter(v); setPageNumber(1); }}
            options={[
              { label: '🟢 Hoạt động', value: 'Active' },
              { label: '🔴 Tạm đóng', value: 'Inactive' },
            ]}
          />
        </Space>
      </Card>

      {/* Table with expandable halls */}
      <Card style={{ borderRadius: 12 }}>
        <Table
          columns={columns} dataSource={cinemas} rowKey="id" loading={isLoading}
          expandable={{
            expandedRowRender: (record) => (
              record.cinemaHalls.length > 0
                ? <Table columns={hallColumns} dataSource={record.cinemaHalls} rowKey="id"
                    pagination={false} size="small" style={{ margin: '8px 0' }} />
                : <Empty description="Chưa có phòng chiếu" image={Empty.PRESENTED_IMAGE_SIMPLE} />
            ),
            rowExpandable: () => true,
          }}
          pagination={{
            current: pageNumber, pageSize,
            total: overviewRes?.data?.totalCount || 0,
            onChange: (p, s) => { setPageNumber(p); setPageSize(s); },
            showSizeChanger: true,
          }}
        />
      </Card>
    </div>
  );
};

export default CinemasPage;
