// Login Page - Trang đăng nhập
import { Link } from 'react-router-dom';
import { Form, Input, Button, Divider } from 'antd';
import { UserOutlined, LockOutlined } from '@ant-design/icons';
import { useLogin } from '../../features/auth/hooks/useLogin';

interface LoginFormValues {
    email: string;
    password: string;
    rememberMe?: boolean;
}

function LoginPage() {
    const loginMutation = useLogin();
    const [form] = Form.useForm();

    const onFinish = (values: LoginFormValues) => {
        loginMutation.mutate({
            email: values.email,
            password: values.password,
            rememberMe: values.rememberMe,
        });
    };

    return (
        <div style={{
            background: '#1f1f1f',
            padding: '40px 32px',
            borderRadius: 12,
            boxShadow: '0 8px 32px rgba(0, 0, 0, 0.4)',
            width: '100%',
            maxWidth: '400px',
        }}>
            <h2 style={{
                textAlign: 'center',
                marginBottom: 32,
                color: '#fff',
                fontSize: 24,
            }}>
                Đăng nhập
            </h2>

            <Form
                form={form}
                name="login"
                onFinish={onFinish}
                layout="vertical"
                size="large"
                initialValues={{ rememberMe: false }}
            >
                <Form.Item
                    name="email"
                    rules={[
                        { required: true, message: 'Vui lòng nhập email!' },
                        { type: 'email', message: 'Email không hợp lệ!' }
                    ]}
                >
                    <Input
                        prefix={<UserOutlined />}
                        placeholder="Email"
                    />
                </Form.Item>

                <Form.Item
                    name="password"
                    rules={[
                        { required: true, message: 'Vui lòng nhập mật khẩu!' },
                        { min: 6, message: 'Mật khẩu phải có ít nhất 6 ký tự!' }
                    ]}
                >
                    <Input.Password
                        prefix={<LockOutlined />}
                        placeholder="Mat khau"
                    />
                </Form.Item>

                <Form.Item>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <Form.Item name="rememberMe" valuePropName="checked" noStyle>
                            <label style={{ color: '#999' }}>
                                <input
                                    type="checkbox"
                                    onChange={(e) => form.setFieldValue('rememberMe', e.target.checked)}
                                /> Ghi nhớ đăng nhập
                            </label>
                        </Form.Item>
                        <Link to="/forgot-password" style={{ color: '#e50914' }}>
                            Quên mật khẩu?
                        </Link>
                    </div>
                </Form.Item>

                <Form.Item>
                    <Button
                        type="primary"
                        htmlType="submit"
                        loading={loginMutation.isPending}
                        block
                        style={{
                            height: 48,
                            fontSize: 16,
                            fontWeight: 600,
                            background: '#e50914',
                            borderColor: '#e50914',
                        }}
                    >
                        Đăng nhập
                    </Button>
                </Form.Item>
            </Form>

            <Divider plain style={{ color: '#737373' }}>
                Chưa có tài khoản?
            </Divider>

            <div style={{ textAlign: 'center' }}>
                <Link to="/register">
                    <Button
                        size="large"
                        block
                        style={{
                            height: 48,
                            fontSize: 16,
                            fontWeight: 600,
                        }}
                    >
                        Đăng ký ngay
                    </Button>
                </Link>
            </div>
        </div>
    );
}

export default LoginPage;
