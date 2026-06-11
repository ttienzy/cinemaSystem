import { Button, Result, Space } from 'antd';
import { useNavigate } from 'react-router-dom';

export default function PaymentErrorPage() {
  const navigate = useNavigate();

  return (
    <main className="page-shell">
      <Result
        extra={
          <Space>
            <Button onClick={() => navigate('/movies')}>Try again</Button>
            <Button onClick={() => navigate('/')} type="primary">
              Back home
            </Button>
          </Space>
        }
        status="error"
        subTitle="Payment did not complete. The booking will be released by the payment failure flow."
        title="Payment failed"
      />
    </main>
  );
}
