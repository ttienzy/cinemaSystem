import { Button, Result } from 'antd';
import { useNavigate } from 'react-router-dom';

export default function UnauthorizedPage() {
  const navigate = useNavigate();

  return (
    <Result
      status="403"
      title="Access denied"
      extra={
        <Button type="primary" onClick={() => navigate('/login', { replace: true })}>
          Sign in
        </Button>
      }
    />
  );
}
