// Admin Movies Management Page
import { useState } from 'react';
import {
    Table, Button, Space, Modal, Form, Input,
    DatePicker, message, Popconfirm, Tag
} from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useAdminMovies, useCreateMovie, useUpdateMovie, useDeleteMovie } from '../../features/admin/movies/hooks/useAdminMovies';
import type { MovieListItem, CreateMovieRequest } from '../../features/admin/movies/api/adminMoviesApi';

function AdminMoviesPage() {
    const [modalOpen, setModalOpen] = useState(false);
    const [editingMovie, setEditingMovie] = useState<MovieListItem | null>(null);
    const [form] = Form.useForm();

    // Use React Query hooks for data fetching and mutations
    const { data, isLoading } = useAdminMovies();
    const createMovie = useCreateMovie();
    const updateMovie = useUpdateMovie();
    const deleteMovie = useDeleteMovie();

    const movies: MovieListItem[] = data?.data || [];

    const handleAdd = () => {
        setEditingMovie(null);
        form.resetFields();
        setModalOpen(true);
    };

    const handleEdit = (record: MovieListItem) => {
        setEditingMovie(record);
        form.setFieldsValue(record);
        setModalOpen(true);
    };

    const handleDelete = async (id: string) => {
        try {
            await deleteMovie.mutateAsync(id);
            message.success('Movie deleted successfully');
        } catch {
            message.error('Failed to delete movie');
        }
    };

    const handleSubmit = async (values: CreateMovieRequest) => {
        try {
            if (editingMovie) {
                await updateMovie.mutateAsync({ id: editingMovie.id, data: values });
                message.success('Movie updated successfully');
            } else {
                await createMovie.mutateAsync(values);
                message.success('Movie created successfully');
            }
            setModalOpen(false);
            form.resetFields();
        } catch {
            message.error('Failed to save movie');
        }
    };

    const columns: ColumnsType<MovieListItem> = [
        {
            title: 'Title',
            dataIndex: 'title',
            key: 'title',
            render: (text) => <strong>{text}</strong>,
        },
        {
            title: 'Genre',
            dataIndex: 'genre',
            key: 'genre',
        },
        {
            title: 'Duration',
            dataIndex: 'duration',
            key: 'duration',
            render: (min) => `${min} min`,
        },
        {
            title: 'Release Date',
            dataIndex: 'releaseDate',
            key: 'releaseDate',
            render: (date) => new Date(date).toLocaleDateString('vi-VN'),
        },
        {
            title: 'Status',
            dataIndex: 'status',
            key: 'status',
            render: (status) => (
                <Tag color={status === 'NowShowing' ? 'green' : 'blue'}>
                    {status}
                </Tag>
            ),
        },
        {
            title: 'Actions',
            key: 'actions',
            render: (_, record) => (
                <Space>
                    <Button
                        type="link"
                        icon={<EditOutlined />}
                        onClick={() => handleEdit(record)}
                    >
                        Edit
                    </Button>
                    <Popconfirm
                        title="Are you sure you want to delete this movie?"
                        onConfirm={() => handleDelete(record.id)}
                    >
                        <Button type="link" danger icon={<DeleteOutlined />}>
                            Delete
                        </Button>
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
                <h2>Quản lý phim</h2>
                <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
                    Thêm phim mới
                </Button>
            </div>

            <Table
                columns={columns}
                dataSource={movies}
                rowKey="id"
                loading={isLoading}
                pagination={{ pageSize: 10 }}
            />

            <Modal
                title={editingMovie ? 'Chỉnh sửa phim' : 'Thêm phim mới'}
                open={modalOpen}
                onCancel={() => setModalOpen(false)}
                onOk={form.submit}
                width={700}
            >
                <Form form={form} layout="vertical" onFinish={handleSubmit}>
                    <Form.Item name="title" label="Tên phim" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>
                    <Form.Item name="description" label="Mô tả">
                        <Input.TextArea rows={3} />
                    </Form.Item>
                    <Form.Item name="genre" label="Thể loại" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>
                    <Form.Item name="duration" label="Thời lượng (phút)" rules={[{ required: true }]}>
                        <Input type="number" />
                    </Form.Item>
                    <Form.Item name="releaseDate" label="Ngày khởi chiếu" rules={[{ required: true }]}>
                        <DatePicker style={{ width: '100%' }} />
                    </Form.Item>
                    <Form.Item name="posterUrl" label="URL Poster">
                        <Input />
                    </Form.Item>
                    <Form.Item name="trailerUrl" label="URL Trailer">
                        <Input />
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
}

export default AdminMoviesPage;
