import React, { useState } from 'react';
import {
    Table, Button, Modal, Form, Input, message, Popconfirm, Space,
    Card, Typography, Breadcrumb, Tag, Spin, Alert,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
    PlusOutlined, EditOutlined, DeleteOutlined, AppstoreOutlined,
    ArrowLeftOutlined, SettingOutlined, CheckCircleOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import {
    cinemaApi,
    type CinemaHall,
} from '../../../features/cinemas/api/cinemaApi';

const { Title, Text } = Typography;

const CinemaHallManagementPage: React.FC = () => {
    const { cinemaId } = useParams<{ cinemaId: string }>();
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingHall, setEditingHall] = useState<CinemaHall | null>(null);
    const [form] = Form.useForm();
    const queryClient = useQueryClient();
    const navigate = useNavigate();

    // Queries
    const { data: cinemaRes, isLoading: cinemaLoading } = useQuery({
        queryKey: ['cinema', cinemaId],
        queryFn: () => cinemaApi.getCinemaById(cinemaId!),
        enabled: !!cinemaId,
    });

    const { data: hallsRes, isLoading: hallsLoading } = useQuery({
        queryKey: ['cinema-halls', cinemaId],
        queryFn: () => cinemaApi.getHallsByCinemaId(cinemaId!),
        enabled: !!cinemaId,
    });

    const cinema = cinemaRes?.data;
    const halls = hallsRes?.data || [];

    // Mutations
    const createMutation = useMutation({
        mutationFn: cinemaApi.createHall,
        onSuccess: () => {
            message.success('Tạo phòng chiếu thành công!');
            queryClient.invalidateQueries({ queryKey: ['cinema-halls', cinemaId] });
            queryClient.invalidateQueries({ queryKey: ['admin-cinemas'] });
            handleCloseModal();
        },
        onError: () => message.error('Không thể tạo phòng chiếu'),
    });

    const updateMutation = useMutation({
        mutationFn: ({ id, data }: { id: string; data: any }) => cinemaApi.updateHall(id, data),
        onSuccess: () => {
            message.success('Cập nhật phòng chiếu thành công!');
            queryClient.invalidateQueries({ queryKey: ['cinema-halls', cinemaId] });
            handleCloseModal();
        },
        onError: () => message.error('Không thể cập nhật phòng chiếu'),
    });

    const deleteMutation = useMutation({
        mutationFn: cinemaApi.deleteHall,
        onSuccess: () => {
            message.success('Xóa phòng chiếu thành công!');
            queryClient.invalidateQueries({ queryKey: ['cinema-halls', cinemaId] });
            queryClient.invalidateQueries({ queryKey: ['admin-cinemas'] });
        },
        onError: (error: any) => {
            const errorMsg = error?.response?.data?.message || 'Không thể xóa phòng chiếu';
            message.error(errorMsg);
        },
    });

    // Handlers
    const handleOpenModal = (hall?: CinemaHall) => {
        if (hall) {
            setEditingHall(hall);
            form.setFieldsValue({ name: hall.name });
        } else {
            setEditingHall(null);
            form.resetFields();
        }
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
        setEditingHall(null);
        form.resetFields();
    };

    const handleSubmit = async () => {
        try {
            const values = await form.validateFields();
            if (editingHall) {
                updateMutation.mutate({ id: editingHall.id, data: values });
            } else {
                createMutation.mutate({ cinemaId: cinemaId!, ...values });
            }
        } catch (error) {
            console.error('Validation failed:', error);
        }
    };

    const handleDelete = (id: string) => {
        deleteMutation.mutate(id);
    };

    const handleManageSeats = (hallId: string) => {
        navigate(`/admin/halls/${hallId}/seats`);
    };

    // Columns
    const columns: ColumnsType<CinemaHall> = [
        {
            title: 'Phòng chiếu',
            dataIndex: 'name',
            key: 'name',
            render: (name: string) => (
                <Text strong style={{ fontSize: 14 }}>
                    <AppstoreOutlined style={{ marginRight: 6, color: '#1677ff' }} />
                    {name}
                </Text>
            ),
        },
        {
            title: 'Tổng ghế',
            dataIndex: 'totalSeats',
            key: 'totalSeats',
            width: 120,
            render: (v: number) => <span style={{ fontWeight: 600 }}>{v}</span>,
        },
        {
            title: 'Sơ đồ ghế',
            dataIndex: 'seatMapConfigured',
            key: 'seatMapConfigured',
            width: 150,
            render: (configured: boolean) =>
                configured ? (
                    <Tag color="success" icon={<CheckCircleOutlined />}>
                        Đã cấu hình
                    </Tag>
                ) : (
                    <Tag color="warning" icon={<SettingOutlined />}>
                        Chưa cấu hình
                    </Tag>
                ),
        },
        {
            title: 'Thao tác',
            key: 'actions',
            width: 280,
            render: (_, record) => (
                <Space>
                    <Button
                        type="primary"
                        icon={<SettingOutlined />}
                        onClick={() => handleManageSeats(record.id)}
                    >
                        Quản lý ghế
                    </Button>
                    <Button icon={<EditOutlined />} onClick={() => handleOpenModal(record)}>
                        Sửa
                    </Button>
                    <Popconfirm
                        title="Xóa phòng chiếu?"
                        description="Bạn có chắc muốn xóa phòng chiếu này? Phải xóa hết ghế trước."
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

    if (cinemaLoading) {
        return (
            <div style={{ textAlign: 'center', padding: 50 }}>
                <Spin size="large" />
            </div>
        );
    }

    if (!cinema) {
        return (
            <Alert
                message="Không tìm thấy cụm rạp"
                type="error"
                showIcon
                action={
                    <Button onClick={() => navigate('/admin/cinemas')}>Quay lại</Button>
                }
            />
        );
    }

    return (
        <div>
            <Breadcrumb
                items={[
                    { title: 'Admin' },
                    { title: 'Cụm rạp', onClick: () => navigate('/admin/cinemas') },
                    { title: cinema.name },
                ]}
                style={{ marginBottom: 16 }}
            />

            {/* Header */}
            <Card style={{ marginBottom: 16, borderRadius: 12 }}>
                <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                    <div>
                        <Button
                            icon={<ArrowLeftOutlined />}
                            onClick={() => navigate('/admin/cinemas')}
                            style={{ marginRight: 16 }}
                        >
                            Quay lại
                        </Button>
                        <Title level={4} style={{ display: 'inline', margin: 0 }}>
                            Quản lý phòng chiếu - {cinema.name}
                        </Title>
                    </div>
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={() => handleOpenModal()}
                    >
                        Thêm phòng chiếu
                    </Button>
                </Space>
            </Card>

            {/* Table */}
            <Card style={{ borderRadius: 12 }}>
                <Table
                    columns={columns}
                    dataSource={halls}
                    rowKey="id"
                    loading={hallsLoading}
                    pagination={false}
                />
            </Card>

            {/* Modal */}
            <Modal
                title={editingHall ? 'Sửa phòng chiếu' : 'Thêm phòng chiếu mới'}
                open={isModalOpen}
                onOk={handleSubmit}
                onCancel={handleCloseModal}
                confirmLoading={createMutation.isPending || updateMutation.isPending}
                okText={editingHall ? 'Cập nhật' : 'Tạo mới'}
                cancelText="Hủy"
            >
                <Form form={form} layout="vertical" style={{ marginTop: 24 }}>
                    <Form.Item
                        name="name"
                        label="Tên phòng chiếu"
                        rules={[{ required: true, message: 'Vui lòng nhập tên phòng chiếu' }]}
                    >
                        <Input placeholder="VD: Phòng 1, IMAX Hall, VIP Room" />
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
};

export default CinemaHallManagementPage;
