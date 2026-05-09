import React, { useState } from 'react';
import {
  Table, Button, Space, Popconfirm, message, Tag, Image, Input, Select,
  Row, Col, Card, Statistic, Drawer, Form, InputNumber, DatePicker,
  Upload, Typography, Tooltip, Breadcrumb,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  PlusOutlined, SearchOutlined, DeleteOutlined, EditOutlined,
  EyeOutlined, VideoCameraOutlined, CalendarOutlined,
  UploadOutlined, FireOutlined, ClockCircleOutlined, InboxOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { movieApi, type MovieAdminListItem } from '../../../features/movies/api/movieApi';
import dayjs from '../../../utils/dayjs';

const { Title, Text } = Typography;
const { Search } = Input;

const STATUS_CONFIG: Record<string, { color: string; label: string }> = {
  Showing:    { color: 'green',   label: '🟢 Đang chiếu' },
  ComingSoon: { color: 'gold',    label: '🟡 Sắp chiếu' },
  Archived:   { color: 'default', label: '🔴 Ngừng chiếu' },
};

const MoviesPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [editingMovie, setEditingMovie] = useState<MovieAdminListItem | null>(null);
  const [form] = Form.useForm();

  // Data
  const { data: listRes, isLoading } = useQuery({
    queryKey: ['admin-movies', search, statusFilter, pageNumber, pageSize],
    queryFn: () => movieApi.getAdminList({ search, status: statusFilter, pageNumber, pageSize }),
  });
  const { data: summaryRes } = useQuery({
    queryKey: ['admin-movies-summary'],
    queryFn: () => movieApi.getAdminSummary(),
  });

  const movies = listRes?.data?.items || [];
  const summary = summaryRes?.data;

  // Mutations
  const deleteMutation = useMutation({
    mutationFn: movieApi.deleteMovie,
    onSuccess: (res) => {
      if (res.success) {
        message.success('Xóa phim thành công');
        queryClient.invalidateQueries({ queryKey: ['admin-movies'] });
        queryClient.invalidateQueries({ queryKey: ['admin-movies-summary'] });
      }
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: FormData) => movieApi.createMovie(data),
    onSuccess: (res) => {
      if (res.success) {
        message.success(res.message || 'Thêm phim thành công!');
        setDrawerOpen(false);
        form.resetFields();
        queryClient.invalidateQueries({ queryKey: ['admin-movies'] });
        queryClient.invalidateQueries({ queryKey: ['admin-movies-summary'] });
      }
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: FormData }) => movieApi.updateMovie(id, data),
    onSuccess: (res) => {
      if (res.success) {
        message.success(res.message || 'Cập nhật phim thành công!');
        setDrawerOpen(false);
        setEditingMovie(null);
        form.resetFields();
        queryClient.invalidateQueries({ queryKey: ['admin-movies'] });
      }
    },
  });

  // Handlers
  const openCreateDrawer = () => {
    setEditingMovie(null);
    form.resetFields();
    setDrawerOpen(true);
  };

  const openEditDrawer = (movie: MovieAdminListItem) => {
    setEditingMovie(movie);
    form.setFieldsValue({
      title: movie.title,
      description: movie.description,
      duration: movie.duration,
      language: movie.language,
      releaseDate: dayjs(movie.releaseDate),
    });
    setDrawerOpen(true);
  };

  const handleSubmit = async () => {
    const values = await form.validateFields();
    const fd = new FormData();
    fd.append('title', values.title);
    fd.append('duration', String(values.duration));
    fd.append('releaseDate', dayjs(values.releaseDate).toISOString());
    if (values.description) fd.append('description', values.description);
    if (values.language) fd.append('language', values.language);
    if (values.posterFile?.[0]?.originFileObj) {
      fd.append('posterFile', values.posterFile[0].originFileObj);
    }

    if (editingMovie) {
      updateMutation.mutate({ id: editingMovie.id, data: fd });
    } else {
      createMutation.mutate(fd);
    }
  };

  // Columns
  const columns: ColumnsType<MovieAdminListItem> = [
    {
      title: 'Poster',
      dataIndex: 'posterUrl',
      key: 'poster',
      width: 80,
      render: (url: string) => (
        <Image
          src={url || 'https://via.placeholder.com/60x90?text=No'}
          alt="poster"
          width={50} height={70}
          style={{ borderRadius: 4, objectFit: 'cover' }}
          fallback="https://via.placeholder.com/60x90?text=Err"
          preview={{ mask: <EyeOutlined /> }}
        />
      ),
    },
    {
      title: 'Tên phim',
      dataIndex: 'title',
      key: 'title',
      render: (title: string, record) => (
        <div>
          <Text strong>{title}</Text>
          {record.genres?.length > 0 && (
            <div style={{ marginTop: 4 }}>
              {record.genres.map(g => <Tag key={g.id} style={{ fontSize: 11 }}>{g.name}</Tag>)}
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      width: 130,
      render: (status: string) => {
        const cfg = STATUS_CONFIG[status] || { color: 'default', label: status };
        return <Tag color={cfg.color}>{cfg.label}</Tag>;
      },
    },
    {
      title: 'Thời lượng',
      dataIndex: 'duration',
      key: 'duration',
      width: 100,
      render: (val: number) => <><ClockCircleOutlined /> {val} phút</>,
    },
    {
      title: 'Ngày chiếu',
      dataIndex: 'releaseDate',
      key: 'releaseDate',
      width: 120,
      render: (d: string) => dayjs(d).format('DD/MM/YYYY'),
    },
    {
      title: 'Suất chiếu',
      key: 'showtimes',
      width: 120,
      render: (_, r) => (
        <Tooltip title={`${r.upcomingShowtimesCount} suất sắp tới / ${r.totalShowtimes} tổng`}>
          <Tag color="blue">{r.upcomingShowtimesCount} sắp tới</Tag>
          <div style={{ fontSize: 11, color: '#8c8c8c' }}>Tổng: {r.totalShowtimes}</div>
        </Tooltip>
      ),
    },
    {
      title: 'Hành động',
      key: 'action',
      width: 140,
      fixed: 'right',
      render: (_, record) => (
        <Space size={4}>
          <Tooltip title="Sửa">
            <Button type="text" icon={<EditOutlined />} onClick={() => openEditDrawer(record)} />
          </Tooltip>
          <Popconfirm title="Xóa phim này?" onConfirm={() => deleteMutation.mutate(record.id)}>
            <Tooltip title="Xóa">
              <Button type="text" danger icon={<DeleteOutlined />} />
            </Tooltip>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Breadcrumb items={[{ title: 'Admin' }, { title: 'Quản lý Phim' }]} style={{ marginBottom: 16 }} />

      {/* Summary Cards */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}><Card style={{ borderRadius: 8 }}><Statistic title="Tổng số phim" value={summary?.totalMovies || 0} prefix={<VideoCameraOutlined />} /></Card></Col>
        <Col span={6}><Card style={{ borderRadius: 8 }}><Statistic title="Đang chiếu" value={summary?.showingMovies || 0} valueStyle={{ color: '#52c41a' }} prefix={<FireOutlined />} /></Card></Col>
        <Col span={6}><Card style={{ borderRadius: 8 }}><Statistic title="Sắp chiếu" value={summary?.comingSoonMovies || 0} valueStyle={{ color: '#faad14' }} prefix={<CalendarOutlined />} /></Card></Col>
        <Col span={6}><Card style={{ borderRadius: 8 }}><Statistic title="Ngừng chiếu" value={summary?.archivedMovies || 0} valueStyle={{ color: '#8c8c8c' }} prefix={<InboxOutlined />} /></Card></Col>
      </Row>

      {/* Toolbar */}
      <Card style={{ marginBottom: 16, borderRadius: 12 }}>
        <Row gutter={16} align="middle">
          <Col flex="auto">
            <Space>
              <Search placeholder="Tìm tên phim..." allowClear style={{ width: 260 }}
                onSearch={(v) => { setSearch(v); setPageNumber(1); }} />
              <Select placeholder="Trạng thái" allowClear style={{ width: 160 }}
                value={statusFilter} onChange={(v) => { setStatusFilter(v); setPageNumber(1); }}
                options={[
                  { label: '🟢 Đang chiếu', value: 'Showing' },
                  { label: '🟡 Sắp chiếu', value: 'ComingSoon' },
                  { label: '🔴 Ngừng chiếu', value: 'Archived' },
                ]}
              />
            </Space>
          </Col>
          <Col>
            <Button type="primary" icon={<PlusOutlined />} onClick={openCreateDrawer}>Thêm phim mới</Button>
          </Col>
        </Row>
      </Card>

      {/* Table */}
      <Card style={{ borderRadius: 12 }}>
        <Table columns={columns} dataSource={movies} rowKey="id" loading={isLoading}
          scroll={{ x: 1000 }}
          pagination={{
            current: pageNumber, pageSize,
            total: listRes?.data?.totalCount || 0,
            onChange: (p, s) => { setPageNumber(p); setPageSize(s); },
            showSizeChanger: true,
            showTotal: (t) => `Tổng ${t} phim`,
          }}
        />
      </Card>

      {/* Create/Edit Drawer */}
      <Drawer
        title={editingMovie ? `Sửa phim: ${editingMovie.title}` : 'Thêm phim mới'}
        width={520}
        open={drawerOpen}
        onClose={() => { setDrawerOpen(false); setEditingMovie(null); }}
        extra={
          <Button type="primary" onClick={handleSubmit}
            loading={createMutation.isPending || updateMutation.isPending}>
            {editingMovie ? 'Cập nhật' : 'Tạo phim'}
          </Button>
        }
      >
        <Form form={form} layout="vertical">
          <Form.Item name="title" label="Tên phim" rules={[{ required: true, message: 'Vui lòng nhập tên phim' }]}>
            <Input placeholder="Nhập tên phim" />
          </Form.Item>
          <Form.Item name="description" label="Mô tả">
            <Input.TextArea rows={3} placeholder="Mô tả nội dung phim" />
          </Form.Item>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="duration" label="Thời lượng (phút)" rules={[{ required: true }]}>
                <InputNumber min={1} max={500} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="language" label="Ngôn ngữ">
                <Input placeholder="VD: Phụ đề, Lồng tiếng" />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="releaseDate" label="Ngày khởi chiếu" rules={[{ required: true }]}>
            <DatePicker format="DD/MM/YYYY" style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="posterFile" label="Poster" valuePropName="fileList"
            getValueFromEvent={(e) => (Array.isArray(e) ? e : e?.fileList)}>
            <Upload listType="picture-card" maxCount={1} beforeUpload={() => false} accept="image/*">
              <div><UploadOutlined /><div style={{ marginTop: 8 }}>Tải ảnh</div></div>
            </Upload>
          </Form.Item>
          {editingMovie?.posterUrl && (
            <div style={{ marginBottom: 16 }}>
              <Text type="secondary">Poster hiện tại:</Text>
              <Image src={editingMovie.posterUrl} width={100} style={{ borderRadius: 6, marginTop: 8 }} />
            </div>
          )}
        </Form>
      </Drawer>
    </div>
  );
};

export default MoviesPage;
