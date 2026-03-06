import { Outlet, Link, useNavigate } from 'react-router-dom';
import { Layout, Menu, Button, Avatar, Dropdown, Space, Tag, Modal } from 'antd';
import {
    HomeOutlined,
    VideoCameraOutlined,
    CalendarOutlined,
    UserOutlined,
    LogoutOutlined,
} from '@ant-design/icons';
import { useAuthStore } from '../../../features/auth/store/authStore';
import { useSidebarStore } from '../../store/sidebarStore';
import { usePageTitle } from '../../hooks/usePageTitle';
import Breadcrumb from '../common/Breadcrumb';
import { useState, useEffect } from 'react';

const { Header, Content, Footer, Sider } = Layout;

// Get role display name in Vietnamese
const getRoleDisplayName = (role: string): string => {
    const roleMap: Record<string, string> = {
        admin: 'Quản trị viên',
        manager: 'Quản lý',
    };
    return roleMap[role.toLowerCase()] || role;
};

// Get role color
const getRoleColor = (role: string): string => {
    const colorMap: Record<string, string> = {
        admin: 'red',
        manager: 'blue',
    };
    return colorMap[role.toLowerCase()] || 'default';
};

function AppLayout() {
    const navigate = useNavigate();
    const { user, isAuthenticated, logout } = useAuthStore();
    const { collapsed, setCollapsed } = useSidebarStore();
    const [isMobile, setIsMobile] = useState(false);

    // Initialize page title
    usePageTitle();

    // Handle responsive breakpoint
    useEffect(() => {
        const handleResize = () => {
            setIsMobile(window.innerWidth < 992);
        };

        handleResize();
        window.addEventListener('resize', handleResize);
        return () => window.removeEventListener('resize', handleResize);
    }, []);

    // Get user's primary role (if admin/manager)
    const userRoles = user?.roles || [];
    const isAdminOrManager = userRoles.some(role => ['admin', 'manager'].includes(role.toLowerCase()));
    const primaryRole = isAdminOrManager ? userRoles[0] : '';

    // Handle logout with confirmation
    const handleLogout = () => {
        Modal.confirm({
            title: 'Xác nhận đăng xuất',
            content: 'Bạn có chắc chắn muốn đăng xuất khỏi hệ thống?',
            okText: 'Đăng xuất',
            cancelText: 'Hủy',
            okButtonProps: { danger: true },
            onOk: () => {
                // Get roles before logout
                logout();
                // Role-based redirect after logout
                if (isAdminOrManager) {
                    navigate('/login');
                } else {
                    navigate('/');
                }
            },
        });
    };

    const userMenuItems = [
        {
            key: 'profile',
            icon: <UserOutlined />,
            label: 'Hồ sơ',
            onClick: () => navigate('/profile'),
        },
        {
            key: 'history',
            icon: <CalendarOutlined />,
            label: 'Lịch sử đặt vé',
            onClick: () => navigate('/profile/history'),
        },
        {
            type: 'divider' as const,
        },
        {
            key: 'logout',
            icon: <LogoutOutlined />,
            label: 'Đăng xuất',
            onClick: handleLogout,
        },
    ];

    const menuItems = [
        {
            key: '/',
            icon: <HomeOutlined />,
            label: <Link to="/">Trang chủ</Link>,
        },
        {
            key: '/movies',
            icon: <VideoCameraOutlined />,
            label: <Link to="/movies">Phim</Link>,
        },
        {
            key: '/cinemas',
            icon: <CalendarOutlined />,
            label: <Link to="/cinemas">Rạp chiếu</Link>,
        },
    ];

    return (
        <Layout style={{ minHeight: '100vh' }}>
            <Sider
                collapsible
                collapsed={collapsed}
                onCollapse={setCollapsed}
                breakpoint="lg"
                collapsedWidth={isMobile ? 0 : 80}
                style={{
                    overflow: 'auto',
                    height: '100vh',
                    position: 'fixed',
                    left: 0,
                    top: 0,
                    bottom: 0,
                    zIndex: 100,
                }}
            >
                <div style={{
                    height: 64,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    margin: '16px 0',
                }}>
                    {!collapsed && (
                        <Link to="/" style={{ color: '#e50914', fontSize: 20, fontWeight: 'bold' }}>
                            CINEMA
                        </Link>
                    )}
                    {collapsed && (
                        <span style={{ color: '#e50914', fontSize: 24, fontWeight: 'bold' }}>C</span>
                    )}
                </div>
                <Menu
                    theme="dark"
                    defaultSelectedKeys={['/']}
                    mode="inline"
                    items={menuItems}
                    onClick={({ key }) => navigate(key)}
                />
            </Sider>
            <Layout style={{ marginLeft: collapsed ? (isMobile ? 0 : 80) : (isMobile ? 0 : 200), transition: 'margin-left 0.2s' }}>
                <Header style={{
                    padding: '0 24px',
                    background: '#000',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'flex-end',
                    position: 'sticky',
                    top: 0,
                    zIndex: 99,
                    width: '100%',
                }}>
                    {isAuthenticated ? (
                        <Dropdown menu={{ items: userMenuItems }} placement="bottomRight">
                            <Space style={{ cursor: 'pointer' }}>
                                <Avatar icon={<UserOutlined />} style={{ backgroundColor: '#e50914' }} />
                                <span>{user?.firstName || user?.username || user?.email}</span>
                                {isAdminOrManager && primaryRole && (
                                    <Tag color={getRoleColor(primaryRole)} style={{ marginLeft: 8 }}>
                                        {getRoleDisplayName(primaryRole)}
                                    </Tag>
                                )}
                            </Space>
                        </Dropdown>
                    ) : (
                        <Space>
                            <Button type="text" onClick={() => navigate('/login')}>
                                Đăng nhập
                            </Button>
                            <Button type="primary" onClick={() => navigate('/register')}>
                                Đăng ký
                            </Button>
                        </Space>
                    )}
                </Header>
                <Content style={{ margin: '24px 16px', padding: 24, minHeight: 280 }}>
                    <Breadcrumb style={{ marginBottom: 16 }} />
                    <Outlet />
                </Content>
                <Footer style={{ textAlign: 'center', background: 'transparent' }}>
                    Cinema System ©{new Date().getFullYear()} - Hệ thống đặt vé xem phim
                </Footer>
            </Layout>
        </Layout>
    );
}

export default AppLayout;
