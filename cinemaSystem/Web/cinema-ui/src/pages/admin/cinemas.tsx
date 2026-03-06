// Admin Cinemas Management Page
import { useState, useEffect } from 'react';
import { Table, Button, Space, Modal, Form, Input, message, Popconfirm, Tag } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import api from '../../shared/api/axios.instance';
import { Endpoints } from '../../shared/api/endpoints';

interface Cinema {
    id: string;
    name: string;
    address: string;
    city: string;
    totalScreens: number;
    isActive: boolean;
}

function AdminCinemasPage() {
    const [cinemas, setCinemas] = useState<Cinema[]>([]);
    const [loading, setLoading] = useState(false);
    const [modalOpen, setModalOpen] = useState(false);
    const [editingCinema, setEditingCinema] = useState<Cinema | null>(null);
    const [form] = Form.useForm();

    const fetchCinemas = async () => {
        setLoading(true);
        try {
            // Use public cinemas endpoint - backend doesn't have admin-specific list
            const response = await api.get(Endpoints.CINEMAS.BASE);
            setCinemas(response.data.data || response.data || []);
        } catch {
            message.error('Failed to load cinemas');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { fetchCinemas(); }, []);

    const handleAdd = () => {
        setEditingCinema(null);
        form.resetFields();
        setModalOpen(true);
    };

    const handleEdit = (record: Cinema) => {
        setEditingCinema(record);
        form.setFieldsValue(record);
        setModalOpen(true);
    };

    const handleDelete = async (id: string) => {
        try {
            await api.delete(Endpoints.ADMIN_CINEMAS.DETAIL(id));
            message.success('Cinema deleted successfully');
            fetchCinemas();
        } catch {
            message.error('Failed to delete cinema');
        }
    };

    const handleSubmit = async (values: unknown) => {
        try {
            if (editingCinema) {
                await api.put(Endpoints.ADMIN_CINEMAS.DETAIL(editingCinema.id), values);
                message.success('Cinema updated successfully');
            } else {
                await api.post(Endpoints.ADMIN_CINEMAS.CREATE, values);
                message.success('Cinema created successfully');
            }
            setModalOpen(false);
            fetchCinemas();
        } catch {
            message.error('Failed to save cinema');
        }
    };

    const columns: ColumnsType<Cinema> = [
        { title: 'Name', dataIndex: 'name', key: 'name', render: (text) => <strong>{text}</strong> },
        { title: 'Address', dataIndex: 'address', key: 'address' },
        { title: 'City', dataIndex: 'city', key: 'city' },
        { title: 'Screens', dataIndex: 'totalScreens', key: 'totalScreens' },
        {
            title: 'Status',
            dataIndex: 'isActive',
            key: 'isActive',
            render: (active) => <Tag color={active ? 'green' : 'red'}>{active ? 'Active' : 'Inactive'}</Tag>
        },
        {
            title: 'Actions',
            key: 'actions',
            render: (_, record) => (
                <Space>
                    <Button type="link" icon={<EditOutlined />} onClick={() => handleEdit(record)}>Edit</Button>
                    <Popconfirm title="Delete this cinema?" onConfirm={() => handleDelete(record.id)}>
                        <Button type="link" danger icon={<DeleteOutlined />}>Delete</Button>
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
                <h2>Quản lý rạp chiếu</h2>
                <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>Thêm rạp mới</Button>
            </div>
            <Table columns={columns} dataSource={cinemas} rowKey="id" loading={loading} pagination={{ pageSize: 10 }} />
            <Modal title={editingCinema ? 'Edit Cinema' : 'New Cinema'} open={modalOpen} onCancel={() => setModalOpen(false)} onOk={form.submit}>
                <Form form={form} layout="vertical" onFinish={handleSubmit}>
                    <Form.Item name="name" label="Name" rules={[{ required: true }]}><Input /></Form.Item>
                    <Form.Item name="address" label="Address"><Input /></Form.Item>
                    <Form.Item name="city" label="City"><Input /></Form.Item>
                </Form>
            </Modal>
        </div>
    );
}

export default AdminCinemasPage;
