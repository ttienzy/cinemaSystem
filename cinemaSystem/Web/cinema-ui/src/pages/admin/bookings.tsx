// Admin Bookings Management Page
import { useState, useEffect } from 'react';
import { Table, Tag, Button, Space, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import api from '../../shared/api/axios.instance';
import { Endpoints } from '../../shared/api/endpoints';

interface Booking {
    id: string;
    userEmail: string;
    movieTitle: string;
    cinemaName: string;
    showtime: string;
    totalAmount: number;
    status: string;
    createdAt: string;
}

function AdminBookingsPage() {
    const [bookings, setBookings] = useState<Booking[]>([]);
    const [cinemas, setCinemas] = useState<{ id: string; name: string }[]>([]);
    const [selectedCinemaId, setSelectedCinemaId] = useState<string>('');
    const [loading, setLoading] = useState(false);

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

    const fetchBookings = async () => {
        if (!selectedCinemaId) return;
        setLoading(true);
        try {
            // Backend requires cinemaId parameter
            const response = await api.get(Endpoints.MANAGER_BOOKINGS.BASE(selectedCinemaId));
            setBookings(response.data.data || []);
        } catch {
            message.error('Failed to load bookings');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { fetchCinemas(); }, []);
    useEffect(() => { if (selectedCinemaId) fetchBookings(); }, [selectedCinemaId]);

    const getStatusColor = (status: string) => {
        const colors: Record<string, string> = {
            'Pending': 'orange',
            'Confirmed': 'blue',
            'Completed': 'green',
            'Cancelled': 'red',
            'Refunded': 'purple',
        };
        return colors[status] || 'default';
    };

    const columns: ColumnsType<Booking> = [
        { title: 'ID', dataIndex: 'id', key: 'id', render: (id) => id.slice(0, 8) },
        { title: 'User', dataIndex: 'userEmail', key: 'userEmail' },
        { title: 'Movie', dataIndex: 'movieTitle', key: 'movieTitle', render: (t) => <strong>{t}</strong> },
        { title: 'Cinema', dataIndex: 'cinemaName', key: 'cinemaName' },
        { title: 'Showtime', dataIndex: 'showtime', key: 'showtime', render: (t) => new Date(t).toLocaleString('vi-VN') },
        { title: 'Amount', dataIndex: 'totalAmount', key: 'totalAmount', render: (a) => `${a.toLocaleString()} VND` },
        { title: 'Status', dataIndex: 'status', key: 'status', render: (s) => <Tag color={getStatusColor(s)}>{s}</Tag> },
        { title: 'Created', dataIndex: 'createdAt', key: 'createdAt', render: (d) => new Date(d).toLocaleDateString('vi-VN') },
    ];

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16, alignItems: 'center' }}>
                <h2>Quản lý đặt vé</h2>
                <Space>
                    <Select
                        style={{ width: 200 }}
                        placeholder="Chọn rạp"
                        value={selectedCinemaId}
                        onChange={setSelectedCinemaId}
                        options={cinemas.map(c => ({ label: c.name, value: c.id }))}
                    />
                    <Button onClick={fetchBookings}>Refresh</Button>
                </Space>
            </div>
            <Table columns={columns} dataSource={bookings} rowKey="id" loading={loading} pagination={{ pageSize: 10 }} />
        </div>
    );
}

export default AdminBookingsPage;
