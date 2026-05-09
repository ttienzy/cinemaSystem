import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Card, Spin, message, Button } from 'antd';
import { LoadingOutlined } from '@ant-design/icons';
import axiosClient from '../../../api/axiosClient';

const BookingStatusPage: React.FC = () => {
    const { bookingId } = useParams<{ bookingId: string }>();
    const navigate = useNavigate();
    const [polling, setPolling] = useState(true);
    const [attempts, setAttempts] = useState(0);
    const maxAttempts = 20;

    useEffect(() => {
        if (!bookingId) {
            message.error('Booking ID không hợp lệ');
            navigate('/');
            return;
        }

        const pollPayment = async () => {
            try {
                // Poll for payment by booking ID
                const response = await axiosClient.get(`/api/payments/booking/${bookingId}`);

                if (response.success && response.data) {
                    // Payment found, redirect to checkout
                    message.success('Đang chuyển đến trang thanh toán...');
                    window.location.href = `http://localhost:5000/api/payments/${response.data.id}/checkout`;
                    setPolling(false);
                } else if (attempts >= maxAttempts) {
                    // Max attempts reached
                    message.error('Không thể tạo thanh toán. Vui lòng thử lại sau.');
                    setPolling(false);
                } else {
                    // Continue polling
                    setAttempts(prev => prev + 1);
                }
            } catch (error) {
                if (attempts >= maxAttempts) {
                    message.error('Lỗi khi kiểm tra trạng thái thanh toán');
                    setPolling(false);
                } else {
                    setAttempts(prev => prev + 1);
                }
            }
        };

        if (polling && attempts < maxAttempts) {
            const timer = setTimeout(pollPayment, 500); // Poll every 500ms
            return () => clearTimeout(timer);
        }
    }, [bookingId, polling, attempts, navigate]);

    return (
        <div style={{
            padding: '40px 10%',
            backgroundColor: '#0f172a',
            minHeight: '100vh',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center'
        }}>
            <Card
                style={{
                    background: 'rgba(30,41,59,0.8)',
                    border: 'none',
                    textAlign: 'center',
                    minWidth: 400
                }}
            >
                {polling ? (
                    <>
                        <Spin indicator={<LoadingOutlined style={{ fontSize: 48, color: '#38bdf8' }} spin />} />
                        <h2 style={{ color: 'white', marginTop: 24 }}>Đang xử lý thanh toán...</h2>
                        <p style={{ color: '#94a3b8' }}>Vui lòng đợi trong giây lát</p>
                        <p style={{ color: '#64748b', fontSize: '0.9rem' }}>
                            Đang kiểm tra ({attempts}/{maxAttempts})
                        </p>
                    </>
                ) : (
                    <>
                        <h2 style={{ color: '#ef4444' }}>Không thể xử lý thanh toán</h2>
                        <p style={{ color: '#94a3b8', marginBottom: 24 }}>
                            Hệ thống đang gặp sự cố. Vui lòng thử lại sau.
                        </p>
                        <Button
                            type="primary"
                            size="large"
                            onClick={() => navigate('/')}
                            style={{ background: '#38bdf8' }}
                        >
                            Quay về trang chủ
                        </Button>
                    </>
                )}
            </Card>
        </div>
    );
};

export default BookingStatusPage;
