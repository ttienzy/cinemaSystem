import { Button, Result, Space } from 'antd';
import { useNavigate, useSearchParams } from 'react-router-dom';

export default function PaymentSuccessPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const bookingId = searchParams.get('bookingId');

  return (
    <main className="page-shell">
      <Result
        extra={
          <Space>
            {bookingId ? <Button onClick={() => navigate(`/success/${bookingId}`)}>View booking</Button> : null}
            <Button onClick={() => navigate('/')} type="primary">
              Back home
            </Button>
          </Space>
        }
        status="success"
        subTitle={bookingId ? `Booking ${bookingId} is paid successfully.` : 'Your payment was completed.'}
        title="Payment successful"
      />
    </main>
  );
}
