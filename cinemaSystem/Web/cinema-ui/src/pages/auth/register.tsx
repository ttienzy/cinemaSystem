// Register Page - Trang đăng ký
import { Link, useNavigate } from 'react-router-dom';
import { Form, Input, Button, message } from 'antd';
import { LockOutlined, MailOutlined, PhoneOutlined, UserOutlined } from '@ant-design/icons';
import { useRegister } from '../../features/auth/hooks/useRegister';

interface RegisterFormValues {
    email: string;
    password: string;
    confirmPassword: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
}

function RegisterPage() {
    const registerMutation = useRegister();
    const navigate = useNavigate();
    const [form] = Form.useForm();

    const onFinish = (values: RegisterFormValues) => {
        if (values.password !== values.confirmPassword) {
            message.error('Mật khẩu không khớp!');
            return;
        }

        registerMutation.mutate({
            email: values.email,
            password: values.password,
            confirmPassword: values.confirmPassword,
            firstName: values.firstName,
            lastName: values.lastName,
            phoneNumber: values.phoneNumber,
        }, {
            onSuccess: () => {
                message.success('Đăng ký thành công! Vui lòng đăng nhập.');
                navigate('/login');
            },
            onError: (error: Error) => {
                message.error(error.message || 'Đăng ký thất bại. Vui lòng thử lại.');
            }
        });
    };

    return (
        <div style={{
            background: '#1f1f1f',
            padding: '40px 32px',
            borderRadius: 12,
            boxShadow: '0 8px 32px rgba(0, 0, 0, 0.4)',
            width: '100%',
            maxWidth: '450px',
        }}>
            <h2 style={{
                textAlign: 'center',
                marginBottom: 32,
                color: '#fff',
                fontSize: 24,
            }}>
                Đăng ký
            </h2>

            <Form
                form={form}
                name="register"
                onFinish={onFinish}
                layout="vertical"
                size="large"
            >
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                    <Form.Item
                        name="firstName"
                        label="Họ"
                        rules={[{ required: true, message: 'Vui lòng nhập họ!' }]}
                    >
                        <Input
                            prefix={<UserOutlined />}
                            placeholder="Nhập họ"
                        />
                    </Form.Item>

                    <Form.Item
                        name="lastName"
                        label="Tên"
                        rules={[{ required: true, message: 'Vui lòng nhập tên!' }]}
                    >
                        <Input
                            placeholder="Nhập tên"
                        />
                    </Form.Item>
                </div>

                <Form.Item
                    name="email"
                    label="Email"
                    rules={[
                        { required: true, message: 'Vui lòng nhập email!' },
                        { type: 'email', message: 'Email không hợp lệ!' }
                    ]}
                >
                    <Input
                        prefix={<MailOutlined />}
                        placeholder="Nhập email"
                    />
                </Form.Item>

                <Form.Item
                    name="phoneNumber"
                    label="Số điện thoại"
                >
                    <Input
                        prefix={<PhoneOutlined />}
                        placeholder="Nhập số điện thoại (tùy chọn)"
                    />
                </Form.Item>

                <Form.Item
                    name="password"
                    label="Mật khẩu"
                    rules={[
                        { required: true, message: 'Vui lòng nhập mật khẩu!' },
                        { min: 6, message: 'Mật khẩu phải có ít nhất 6 ký tự!' }
                    ]}
                >
                    <Input.Password
                        prefix={<LockOutlined />}
                        placeholder="Nhập mật khẩu"
                    />
                </Form.Item>

                <Form.Item
                    name="confirmPassword"
                    label="Xác nhận mật khẩu"
                    rules={[
                        { required: true, message: 'Vui lòng xác nhận mật khẩu!' },
                        { min: 6, message: 'Mật khẩu phải có ít nhất 6 ký tự!' }
                    ]}
                >
                    <Input.Password
                        prefix={<LockOutlined />}
                        placeholder="Nhập lại mật khẩu"
                    />
                </Form.Item>

                <Form.Item>
                    <Button
                        type="primary"
                        htmlType="submit"
                        loading={registerMutation.isPending}
                        block
                        style={{
                            height: 48,
                            fontSize: 16,
                            fontWeight: 600,
                            background: '#e50914',
                            borderColor: '#e50914',
                        }}
                    >
                        Đăng ký
                    </Button>
                </Form.Item>
            </Form>

            <div style={{ textAlign: 'center', marginTop: 16 }}>
                <span style={{ color: '#a3a3a3' }}>Đã có tài khoản? </span>
                <Link to="/login" style={{ color: '#e50914' }}>
                    Đăng nhập ngay
                </Link>
            </div>
        </div>
    );
}

export default RegisterPage;
