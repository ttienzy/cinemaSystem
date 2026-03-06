// Profile Page - View and Edit Profile
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    Card,
    Typography,
    Form,
    Input,
    Button,
    DatePicker,
    Select,
    message,
    Spin,
    Avatar,
    Divider,
    Tabs
} from 'antd';
import { UserOutlined, SaveOutlined, LockOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { useMyProfile, useUpdateProfile, useChangePassword } from '../../features/profile/hooks/useProfile';
import type { UpdateProfileRequest, ChangePasswordRequest } from '../../features/profile/types/profile.types';

const { Title, Text } = Typography;
const { Option } = Select;

const ProfilePage = () => {
    const navigate = useNavigate();
    const [form] = Form.useForm();
    const [passwordForm] = Form.useForm();
    const [activeTab, setActiveTab] = useState('profile');

    // Fetch profile data
    const { data: profile, isLoading, error } = useMyProfile();
    const updateProfile = useUpdateProfile();
    const changePassword = useChangePassword();

    useEffect(() => {
        if (profile) {
            form.setFieldsValue({
                firstName: profile.firstName,
                lastName: profile.lastName,
                email: profile.email,
                phoneNumber: profile.phoneNumber,
                dateOfBirth: profile.dateOfBirth ? dayjs(profile.dateOfBirth) : null,
                gender: profile.gender,
                address: profile.address,
            });
        }
    }, [profile, form]);

    const handleUpdateProfile = async (values: UpdateProfileRequest) => {
        try {
            await updateProfile.mutateAsync({
                ...values,
                dateOfBirth: values.dateOfBirth ? (values.dateOfBirth as unknown as dayjs.Dayjs).format('YYYY-MM-DD') : undefined,
            });
            message.success('Cập nhật hồ sơ thành công');
        } catch {
            message.error('Cập nhật hồ sơ thất bại');
        }
    };

    const handleChangePassword = async (values: ChangePasswordRequest) => {
        try {
            await changePassword.mutateAsync(values);
            message.success('Đổi mật khẩu thành công');
            passwordForm.resetFields();
            setActiveTab('profile');
        } catch {
            message.error('Đổi mật khẩu thất bại. Vui lòng kiểm tra mật khẩu hiện tại.');
        }
    };

    if (isLoading) {
        return (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
                <Spin size="large" />
            </div>
        );
    }

    if (error || !profile) {
        return (
            <div style={{ padding: '24px', textAlign: 'center' }}>
                <Title level={4}>Không thể tải thông tin hồ sơ</Title>
                <Button type="primary" onClick={() => navigate('/')}>
                    Về trang chủ
                </Button>
            </div>
        );
    }

    const tabItems = [
        {
            key: 'profile',
            label: (
                <span>
                    <UserOutlined />
                    Thông tin cá nhân
                </span>
            ),
            children: (
                <Card style={{ background: '#1f1f1f', borderColor: '#333' }}>
                    <div style={{ textAlign: 'center', marginBottom: '24px' }}>
                        <Avatar
                            size={100}
                            src={profile.avatarUrl}
                            icon={<UserOutlined />}
                            style={{ marginBottom: '16px' }}
                        />
                        <Title level={4} style={{ color: '#fff', marginBottom: '4px' }}>
                            {profile.firstName} {profile.lastName}
                        </Title>
                        <Text type="secondary" style={{ color: '#999' }}>
                            @{profile.userName}
                        </Text>
                        <div style={{ marginTop: '8px' }}>
                            {profile.roles.map(role => (
                                <span key={role} style={{
                                    display: 'inline-block',
                                    padding: '2px 8px',
                                    margin: '0 4px',
                                    background: '#1890ff',
                                    borderRadius: '4px',
                                    fontSize: '12px',
                                    color: '#fff'
                                }}>
                                    {role}
                                </span>
                            ))}
                        </div>
                    </div>

                    <Divider style={{ borderColor: '#333' }} />

                    <Form
                        form={form}
                        layout="vertical"
                        onFinish={handleUpdateProfile}
                    >
                        <Form.Item
                            name="firstName"
                            label={<span style={{ color: '#fff' }}>Họ</span>}
                            rules={[{ required: true, message: 'Vui lòng nhập họ' }]}
                        >
                            <Input />
                        </Form.Item>

                        <Form.Item
                            name="lastName"
                            label={<span style={{ color: '#fff' }}>Tên</span>}
                            rules={[{ required: true, message: 'Vui lòng nhập tên' }]}
                        >
                            <Input />
                        </Form.Item>

                        <Form.Item
                            name="email"
                            label={<span style={{ color: '#fff' }}>Email</span>}
                        >
                            <Input disabled />
                        </Form.Item>

                        <Form.Item
                            name="phoneNumber"
                            label={<span style={{ color: '#fff' }}>Số điện thoại</span>}
                        >
                            <Input />
                        </Form.Item>

                        <Form.Item
                            name="dateOfBirth"
                            label={<span style={{ color: '#fff' }}>Ngày sinh</span>}
                        >
                            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
                        </Form.Item>

                        <Form.Item
                            name="gender"
                            label={<span style={{ color: '#fff' }}>Giới tính</span>}
                        >
                            <Select>
                                <Option value="Male">Nam</Option>
                                <Option value="Female">Nữ</Option>
                                <Option value="Other">Khác</Option>
                            </Select>
                        </Form.Item>

                        <Form.Item
                            name="address"
                            label={<span style={{ color: '#fff' }}>Địa chỉ</span>}
                        >
                            <Input.TextArea rows={2} />
                        </Form.Item>

                        <Form.Item>
                            <Button
                                type="primary"
                                htmlType="submit"
                                loading={updateProfile.isPending}
                                icon={<SaveOutlined />}
                                style={{ marginTop: '16px' }}
                            >
                                Lưu thay đổi
                            </Button>
                        </Form.Item>
                    </Form>
                </Card>
            ),
        },
        {
            key: 'password',
            label: (
                <span>
                    <LockOutlined />
                    Đổi mật khẩu
                </span>
            ),
            children: (
                <Card style={{ background: '#1f1f1f', borderColor: '#333' }}>
                    <Title level={4} style={{ color: '#fff', marginBottom: '24px' }}>
                        Đổi mật khẩu
                    </Title>

                    <Form
                        form={passwordForm}
                        layout="vertical"
                        onFinish={handleChangePassword}
                    >
                        <Form.Item
                            name="currentPassword"
                            label={<span style={{ color: '#fff' }}>Mật khẩu hiện tại</span>}
                            rules={[{ required: true, message: 'Vui lòng nhập mật khẩu hiện tại' }]}
                        >
                            <Input.Password />
                        </Form.Item>

                        <Form.Item
                            name="newPassword"
                            label={<span style={{ color: '#fff' }}>Mật khẩu mới</span>}
                            rules={[
                                { required: true, message: 'Vui lòng nhập mật khẩu mới' },
                                { min: 6, message: 'Mật khẩu phải có ít nhất 6 ký tự' }
                            ]}
                        >
                            <Input.Password />
                        </Form.Item>

                        <Form.Item
                            name="confirmPassword"
                            label={<span style={{ color: '#fff' }}>Xác nhận mật khẩu mới</span>}
                            dependencies={['newPassword']}
                            rules={[
                                { required: true, message: 'Vui lòng xác nhận mật khẩu mới' },
                                ({ getFieldValue }) => ({
                                    validator(_, value) {
                                        if (!value || getFieldValue('newPassword') === value) {
                                            return Promise.resolve();
                                        }
                                        return Promise.reject(new Error('Mật khẩu xác nhận không khớp'));
                                    },
                                }),
                            ]}
                        >
                            <Input.Password />
                        </Form.Item>

                        <Form.Item>
                            <Button
                                type="primary"
                                htmlType="submit"
                                loading={changePassword.isPending}
                                icon={<LockOutlined />}
                                style={{ marginTop: '16px' }}
                            >
                                Đổi mật khẩu
                            </Button>
                        </Form.Item>
                    </Form>
                </Card>
            ),
        },
    ];

    return (
        <div style={{ padding: '24px', background: '#141414', minHeight: '100vh' }}>
            <Title level={2} style={{ color: '#fff', marginBottom: '24px' }}>
                Hồ sơ cá nhân
            </Title>

            <Tabs
                activeKey={activeTab}
                onChange={setActiveTab}
                items={tabItems}
            />
        </div>
    );
};

export default ProfilePage;
