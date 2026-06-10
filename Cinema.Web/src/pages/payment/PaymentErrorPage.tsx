import { Button, Result, Space } from 'antd';
import { useMutation } from '@tanstack/react-query';
import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { bookingApi } from '../../features/booking/bookingApi';
import { getAccessToken } from '../../shared/auth/tokenStorage';

export default function PaymentErrorPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const bookingId = searchParams.get('bookingId');

  const cancelMutation = useMutation({
    mutationFn: () => bookingApi.cancelBooking(bookingId!, 'Payment failed or returned error'),
  });

  useEffect(() => {
    if (bookingId && getAccessToken()) {
      cancelMutation.mutate();
    }
  }, [bookingId]);

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
        subTitle="Payment did not complete. Please create a new booking when you are ready."
        title="Payment failed"
      />
    </main>
  );
}
