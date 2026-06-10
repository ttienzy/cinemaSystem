import { Button, Result, Space } from 'antd';
import { useNavigate } from 'react-router-dom';

export default function PaymentCancelPage() {
  const navigate = useNavigate();

  return (
    <main className="page-shell">
      <Result
        extra={
          <Space>
            <Button onClick={() => navigate('/movies')}>Choose another movie</Button>
            <Button onClick={() => navigate('/')} type="primary">
              Back home
            </Button>
          </Space>
        }
        status="warning"
        subTitle="The payment service has received the cancellation and will release the booking."
        title="Payment cancelled"
      />
    </main>
  );
}
