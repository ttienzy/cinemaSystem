// ============================================================
// Ticket Operations Page — Quản lý Đặt vé & Vận hành tại quầy
// ============================================================
// Trang này dùng cho Staff/Admin để:
// 1. Tra cứu vé nhanh (theo mã vé, email, SĐT)
// 2. Xem chi tiết vé (ghế, suất chiếu, thanh toán)
// 3. Check-in vé (quét mã QR / xác nhận khách vào phòng)
// ============================================================

import React, { useState, useCallback } from 'react';
import {
  Input,
  Table,
  Tag,
  Button,
  Space,
  Card,
  Modal,
  Descriptions,
  Typography,
  message,
  Tooltip,
  Badge,
  Row,
  Col,
  Statistic,
  Empty,
} from 'antd';
import {
  SearchOutlined,
  CheckCircleOutlined,
  InfoCircleOutlined,
  UserOutlined,
  MailOutlined,
  PhoneOutlined,
  VideoCameraOutlined,
  ClockCircleOutlined,
  CreditCardOutlined,
  ScanOutlined,
  ReloadOutlined,
  ExclamationCircleOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  ticketOperationsApi,
  type TicketOperationResponse,
} from '../../../features/tickets/api/ticketOperationsApi';
import type { ColumnsType } from 'antd/es/table';
import { toLocalDateTime } from '../../../utils/dateTime';

const { Title, Text } = Typography;
const { Search } = Input;

// ---- Status Helpers ----

const getStatusConfig = (status: string) => {
  const map: Record<string, { color: string; label: string; icon?: React.ReactNode }> = {
    Paid:              { color: 'green',      label: 'Đã thanh toán',  icon: <CreditCardOutlined /> },
    CheckedIn:         { color: 'blue',       label: 'Đã check-in',   icon: <CheckCircleOutlined /> },
    Cancelled:         { color: 'default',    label: 'Đã hủy' },
    Expired:           { color: 'default',    label: 'Hết hạn' },
    Refunded:          { color: 'orange',     label: 'Đã hoàn tiền' },
    ProcessingPayment: { color: 'processing', label: 'Đang thanh toán' },
    PaymentFailed:     { color: 'error',      label: 'Thanh toán lỗi' },
    PaymentCancelled:  { color: 'default',    label: 'Hủy thanh toán' },
    Pending:           { color: 'warning',    label: 'Chờ xử lý' },
    Confirmed:         { color: 'cyan',       label: 'Đã xác nhận' },
  };
  return map[status] || { color: 'default', label: status };
};

// ---- Component ----

const TicketOperationsPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [searchQuery, setSearchQuery] = useState('');
  const [activeQuery, setActiveQuery] = useState('');
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(15);
  const [detailModalOpen, setDetailModalOpen] = useState(false);
  const [selectedTicket, setSelectedTicket] = useState<TicketOperationResponse | null>(null);

  // ---- Data Fetching ----

  const { data: apiResponse, isLoading, isFetching } = useQuery({
    queryKey: ['ticket-operations', activeQuery, pageNumber, pageSize],
    queryFn: () => ticketOperationsApi.searchTickets(activeQuery, pageNumber, pageSize),
    enabled: activeQuery.length > 0,
  });

  const ticketsData = apiResponse?.data;
  const tickets = ticketsData?.items || [];

  // ---- Check-in Mutation ----

  const checkInMutation = useMutation({
    mutationFn: ticketOperationsApi.checkInTicket,
    onSuccess: (res) => {
      if (res.success) {
        message.success(res.message || 'Check-in thành công!');
        queryClient.invalidateQueries({ queryKey: ['ticket-operations'] });
        // Cập nhật selectedTicket nếu đang mở modal
        if (selectedTicket?.bookingId === res.data?.bookingId) {
          setSelectedTicket(res.data);
        }
      }
    },
  });

  // ---- Handlers ----

  const handleSearch = useCallback((value: string) => {
    const trimmed = value.trim();
    setActiveQuery(trimmed);
    setPageNumber(1);
  }, []);

  const handleCheckIn = useCallback((ticket: TicketOperationResponse) => {
    Modal.confirm({
      title: 'Xác nhận Check-in',
      icon: <ExclamationCircleOutlined />,
      content: (
        <div>
          <p>Bạn có chắc chắn muốn check-in vé này?</p>
          <Descriptions column={1} size="small" bordered style={{ marginTop: 12 }}>
            <Descriptions.Item label="Mã vé">{ticket.ticketCode}</Descriptions.Item>
            <Descriptions.Item label="Khách hàng">{ticket.customerName}</Descriptions.Item>
            <Descriptions.Item label="Phim">{ticket.showtimeDetails?.movieTitle}</Descriptions.Item>
            <Descriptions.Item label="Ghế">
              {ticket.seats.map(s => `${s.row}${s.number}`).join(', ')}
            </Descriptions.Item>
          </Descriptions>
        </div>
      ),
      okText: 'Check-in',
      okType: 'primary',
      cancelText: 'Hủy',
      onOk: () => checkInMutation.mutateAsync(ticket.bookingId),
    });
  }, [checkInMutation]);

  const openDetail = useCallback((ticket: TicketOperationResponse) => {
    setSelectedTicket(ticket);
    setDetailModalOpen(true);
  }, []);

  // ---- Table Columns ----

  const columns: ColumnsType<TicketOperationResponse> = [
    {
      title: 'Mã vé',
      dataIndex: 'ticketCode',
      key: 'ticketCode',
      width: 160,
      render: (code: string) => (
        <Text strong copyable style={{ fontFamily: 'monospace', fontSize: 13 }}>
          {code}
        </Text>
      ),
    },
    {
      title: 'Khách hàng',
      key: 'customer',
      width: 200,
      render: (_, record) => (
        <div>
          <div>
            <UserOutlined style={{ marginRight: 4, color: '#1677ff' }} />
            <Text strong>{record.customerName || '—'}</Text>
          </div>
          <div style={{ fontSize: 12, color: '#8c8c8c', marginTop: 2 }}>
            <MailOutlined style={{ marginRight: 4 }} />
            {record.customerEmail || '—'}
          </div>
          {record.customerPhone && (
            <div style={{ fontSize: 12, color: '#8c8c8c' }}>
              <PhoneOutlined style={{ marginRight: 4 }} />
              {record.customerPhone}
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Phim / Suất chiếu',
      key: 'showtime',
      width: 240,
      render: (_, record) => {
        const st = record.showtimeDetails;
        if (!st) return <Text type="secondary">—</Text>;
        return (
          <div>
            <div>
              <VideoCameraOutlined style={{ marginRight: 4, color: '#722ed1' }} />
              <Text strong>{st.movieTitle}</Text>
            </div>
            <div style={{ fontSize: 12, color: '#8c8c8c', marginTop: 2 }}>
              <ClockCircleOutlined style={{ marginRight: 4 }} />
              {toLocalDateTime(st.startTime).format('HH:mm DD/MM')} • {st.cinemaHallName}
            </div>
          </div>
        );
      },
    },
    {
      title: 'Ghế',
      key: 'seats',
      width: 120,
      render: (_, record) => (
        <Space size={4} wrap>
          {record.seats.map(s => (
            <Tag key={s.seatId} color={s.seatType === 'VIP' ? 'gold' : s.seatType === 'Couple' ? 'magenta' : 'default'}>
              {s.row}{s.number}
            </Tag>
          ))}
        </Space>
      ),
    },
    {
      title: 'Tổng tiền',
      dataIndex: 'totalPrice',
      key: 'totalPrice',
      width: 130,
      align: 'right',
      render: (val: number) => (
        <Text strong style={{ color: '#1677ff' }}>
          {val.toLocaleString()} đ
        </Text>
      ),
    },
    {
      title: 'Trạng thái',
      dataIndex: 'operationalStatus',
      key: 'operationalStatus',
      width: 150,
      filters: [
        { text: 'Đã thanh toán', value: 'Paid' },
        { text: 'Đã check-in', value: 'CheckedIn' },
        { text: 'Đã hủy', value: 'Cancelled' },
        { text: 'Hết hạn', value: 'Expired' },
      ],
      onFilter: (value, record) => record.operationalStatus === value,
      render: (status: string) => {
        const config = getStatusConfig(status);
        return (
          <Badge
            status={status === 'Paid' ? 'success' : status === 'CheckedIn' ? 'processing' : 'default'}
            text={<Tag color={config.color} icon={config.icon}>{config.label}</Tag>}
          />
        );
      },
    },
    {
      title: 'Hành động',
      key: 'actions',
      width: 160,
      fixed: 'right',
      render: (_, record) => (
        <Space size={4}>
          <Tooltip title="Xem chi tiết">
            <Button
              type="text"
              icon={<InfoCircleOutlined />}
              onClick={() => openDetail(record)}
            />
          </Tooltip>

          {record.canCheckIn && (
            <Button
              type="primary"
              icon={<ScanOutlined />}
              size="small"
              loading={checkInMutation.isPending}
              onClick={() => handleCheckIn(record)}
              style={{ borderRadius: 6 }}
            >
              Check-in
            </Button>
          )}

          {record.operationalStatus === 'CheckedIn' && (
            <Tag color="blue" icon={<CheckCircleOutlined />}>
              Đã vào rạp
            </Tag>
          )}
        </Space>
      ),
    },
  ];

  // ---- KPI Summary ----

  const paidCount = tickets.filter(t => t.operationalStatus === 'Paid').length;
  const checkedInCount = tickets.filter(t => t.operationalStatus === 'CheckedIn').length;
  const totalRevenue = tickets.reduce((sum, t) => sum + t.totalPrice, 0);

  return (
    <div>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 24 }}>
        <div>
          <Title level={3} style={{ margin: 0 }}>
            🎫 Quản lý Vé & Vận hành
          </Title>
          <Text type="secondary">
            Tra cứu vé theo mã vé, email hoặc số điện thoại khách hàng
          </Text>
        </div>
        <Button
          icon={<ReloadOutlined />}
          onClick={() => queryClient.invalidateQueries({ queryKey: ['ticket-operations'] })}
          disabled={!activeQuery}
        >
          Làm mới
        </Button>
      </div>

      {/* Search Bar */}
      <Card style={{ marginBottom: 24, borderRadius: 12 }}>
        <Search
          placeholder="Nhập mã vé, email hoặc SĐT khách hàng..."
          enterButton={
            <Button type="primary" icon={<SearchOutlined />}>
              Tra cứu
            </Button>
          }
          size="large"
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          onSearch={handleSearch}
          loading={isFetching}
          allowClear
          style={{ maxWidth: 600 }}
        />
      </Card>

      {/* Quick Stats (chỉ hiện khi có kết quả) */}
      {tickets.length > 0 && (
        <Row gutter={16} style={{ marginBottom: 24 }}>
          <Col span={6}>
            <Card style={{ borderRadius: 8 }}>
              <Statistic title="Tổng kết quả" value={ticketsData?.totalCount || 0} suffix="vé" />
            </Card>
          </Col>
          <Col span={6}>
            <Card style={{ borderRadius: 8 }}>
              <Statistic
                title="Chờ check-in"
                value={paidCount}
                valueStyle={{ color: '#52c41a' }}
                suffix="vé"
              />
            </Card>
          </Col>
          <Col span={6}>
            <Card style={{ borderRadius: 8 }}>
              <Statistic
                title="Đã check-in"
                value={checkedInCount}
                valueStyle={{ color: '#1677ff' }}
                suffix="vé"
              />
            </Card>
          </Col>
          <Col span={6}>
            <Card style={{ borderRadius: 8 }}>
              <Statistic
                title="Tổng doanh thu"
                value={totalRevenue}
                precision={0}
                valueStyle={{ color: '#722ed1' }}
                formatter={(val) => `${(Number(val) / 1000).toFixed(0)}K đ`}
              />
            </Card>
          </Col>
        </Row>
      )}

      {/* Results Table */}
      {activeQuery ? (
        <Card style={{ borderRadius: 12 }}>
          <Table
            columns={columns}
            dataSource={tickets}
            rowKey="bookingId"
            loading={isLoading}
            scroll={{ x: 1200 }}
            pagination={{
              current: pageNumber,
              pageSize: pageSize,
              total: ticketsData?.totalCount || 0,
              onChange: (page, size) => {
                setPageNumber(page);
                setPageSize(size);
              },
              showSizeChanger: true,
              showTotal: (total) => `Tổng ${total} vé`,
            }}
            rowClassName={(record) =>
              record.canCheckIn ? 'ticket-row-actionable' : ''
            }
          />
        </Card>
      ) : (
        <Card style={{ borderRadius: 12, textAlign: 'center', padding: 60 }}>
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description={
              <div>
                <p style={{ fontSize: 16, marginBottom: 8 }}>Nhập từ khóa để bắt đầu tra cứu</p>
                <Text type="secondary">
                  Hỗ trợ tìm kiếm theo: Mã vé (VD: INV-20260501...), Email (VD: user@email.com), SĐT (VD: 0901234567)
                </Text>
              </div>
            }
          />
        </Card>
      )}

      {/* Detail Modal */}
      <Modal
        title={
          <Space>
            <InfoCircleOutlined />
            Chi tiết vé: {selectedTicket?.ticketCode}
          </Space>
        }
        open={detailModalOpen}
        onCancel={() => setDetailModalOpen(false)}
        width={700}
        footer={
          selectedTicket?.canCheckIn ? (
            <Button
              type="primary"
              icon={<ScanOutlined />}
              size="large"
              block
              loading={checkInMutation.isPending}
              onClick={() => {
                handleCheckIn(selectedTicket);
                setDetailModalOpen(false);
              }}
            >
              Xác nhận Check-in
            </Button>
          ) : null
        }
      >
        {selectedTicket && (
          <div>
            {/* Status Banner */}
            <div
              style={{
                padding: '12px 16px',
                borderRadius: 8,
                marginBottom: 20,
                background:
                  selectedTicket.operationalStatus === 'Paid' ? '#f6ffed' :
                  selectedTicket.operationalStatus === 'CheckedIn' ? '#e6f4ff' :
                  '#f5f5f5',
                border: `1px solid ${
                  selectedTicket.operationalStatus === 'Paid' ? '#b7eb8f' :
                  selectedTicket.operationalStatus === 'CheckedIn' ? '#91caff' :
                  '#d9d9d9'
                }`,
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
              }}
            >
              <span>
                Trạng thái:{' '}
                <Tag color={getStatusConfig(selectedTicket.operationalStatus).color}>
                  {getStatusConfig(selectedTicket.operationalStatus).label}
                </Tag>
              </span>
              <Text strong style={{ fontSize: 18, color: '#1677ff' }}>
                {selectedTicket.totalPrice.toLocaleString()} đ
              </Text>
            </div>

            {/* Customer Info */}
            <Descriptions title="Thông tin khách hàng" bordered column={2} size="small" style={{ marginBottom: 20 }}>
              <Descriptions.Item label={<><UserOutlined /> Họ tên</>}>
                {selectedTicket.customerName || '—'}
              </Descriptions.Item>
              <Descriptions.Item label={<><PhoneOutlined /> Điện thoại</>}>
                {selectedTicket.customerPhone || '—'}
              </Descriptions.Item>
              <Descriptions.Item label={<><MailOutlined /> Email</>} span={2}>
                {selectedTicket.customerEmail || '—'}
              </Descriptions.Item>
            </Descriptions>

            {/* Showtime Info */}
            {selectedTicket.showtimeDetails && (
              <Descriptions title="Thông tin suất chiếu" bordered column={2} size="small" style={{ marginBottom: 20 }}>
                <Descriptions.Item label="Phim" span={2}>
                  <Text strong>{selectedTicket.showtimeDetails.movieTitle}</Text>
                </Descriptions.Item>
                <Descriptions.Item label="Giờ chiếu">
                  {toLocalDateTime(selectedTicket.showtimeDetails.startTime).format('HH:mm DD/MM/YYYY')}
                </Descriptions.Item>
                <Descriptions.Item label="Phòng chiếu">
                  {selectedTicket.showtimeDetails.cinemaHallName}
                </Descriptions.Item>
              </Descriptions>
            )}

            {/* Seats */}
            <Descriptions title="Ghế đã đặt" bordered column={1} size="small" style={{ marginBottom: 20 }}>
              <Descriptions.Item label="Danh sách ghế">
                <Space size={8} wrap>
                  {selectedTicket.seats.map(s => (
                    <Tag
                      key={s.seatId}
                      color={s.seatType === 'VIP' ? 'gold' : s.seatType === 'Couple' ? 'magenta' : 'blue'}
                      style={{ fontSize: 14, padding: '4px 12px' }}
                    >
                      {s.row}{s.number} ({s.seatType}) — {s.price.toLocaleString()}đ
                    </Tag>
                  ))}
                </Space>
              </Descriptions.Item>
            </Descriptions>

            {/* Timeline */}
            <Descriptions title="Lịch sử" bordered column={1} size="small">
              <Descriptions.Item label="Ngày đặt">
                {toLocalDateTime(selectedTicket.bookingDate).format('HH:mm:ss DD/MM/YYYY')}
              </Descriptions.Item>
              {selectedTicket.paidAt && (
                <Descriptions.Item label="Thanh toán lúc">
                  {toLocalDateTime(selectedTicket.paidAt).format('HH:mm:ss DD/MM/YYYY')}
                </Descriptions.Item>
              )}
              {selectedTicket.checkedInAt && (
                <Descriptions.Item label="Check-in lúc">
                  <Tag color="blue" icon={<CheckCircleOutlined />}>
                    {toLocalDateTime(selectedTicket.checkedInAt).format('HH:mm:ss DD/MM/YYYY')}
                  </Tag>
                </Descriptions.Item>
              )}
            </Descriptions>
          </div>
        )}
      </Modal>
    </div>
  );
};

export default TicketOperationsPage;
