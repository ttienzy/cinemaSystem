import { LockOutlined, MailOutlined, PhoneOutlined, UserOutlined } from '@ant-design/icons';
import { Button, Card, Form, Input, Typography } from 'antd';
import { Link, Navigate } from 'react-router-dom';
import { useState } from 'react';
import { useAuth } from '../../features/auth/useAuth';
import { useAuthStore } from '../../features/auth/authStore';
import type { RegisterRequest } from '../../shared/types/auth';

const { Text, Title } = Typography;

export default function RegisterPage() {
  const { register } = useAuth();
  const { isAuthenticated } = useAuthStore();
  const [submitting, setSubmitting] = useState(false);

  if (isAuthenticated) return <Navigate to="/" replace />;

  async function handleFinish(values: RegisterRequest) {
    setSubmitting(true);
    try {
      await register(values);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="auth-shell">
      <Card style={{ width: '100%', maxWidth: 460, borderRadius: 8 }}>
        <div style={{ marginBottom: 24 }}>
          <Title level={3} style={{ marginBottom: 4 }}>
            Create account
          </Title>
          <Text type="secondary">Register to book tickets faster.</Text>
        </div>

        <Form<RegisterRequest> layout="vertical" onFinish={handleFinish} requiredMark={false}>
          <Form.Item name="fullName" label="Full name" rules={[{ required: true, min: 2 }]}>
            <Input prefix={<UserOutlined />} autoComplete="name" />
          </Form.Item>
          <Form.Item
            name="email"
            label="Email"
            rules={[
              { required: true, message: 'Email is required' },
              { type: 'email', message: 'Enter a valid email' },
            ]}
          >
            <Input prefix={<MailOutlined />} autoComplete="email" />
          </Form.Item>
          <Form.Item name="phoneNumber" label="Phone" rules={[{ required: true }]}>
            <Input prefix={<PhoneOutlined />} autoComplete="tel" />
          </Form.Item>
          <Form.Item name="password" label="Password" rules={[{ required: true, min: 6 }]}>
            <Input.Password prefix={<LockOutlined />} autoComplete="new-password" />
          </Form.Item>
          <Button type="primary" htmlType="submit" block loading={submitting}>
            Register
          </Button>
        </Form>

        <div style={{ marginTop: 16 }}>
          <Text type="secondary">Already have an account? </Text>
          <Link to="/login">Sign in</Link>
        </div>
      </Card>
    </div>
  );
}
