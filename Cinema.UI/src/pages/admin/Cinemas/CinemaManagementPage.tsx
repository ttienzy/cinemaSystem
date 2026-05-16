import React, { useState } from 'react';
import {
    Table, Button, Modal, Form, Input, Select, message, Popconfirm, Space,
    Card, Row, Col, Statistic, Breadcrumb, Tag, Badge,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
    BankOutlined, PlusOutlined, EditOutlined, DeleteOutlined,
    CheckCircleOutlined, CloseCircleOutlined, AppstoreOutlined, HomeOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
    cinemaApi,
    type Cinema,
} from '../../../features/cinemas/api/cinemaApi';

const { Search } = Input;

const CinemaManagementPage: React.FC = () => {
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingCinema, setEditingCinema] = useState<Cinema | null>(null);
    const [search, setSearch] = useState('');
    const [statusFilter, setStatusFilter] = useState<string | undefined>();
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [form] = Form.useForm();
    const queryClient = useQueryClient();
    const navigate = useNavigate();

    // Queries
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

    // Mutations
    const createMutation = useMutation({
        mutationFn: cinemaApi.createCinema,
        onSuccess: () => {
            message.success('Tạo cụm rạp thành công!');
            queryClient.invalidateQueries({ queryKey: ['admin-cinemas'] });
            queryClient.invalidateQueries({ queryKey: ['admin-cinemas-summary'] });
            handleCloseModal();
        },
        onError: () => message.error('Không thể tạo cụm rạp'),
    });

    const updateMutation = useMutation({
        mutationFn: ({ id, data }: { id: string; data: any }) => cinemaApi.updateCinema(id, data),
        onSuccess: () => {
            message.success('Cập nhật cụm rạp thành công!');
            queryClient.invalidateQueries({ queryKey: ['admin-cinemas'] });
            handleCloseModal();
        },
        onError: () => message.error('Không thể cập nhật cụm rạp'),
    });

    const deleteMutation = useMutation({
        mutationFn: cinemaApi.deleteCinema,
        onSuccess: () => {
            message.success('Xóa cụm rạp thành công!');
            queryClient.invalidateQueries({ queryKey: ['admin-cinemas'] });
            queryClient.invalidateQueries({ queryKey: ['admin-cinemas-summary'] });
        },
        onError: () => message.error('Không thể xóa cụm rạp'),
    });

    // Handlers
    const handleOpenModal = (cinema?: Cinema) => {
        if (cinema) {
            setEditingCinema(cinema);
            form.setFieldsValue(cinema);
        } else {
            setEditingCinema(null);
            form.resetFields();
        }
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
        setEditingCinema(null);
        form.resetFields();
    };

    const handleSubmit = async () => {
        try {
            const values = await form.validateFields();
            if (editingCinema) {
                updateMutation.mutate({ id: editingCinema.id, data: values });
            } else {
                createMutation.mutate(values);
            }
        } catch (error) {
            console.error('Validation failed:', error);
        }
    };

    const handleDelete = (id: string) => {
        deleteMutation.mutate(id);
    };

    const handleManageHalls = (cinemaId: string) => {
        navigate(`/admin/cinemas/${cinemaId}/halls`);
    };

    // Columns
    const columns: ColumnsType<Cinema> = [
        {
            title: 'Cụm rạp',
            dataIndex: 'name',
            key: 'name',
            render: (name: string, record) => (
                <div>
                    <div style={{ fontWeight: 600, fontSize: 14 }}>
                        <BankOutlined style={{ marginRight: 6, color: '#1677ff' }} />
                        {name}
                    </div>
                    <div style={{ fontSize: 12, color: '#8c8c8c', marginTop: 2 }}>{record.address}</div>
                    {record.city && <Tag style={{ marginTop: 4, fontSize: 11 }}>{record.city}</Tag>}
                </div>
            ),
        },
        {
            title: 'Trạng thái',
            dataIndex: 'status',
            key: 'status',
            width: 140,
            render: (status: string) =>
                status === 'Active' ? (
                    <Badge status="success" text={<Tag color="success">Hoạt động</Tag>} />
                ) : (
                    <Badge status="error" text={<Tag color="error">Tạm đóng</Tag>} />
                ),
        },
        {
            title: 'Phòng chiếu',
            dataIndex: 'totalHalls',
            key: 'totalHalls',
            width: 120,
            render: (v: number) => (
                <>
                    <AppstoreOutlined style={{ marginRight: 4 }} />
                    {v} phòng
                </>
            ),
        },
        {
            title: 'Tổng ghế',
            dataIndex: 'totalSeats',
            key: 'totalSeats',
            width: 120,
            render: (v: number) => <span style={{ fontWeight: 600 }}>{v.toLocaleString()}</span>,
        },
        {
            title: 'Thao tác',
            key: 'actions',
            width: 280,
            render: (_, record) => (
                <Space>
                    <Button
                        type="primary"
                        icon={<AppstoreOutlined />}
                        onClick={() => handleManageHalls(record.id)}
                    >
                        Quản lý phòng
                    </Button>
                    <Button
                        icon={<EditOutlined />}
                        onClick={() => handleOpenModal(record)}
                    >
                        Sửa
                    </Button>
                    <Popconfirm
                        title="Xóa cụm rạp?"
                        description="Bạn có chắc muốn xóa cụm rạp này?"
                        onConfirm={() => handleDelete(record.id)}
                        okText="Xóa"
                        cancelText="Hủy"
                    >
                        <Button danger icon={<DeleteOutlined />}>
                            Xóa
                        </Button>
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    return (
        <div>
            <Breadcrumb
                items={[{ title: 'Admin' }, { title: 'Quản lý Cụm rạp' }]}
                style={{ marginBottom: 16 }}
            />

            {/* Summary */}
            <Row gutter={16} style={{ marginBottom: 24 }}>
                <Col span={5}>
                    <Card style={{ borderRadius: 8 }}>
                        <Statistic
                            title="Tổng cụm rạp"
                            value={summary?.totalCinemas || 0}
                            prefix={<BankOutlined />}
                        />
                    </Card>
                </Col>
                <Col span={5}>
                    <Card style={{ borderRadius: 8 }}>
                        <Statistic
                            title="Đang hoạt động"
                            value={summary?.activeCinemas || 0}
                            valueStyle={{ color: '#52c41a' }}
                            prefix={<CheckCircleOutlined />}
                        />
                    </Card>
                </Col>
                <Col span={5}>
                    <Card style={{ borderRadius: 8 }}>
                        <Statistic
                            title="Tạm đóng"
                            value={summary?.inactiveCinemas || 0}
                            valueStyle={{ color: '#ff4d4f' }}
                            prefix={<CloseCircleOutlined />}
                        />
                    </Card>
                </Col>
                <Col span={5}>
                    <Card style={{ borderRadius: 8 }}>
                        <Statistic
                            title="Tổng phòng chiếu"
                            value={summary?.totalHalls || 0}
                            prefix={<AppstoreOutlined />}
                        />
                    </Card>
                </Col>
                <Col span={4}>
                    <Card style={{ borderRadius: 8 }}>
                        <Statistic
                            title="Tổng ghế"
                            value={summary?.totalSeats || 0}
                            prefix={<HomeOutlined />}
                        />
                    </Card>
                </Col>
            </Row>

            {/* Toolbar */}
            <Card style={{ marginBottom: 16, borderRadius: 12 }}>
                <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                    <Space>
                        <Search
                            placeholder="Tìm cụm rạp..."
                            allowClear
                            style={{ width: 260 }}
                            onSearch={(v) => {
                                setSearch(v);
                                setPageNumber(1);
                            }}
                        />
                        <Select
                            placeholder="Trạng thái"
                            allowClear
                            style={{ width: 160 }}
                            value={statusFilter}
                            onChange={(v) => {
                                setStatusFilter(v);
                                setPageNumber(1);
                            }}
                            options={[
                                { label: '🟢 Hoạt động', value: 'Active' },
                                { label: '🔴 Tạm đóng', value: 'Inactive' },
                            ]}
                        />
                    </Space>
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={() => handleOpenModal()}
                    >
                        Thêm cụm rạp
                    </Button>
                </Space>
            </Card>

            {/* Table */}
            <Card style={{ borderRadius: 12 }}>
                <Table
                    columns={columns}
                    dataSource={cinemas}
                    rowKey="id"
                    loading={isLoading}
                    pagination={{
                        current: pageNumber,
                        pageSize,
                        total: overviewRes?.data?.totalCount || 0,
                        onChange: (p, s) => {
                            setPageNumber(p);
                            setPageSize(s);
                        },
                        showSizeChanger: true,
                    }}
                />
            </Card>

            {/* Modal */}
            <Modal
                title={editingCinema ? 'Sửa cụm rạp' : 'Thêm cụm rạp mới'}
                open={isModalOpen}
                onOk={handleSubmit}
                onCancel={handleCloseModal}
                confirmLoading={createMutation.isPending || updateMutation.isPending}
                okText={editingCinema ? 'Cập nhật' : 'Tạo mới'}
                cancelText="Hủy"
                width={600}
            >
                <Form form={form} layout="vertical" style={{ marginTop: 24 }}>
                    <Form.Item
                        name="name"
                        label="Tên cụm rạp"
                        rules={[{ required: true, message: 'Vui lòng nhập tên cụm rạp' }]}
                    >
                        <Input placeholder="VD: CGV Vincom Center" />
                    </Form.Item>
                    <Form.Item
                        name="address"
                        label="Địa chỉ"
                        rules={[{ required: true, message: 'Vui lòng nhập địa chỉ' }]}
                    >
                        <Input placeholder="VD: 72 Lê Thánh Tôn, Quận 1" />
                    </Form.Item>
                    <Form.Item name="city" label="Thành phố">
                        <Input placeholder="VD: Hồ Chí Minh" />
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
};

export default CinemaManagementPage;
