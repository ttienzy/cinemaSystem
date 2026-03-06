// Admin Users Management Page
import { useState, useEffect } from 'react';
import { Table, Tag, Button, Space, Modal, message, Popconfirm } from 'antd';
import { EditOutlined, LockOutlined, UnlockOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import api from '../../shared/api/axios.instance';
import { Endpoints } from '../../shared/api/endpoints';

interface User {
    id: string;
    email: string;
    username: string;
    firstName: string;
    lastName: string;
    roles: string[];
    emailConfirmed: boolean;
    isLocked: boolean;
    createdAt: string;
}

function AdminUsersPage() {
    const [users, setUsers] = useState<User[]>([]);
    const [loading, setLoading] = useState(false);

    const fetchUsers = async () => {
        setLoading(true);
        try {
            const response = await api.get(Endpoints.ADMIN_USERS.BASE);
            setUsers(response.data.data || []);
        } catch {
            message.error('Failed to load users');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { fetchUsers(); }, []);

    const handleLock = async (userId: string) => {
        try {
            // Backend expects PUT method, not POST
            await api.put(Endpoints.ADMIN_USERS.LOCK(userId));
            message.success('User locked');
            fetchUsers();
        } catch {
            message.error('Failed to lock user');
        }
    };

    const handleUnlock = async (userId: string) => {
        try {
            // Backend expects PUT method, not POST
            await api.put(Endpoints.ADMIN_USERS.UNLOCK(userId));
            message.success('User unlocked');
            fetchUsers();
        } catch {
            message.error('Failed to unlock user');
        }
    };

    const columns: ColumnsType<User> = [
        { title: 'Username', dataIndex: 'username', key: 'username', render: (t) => <strong>{t}</strong> },
        { title: 'Email', dataIndex: 'email', key: 'email' },
        { title: 'Name', key: 'name', render: (_, r) => `${r.firstName} ${r.lastName}` },
        {
            title: 'Roles',
            dataIndex: 'roles',
            key: 'roles',
            render: (roles) => roles.map((role: string) => (
                <Tag key={role} color={role === 'Admin' ? 'red' : role === 'Manager' ? 'orange' : 'blue'}>{role}</Tag>
            ))
        },
        {
            title: 'Status',
            key: 'status',
            render: (_, r) => (
                <Space>
                    <Tag color={r.emailConfirmed ? 'green' : 'orange'}>{r.emailConfirmed ? 'Verified' : 'Unverified'}</Tag>
                    {r.isLocked && <Tag color="red">Locked</Tag>}
                </Space>
            )
        },
        { title: 'Created', dataIndex: 'createdAt', key: 'createdAt', render: (d) => new Date(d).toLocaleDateString('vi-VN') },
        {
            title: 'Actions',
            key: 'actions',
            render: (_, record) => (
                <Space>
                    <Button type="link" icon={<EditOutlined />}>Edit</Button>
                    {record.isLocked ? (
                        <Popconfirm title="Unlock this user?" onConfirm={() => handleUnlock(record.id)}>
                            <Button type="link" icon={<UnlockOutlined />}>Unlock</Button>
                        </Popconfirm>
                    ) : (
                        <Popconfirm title="Lock this user?" onConfirm={() => handleLock(record.id)}>
                            <Button type="link" danger icon={<LockOutlined />}>Lock</Button>
                        </Popconfirm>
                    )}
                </Space>
            ),
        },
    ];

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
                <h2>Quản lý người dùng</h2>
                <Button onClick={fetchUsers}>Refresh</Button>
            </div>
            <Table columns={columns} dataSource={users} rowKey="id" loading={loading} pagination={{ pageSize: 10 }} />
        </div>
    );
}

export default AdminUsersPage;
