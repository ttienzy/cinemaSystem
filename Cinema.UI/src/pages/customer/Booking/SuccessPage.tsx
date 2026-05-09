import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Result, Button, Card } from 'antd';
import { QrcodeOutlined } from '@ant-design/icons';

const SuccessPage: React.FC = () => {
  const { bookingId } = useParams<{ bookingId: string }>();
  const navigate = useNavigate();

  return (
    <div style={{ padding: '60px 10%', backgroundColor: '#0f172a', minHeight: '100vh', display: 'flex', justifyContent: 'center' }}>
      <Card style={{ width: '100%', maxWidth: 600, background: 'rgba(30,41,59,0.9)', border: '1px solid rgba(255,255,255,0.1)', borderRadius: 16 }}>
        <Result
          status="success"
          title={<span style={{ color: '#38bdf8' }}>Đặt Vé Thành Công!</span>}
          subTitle={<span style={{ color: '#cbd5e1' }}>Mã đặt vé của bạn là: <strong>{bookingId}</strong>. Email xác nhận đã được gửi đi.</span>}
          extra={[
            <Button type="primary" key="console" onClick={() => navigate('/')} style={{ background: '#38bdf8', color: '#0f172a', fontWeight: 'bold' }}>
              Trở về Trang chủ
            </Button>,
            <Button key="buy" onClick={() => navigate('/customer/history')} style={{ background: 'transparent', color: '#38bdf8', borderColor: '#38bdf8' }}>
              Xem Lịch Sử Đặt Vé
            </Button>,
          ]}
        >
          <div style={{ textAlign: 'center', marginTop: 24, padding: 24, background: 'white', borderRadius: 12, display: 'inline-block', margin: '0 auto' }}>
            <QrcodeOutlined style={{ fontSize: 160, color: 'black' }} />
            <p style={{ color: 'black', margin: '8px 0 0 0', fontWeight: 'bold' }}>Đưa mã này cho nhân viên soát vé</p>
          </div>
        </Result>
      </Card>
    </div>
  );
};

export default SuccessPage;
