import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Form, Input, Button, Card, message, Statistic } from 'antd';
import { useMutation } from '@tanstack/react-query';
import { bookingApi, type CreateBookingRequest } from '../../../features/booking/api/bookingApi';
import { useBookingStore } from '../../../features/booking/store/useBookingStore';
import dayjs from '../../../utils/dayjs';

const { Countdown } = Statistic;

const CheckoutPage: React.FC = () => {
  const navigate = useNavigate();
  const { showtimeId, selectedSeats, lockedUntil, clearBookingSession } = useBookingStore();
  const [form] = Form.useForm();

  // Redirect if no active session
  useEffect(() => {
    if (!showtimeId || selectedSeats.length === 0 || !lockedUntil) {
      message.error('Phiên đặt vé đã hết hạn hoặc không hợp lệ.');
      navigate('/');
    }
  }, [showtimeId, selectedSeats, lockedUntil, navigate]);

  const totalPrice = selectedSeats.reduce((sum, seat) => sum + seat.price, 0);

  const bookingMutation = useMutation({
    mutationFn: bookingApi.createBooking,
    onSuccess: (res) => {
      if (res.success && res.data) {
        // Check if payment checkout URL is available
        if (res.data.checkoutUrl) {
          message.success('Đang chuyển đến trang thanh toán...');
          clearBookingSession();

          // ✅ FIXED: Redirect directly to Payment.API (not through Gateway)
          // Gateway doesn't handle HTML responses from checkout endpoint
          window.location.href = `https://localhost:7252${res.data.checkoutUrl}`;
        } else {
          // Fallback: Payment not ready yet
          message.warning('Đang xử lý thanh toán, vui lòng đợi...');

          // Could implement polling here or redirect to a waiting page
          setTimeout(() => {
            navigate(`/booking-status/${res.data.bookingId}`);
          }, 2000);
        }
      } else {
        message.error(res.message || 'Lỗi khi tạo booking');
      }
    },
    onError: () => message.error('Không thể hoàn tất booking')
  });

  const onFinish = (values: any) => {
    if (dayjs().isAfter(dayjs(lockedUntil))) {
      message.error('Đã hết thời gian giữ ghế. Vui lòng chọn lại.');
      navigate(`/booking/${showtimeId}`);
      return;
    }

    // userId will be extracted from JWT token by backend
    const payload: CreateBookingRequest = {
      showtimeId: showtimeId!,
      seatIds: selectedSeats.map(s => s.seatId),
      contactName: values.contactName,
      contactEmail: values.contactEmail,
      contactPhone: values.contactPhone
    };

    bookingMutation.mutate(payload);
  };

  const handleTimeout = () => {
    message.warning('Đã hết thời gian giữ ghế. Giao dịch bị hủy.');
    clearBookingSession();
    navigate(`/booking/${showtimeId}`);
  };

  if (!lockedUntil) return null;

  return (
    <div style={{ padding: '40px 10%', backgroundColor: '#0f172a', minHeight: '100vh', display: 'flex', gap: 32 }}>
      {/* Form Thông tin người đặt */}
      <Card title="Thông Tin Thanh Toán" style={{ flex: 2, background: 'rgba(30,41,59,0.8)', border: 'none' }} headStyle={{ color: 'white', borderBottom: '1px solid #334155' }}>
        <Form form={form} layout="vertical" onFinish={onFinish}>
          <Form.Item name="contactName" label={<span style={{ color: 'white' }}>Họ và tên</span>} rules={[{ required: true, message: 'Vui lòng nhập tên' }]}>
            <Input size="large" placeholder="Nhập họ tên của bạn" />
          </Form.Item>
          <Form.Item name="contactPhone" label={<span style={{ color: 'white' }}>Số điện thoại</span>} rules={[{ required: true, message: 'Vui lòng nhập số điện thoại' }]}>
            <Input size="large" placeholder="Nhập số điện thoại" />
          </Form.Item>
          <Form.Item name="contactEmail" label={<span style={{ color: 'white' }}>Email nhận vé</span>} rules={[{ required: true, type: 'email', message: 'Vui lòng nhập email hợp lệ' }]}>
            <Input size="large" placeholder="Nhập email để nhận mã QR vé" />
          </Form.Item>
        </Form>
      </Card>

      {/* Tổng kết đơn hàng & Countdown */}
      <Card title="Tóm Tắt Đơn Hàng" style={{ flex: 1, background: 'rgba(30,41,59,0.8)', border: 'none' }} headStyle={{ color: 'white', borderBottom: '1px solid #334155' }}>
        <div style={{ textAlign: 'center', marginBottom: 24, padding: 16, background: '#1e293b', borderRadius: 8 }}>
          <p style={{ color: '#94a3b8', margin: 0 }}>Thời gian giữ ghế còn lại</p>
          <Countdown
            value={dayjs(lockedUntil).valueOf()}
            onFinish={handleTimeout}
            format="mm:ss"
            valueStyle={{ color: '#ef4444', fontSize: '2rem', fontWeight: 'bold' }}
          />
        </div>

        <div style={{ color: '#cbd5e1', marginBottom: 24, fontSize: '1.1rem' }}>
          <p><strong>Ghế:</strong> {selectedSeats.map(s => `${s.row}${s.number}`).join(', ')}</p>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 16, paddingTop: 16, borderTop: '1px dashed #475569' }}>
            <span>Tổng tiền:</span>
            <span style={{ color: '#38bdf8', fontWeight: 'bold', fontSize: '1.4rem' }}>{totalPrice.toLocaleString()} đ</span>
          </div>
        </div>

        <Button
          type="primary"
          size="large"
          block
          onClick={() => form.submit()}
          loading={bookingMutation.isPending}
          style={{ background: '#38bdf8', color: '#0f172a', fontWeight: 'bold' }}
        >
          Xác nhận Thanh toán
        </Button>
      </Card>
    </div>
  );
};

export default CheckoutPage;
