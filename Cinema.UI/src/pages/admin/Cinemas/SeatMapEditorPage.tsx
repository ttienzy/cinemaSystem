import React, { useState } from 'react';
import {
    Button, Card, Typography, Breadcrumb, Spin, Alert, Space, InputNumber,
    message, Popconfirm, Modal, Form, Input, Tag, Divider,
} from 'antd';
import {
    ArrowLeftOutlined, PlusOutlined, DeleteOutlined, SaveOutlined,
    AppstoreOutlined, ClearOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import {
    cinemaApi,
    type Seat,
} from '../../../features/cinemas/api/cinemaApi';

const { Title, Text } = Typography;

const SeatMapEditorPage: React.FC = () => {
    const { hallId } = useParams<{ hallId: string }>();
    const [selectedSeats, setSelectedSeats] = useState<Set<string>>(new Set());
    const [isGenerateModalOpen, setIsGenerateModalOpen] = useState(false);
    const [form] = Form.useForm();
    const queryClient = useQueryClient();
    const navigate = useNavigate();

    // Queries
    const { data: hallRes, isLoading: hallLoading } = useQuery({
        queryKey: ['cinema-hall', hallId],
        queryFn: () => cinemaApi.getHallById(hallId!),
        enabled: !!hallId,
    });

    const { data: seatsRes, isLoading: seatsLoading } = useQuery({
        queryKey: ['seats', hallId],
        queryFn: () => cinemaApi.getSeatsByHallId(hallId!),
        enabled: !!hallId,
    });

    const hall = hallRes?.data;
    const seats = seatsRes?.data || [];

    // Mutations
    const bulkCreateMutation = useMutation({
        mutationFn: cinemaApi.bulkCreateSeats,
        onSuccess: () => {
            message.success('Tạo ghế thành công!');
            queryClient.invalidateQueries({ queryKey: ['seats', hallId] });
            queryClient.invalidateQueries({ queryKey: ['cinema-halls'] });
            setIsGenerateModalOpen(false);
            form.resetFields();
        },
        onError: (error: any) => {
            const errorMsg = error?.response?.data?.message || 'Không thể tạo ghế';
            message.error(errorMsg);
        },
    });

    const bulkDeleteMutation = useMutation({
        mutationFn: cinemaApi.bulkDeleteSeats,
        onSuccess: () => {
            message.success('Xóa ghế thành công!');
            queryClient.invalidateQueries({ queryKey: ['seats', hallId] });
            queryClient.invalidateQueries({ queryKey: ['cinema-halls'] });
            setSelectedSeats(new Set());
        },
        onError: () => message.error('Không thể xóa ghế'),
    });

    // Handlers
    const handleGenerateSeats = async () => {
        try {
            const values = await form.validateFields();
            const { startRow, endRow, seatsPerRow } = values;

            const rows = [];
            const startCharCode = startRow.charCodeAt(0);
            const endCharCode = endRow.charCodeAt(0);

            for (let charCode = startCharCode; charCode <= endCharCode; charCode++) {
                const row = String.fromCharCode(charCode);
                for (let num = 1; num <= seatsPerRow; num++) {
                    rows.push({ row, number: num });
                }
            }

            bulkCreateMutation.mutate({
                cinemaHallId: hallId!,
                seats: rows,
            });
        } catch (error) {
            console.error('Validation failed:', error);
        }
    };

    const handleDeleteSelected = () => {
        if (selectedSeats.size === 0) {
            message.warning('Vui lòng chọn ghế để xóa');
            return;
        }
        bulkDeleteMutation.mutate(Array.from(selectedSeats));
    };

    const toggleSeatSelection = (seatId: string) => {
        const newSelected = new Set(selectedSeats);
        if (newSelected.has(seatId)) {
            newSelected.delete(seatId);
        } else {
            newSelected.add(seatId);
        }
        setSelectedSeats(newSelected);
    };

    const handleClearSelection = () => {
        setSelectedSeats(new Set());
    };

    // Build seat map
    const seatMap = new Map<string, Seat>();
    seats.forEach((seat) => {
        seatMap.set(`${seat.row}-${seat.number}`, seat);
    });

    const rows = Array.from(new Set(seats.map((s) => s.row))).sort();
    const maxNumber = Math.max(...seats.map((s) => s.number), 0);

    if (hallLoading) {
        return (
            <div style={{ textAlign: 'center', padding: 50 }}>
                <Spin size="large" />
            </div>
        );
    }

    if (!hall) {
        return (
            <Alert
                message="Không tìm thấy phòng chiếu"
                type="error"
                showIcon
                action={<Button onClick={() => navigate('/admin/cinemas')}>Quay lại</Button>}
            />
        );
    }

    return (
        <div>
            <Breadcrumb
                items={[
                    { title: 'Admin' },
                    { title: 'Cụm rạp', onClick: () => navigate('/admin/cinemas') },
                    { title: 'Phòng chiếu', onClick: () => navigate(-1) },
                    { title: hall.name },
                ]}
                style={{ marginBottom: 16 }}
            />

            {/* Header */}
            <Card style={{ marginBottom: 16, borderRadius: 12 }}>
                <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                    <div>
                        <Button
                            icon={<ArrowLeftOutlined />}
                            onClick={() => navigate(-1)}
                            style={{ marginRight: 16 }}
                        >
                            Quay lại
                        </Button>
                        <Title level={4} style={{ display: 'inline', margin: 0 }}>
                            Sơ đồ ghế - {hall.name}
                        </Title>
                        <Text type="secondary" style={{ marginLeft: 16 }}>
                            Tổng: {seats.length} ghế
                        </Text>
                    </div>
                    <Space>
                        {selectedSeats.size > 0 && (
                            <>
                                <Tag color="blue">{selectedSeats.size} ghế đã chọn</Tag>
                                <Button icon={<ClearOutlined />} onClick={handleClearSelection}>
                                    Bỏ chọn
                                </Button>
                                <Popconfirm
                                    title="Xóa ghế đã chọn?"
                                    description={`Bạn có chắc muốn xóa ${selectedSeats.size} ghế?`}
                                    onConfirm={handleDeleteSelected}
                                    okText="Xóa"
                                    cancelText="Hủy"
                                >
                                    <Button
                                        danger
                                        icon={<DeleteOutlined />}
                                        loading={bulkDeleteMutation.isPending}
                                    >
                                        Xóa đã chọn
                                    </Button>
                                </Popconfirm>
                            </>
                        )}
                        <Button
                            type="primary"
                            icon={<PlusOutlined />}
                            onClick={() => setIsGenerateModalOpen(true)}
                        >
                            Tạo ghế hàng loạt
                        </Button>
                    </Space>
                </Space>
            </Card>

            {/* Seat Map */}
            <Card style={{ borderRadius: 12 }} loading={seatsLoading}>
                {seats.length === 0 ? (
                    <Alert
                        message="Chưa có ghế nào"
                        description="Nhấn 'Tạo ghế hàng loạt' để bắt đầu tạo sơ đồ ghế"
                        type="info"
                        showIcon
                    />
                ) : (
                    <div>
                        {/* Screen */}
                        <div
                            style={{
                                width: '80%',
                                height: 20,
                                background: 'linear-gradient(to bottom, #1677ff, transparent)',
                                borderRadius: '50% 50% 0 0',
                                textAlign: 'center',
                                color: 'white',
                                margin: '0 auto 40px',
                                fontWeight: 'bold',
                            }}
                        >
                            MÀN HÌNH
                        </div>

                        {/* Seat Grid */}
                        <div
                            style={{
                                display: 'grid',
                                gridTemplateColumns: `40px repeat(${maxNumber}, 40px)`,
                                gap: '8px',
                                justifyContent: 'center',
                                alignItems: 'center',
                            }}
                        >
                            {rows.map((row) => (
                                <React.Fragment key={row}>
                                    {/* Row label */}
                                    <div
                                        style={{
                                            fontWeight: 'bold',
                                            textAlign: 'center',
                                            fontSize: 14,
                                        }}
                                    >
                                        {row}
                                    </div>

                                    {/* Seats in row */}
                                    {Array.from({ length: maxNumber }).map((_, idx) => {
                                        const number = idx + 1;
                                        const key = `${row}-${number}`;
                                        const seat = seatMap.get(key);
                                        const isSelected = seat && selectedSeats.has(seat.id);

                                        if (!seat) {
                                            return <div key={key} />; // Empty space
                                        }

                                        return (
                                            <div
                                                key={seat.id}
                                                onClick={() => toggleSeatSelection(seat.id)}
                                                style={{
                                                    width: 40,
                                                    height: 40,
                                                    backgroundColor: isSelected ? '#1677ff' : '#52c41a',
                                                    borderRadius: 4,
                                                    display: 'flex',
                                                    alignItems: 'center',
                                                    justifyContent: 'center',
                                                    cursor: 'pointer',
                                                    fontSize: 12,
                                                    fontWeight: 'bold',
                                                    color: 'white',
                                                    transition: 'all 0.3s ease',
                                                    boxShadow: isSelected
                                                        ? '0 0 10px #1677ff'
                                                        : '0 2px 4px rgba(0,0,0,0.1)',
                                                }}
                                            >
                                                {number}
                                            </div>
                                        );
                                    })}
                                </React.Fragment>
                            ))}
                        </div>

                        {/* Legend */}
                        <Divider />
                        <Space style={{ justifyContent: 'center', width: '100%' }}>
                            <Tag color="success">Ghế có sẵn</Tag>
                            <Tag color="blue">Đã chọn</Tag>
                        </Space>
                    </div>
                )}
            </Card>

            {/* Generate Modal */}
            <Modal
                title="Tạo ghế hàng loạt"
                open={isGenerateModalOpen}
                onOk={handleGenerateSeats}
                onCancel={() => {
                    setIsGenerateModalOpen(false);
                    form.resetFields();
                }}
                confirmLoading={bulkCreateMutation.isPending}
                okText="Tạo ghế"
                cancelText="Hủy"
            >
                <Form form={form} layout="vertical" style={{ marginTop: 24 }}>
                    <Form.Item
                        name="startRow"
                        label="Hàng bắt đầu"
                        rules={[
                            { required: true, message: 'Vui lòng nhập hàng bắt đầu' },
                            { pattern: /^[A-Z]$/, message: 'Phải là chữ cái in hoa (A-Z)' },
                        ]}
                        initialValue="A"
                    >
                        <Input placeholder="A" maxLength={1} style={{ textTransform: 'uppercase' }} />
                    </Form.Item>
                    <Form.Item
                        name="endRow"
                        label="Hàng kết thúc"
                        rules={[
                            { required: true, message: 'Vui lòng nhập hàng kết thúc' },
                            { pattern: /^[A-Z]$/, message: 'Phải là chữ cái in hoa (A-Z)' },
                        ]}
                        initialValue="J"
                    >
                        <Input placeholder="J" maxLength={1} style={{ textTransform: 'uppercase' }} />
                    </Form.Item>
                    <Form.Item
                        name="seatsPerRow"
                        label="Số ghế mỗi hàng"
                        rules={[{ required: true, message: 'Vui lòng nhập số ghế' }]}
                        initialValue={10}
                    >
                        <InputNumber min={1} max={50} style={{ width: '100%' }} />
                    </Form.Item>
                    <Alert
                        message="Ví dụ: A-J với 10 ghế/hàng sẽ tạo 100 ghế (A1-A10, B1-B10, ..., J1-J10)"
                        type="info"
                        showIcon
                    />
                </Form>
            </Modal>
        </div>
    );
};

export default SeatMapEditorPage;
