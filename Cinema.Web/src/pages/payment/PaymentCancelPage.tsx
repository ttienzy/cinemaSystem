import { Button, Result, Space } from 'antd';
import { useMutation } from '@tanstack/react-query';
import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { bookingApi } from '../../features/booking/bookingApi';
import { getAccessToken } from '../../shared/auth/tokenStorage';

export default function PaymentCancelPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const bookingId = searchParams.get('bookingId');

  const cancelMutation = useMutation({
    mutationFn: () => bookingApi.cancelBooking(bookingId!, 'Payment was cancelled by customer'),
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
            <Button onClick={() => navigate('/movies')}>Choose another movie</Button>
            <Button onClick={() => navigate('/')} type="primary">
              Back home
            </Button>
          </Space>
        }
        status="warning"
        subTitle="The booking can be released automatically if payment was not completed."
        title="Payment cancelled"
      />
    </main>
  );
}
