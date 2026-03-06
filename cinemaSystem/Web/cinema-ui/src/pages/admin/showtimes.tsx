// Admin Showtimes Management Page
import { useState, useEffect } from 'react';
import { Table, Button, Space, Modal, Form, Input, Select, DatePicker, message } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import api from '../../shared/api/axios.instance';
import { Endpoints } from '../../shared/api/endpoints';

interface Showtime {
    id: string;
    movieTitle: string;
    cinemaName: string;
    screenName: string;
    startTime: string;
    endTime: string;
    price: number;
    status: string;
}

function AdminShowtimesPage() {
    const [showtimes, setShowtimes] = useState<Showtime[]>([]);
    const [cinemas, setCinemas] = useState<{ id: string; name: string }[]>([]);
    const [selectedCinemaId, setSelectedCinemaId] = useState<string>('');
    const [loading, setLoading] = useState(false);
    const [modalOpen, setModalOpen] = useState(false);
    const [form] = Form.useForm();

    // Fetch cinemas for dropdown
    const fetchCinemas = async () => {
        try {
            const response = await api.get(Endpoints.CINEMAS.BASE);
            const cinemaList = response.data.data || response.data || [];
            setCinemas(cinemaList);
            if (cinemaList.length > 0 && !selectedCinemaId) {
                setSelectedCinemaId(cinemaList[0].id);
            }
        } catch {
            message.error('Failed to load cinemas');
        }
    };

    const fetchShowtimes = async () => {
        if (!selectedCinemaId) return;
        setLoading(true);
        try {
            // Backend requires cinemaId parameter
            const response = await api.get(Endpoints.MANAGER_SHOWTIMES.BY_CINEMA(selectedCinemaId));
            setShowtimes(response.data.data || []);
        } catch {
            message.error('Failed to load showtimes');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { fetchCinemas(); }, []);
    useEffect(() => { if (selectedCinemaId) fetchShowtimes(); }, [selectedCinemaId]);

    const handleSubmit = async (values: unknown) => {
        try {
            await api.post(Endpoints.MANAGER_SHOWTIMES.BULK, values);
            message.success('Showtime created successfully');
            setModalOpen(false);
            fetchShowtimes();
        } catch {
            message.error('Failed to create showtime');
        }
    };

    const columns: ColumnsType<Showtime> = [
        { title: 'Movie', dataIndex: 'movieTitle', key: 'movieTitle', render: (text) => <strong>{text}</strong> },
        { title: 'Cinema', dataIndex: 'cinemaName', key: 'cinemaName' },
        { title: 'Screen', dataIndex: 'screenName', key: 'screenName' },
        { title: 'Start Time', dataIndex: 'startTime', key: 'startTime', render: (t) => new Date(t).toLocaleString('vi-VN') },
        { title: 'End Time', dataIndex: 'endTime', key: 'endTime', render: (t) => new Date(t).toLocaleString('vi-VN') },
        { title: 'Price', dataIndex: 'price', key: 'price', render: (p) => `${p.toLocaleString()} VND` },
    ];

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16, alignItems: 'center' }}>
                <h2>Quản lý suất chiếu</h2>
                <Space>
                    <Select
                        style={{ width: 200 }}
                        placeholder="Chọn rạp"
                        value={selectedCinemaId}
                        onChange={setSelectedCinemaId}
                        options={cinemas.map(c => ({ label: c.name, value: c.id }))}
                    />
                    <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalOpen(true)}>Thêm suất chiếu</Button>
                </Space>
            </div>
            <Table columns={columns} dataSource={showtimes} rowKey="id" loading={loading} pagination={{ pageSize: 10 }} />
            <Modal title="New Showtime" open={modalOpen} onCancel={() => setModalOpen(false)} onOk={form.submit}>
                <Form form={form} layout="vertical" onFinish={handleSubmit}>
                    <Form.Item name="movieId" label="Movie" rules={[{ required: true }]}><Input /></Form.Item>
                    <Form.Item name="cinemaId" label="Cinema" rules={[{ required: true }]}><Input /></Form.Item>
                    <Form.Item name="screenId" label="Screen" rules={[{ required: true }]}><Input /></Form.Item>
                    <Form.Item name="startTime" label="Start Time" rules={[{ required: true }]}><DatePicker showTime style={{ width: '100%' }} /></Form.Item>
                    <Form.Item name="price" label="Price" rules={[{ required: true }]}><Input type="number" /></Form.Item>
                </Form>
            </Modal>
        </div>
    );
}

export default AdminShowtimesPage;
