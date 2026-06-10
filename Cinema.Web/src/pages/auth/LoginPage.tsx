import { LockOutlined, MailOutlined } from '@ant-design/icons';
import { Button, Card, Form, Input, Typography } from 'antd';
import { Navigate } from 'react-router-dom';
import { Link } from 'react-router-dom';
import { useState } from 'react';
import { useAuth } from '../../features/auth/useAuth';
import { useAuthStore } from '../../features/auth/authStore';
import type { LoginRequest } from '../../shared/types/auth';

const { Text, Title } = Typography;

export default function LoginPage() {
  const { login } = useAuth();
  const { isAuthenticated, user } = useAuthStore();
  const [submitting, setSubmitting] = useState(false);

  if (isAuthenticated && user) {
    return <Navigate to="/" replace />;
  }

  async function handleFinish(values: LoginRequest) {
    setSubmitting(true);
    try {
      await login(values);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div
      style={{
        minHeight: '100vh',
        display: 'grid',
        placeItems: 'center',
        padding: 24,
        background:
          'linear-gradient(135deg, #f6f8fb 0%, #eef3f9 48%, #f9fafb 100%)',
      }}
      className="auth-shell"
    >
      <Card style={{ width: '100%', maxWidth: 420, borderRadius: 8 }}>
        <div style={{ marginBottom: 24 }}>
          <Title level={3} style={{ marginBottom: 4 }}>
            Cinema Web
          </Title>
          <Text type="secondary">Sign in to book tickets and manage your orders.</Text>
        </div>

        <Form<LoginRequest> layout="vertical" onFinish={handleFinish} requiredMark={false}>
          <Form.Item
            label="Email"
            name="email"
            rules={[
              { required: true, message: 'Email is required' },
              { type: 'email', message: 'Enter a valid email' },
            ]}
          >
            <Input prefix={<MailOutlined />} autoComplete="email" />
          </Form.Item>

          <Form.Item
            label="Password"
            name="password"
            rules={[{ required: true, message: 'Password is required' }]}
          >
            <Input.Password prefix={<LockOutlined />} autoComplete="current-password" />
          </Form.Item>

          <Button type="primary" htmlType="submit" block loading={submitting}>
            Sign in
          </Button>
        </Form>
        <div style={{ marginTop: 16 }}>
          <Text type="secondary">New here? </Text>
          <Link to="/register">Create an account</Link>
        </div>
      </Card>
    </div>
  );
}
