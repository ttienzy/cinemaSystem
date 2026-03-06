import { Outlet, Link, useNavigate } from 'react-router-dom';
import { Layout, Menu, Avatar, Dropdown, Space, Tag, Modal, Drawer, type MenuProps } from 'antd';
import {
    DashboardOutlined,
    VideoCameraOutlined,
    DesktopOutlined,
    CalendarOutlined,
    UserOutlined,
    LogoutOutlined,
    HomeOutlined,
    MenuOutlined,
    TeamOutlined,
    ToolOutlined,
} from '@ant-design/icons';
import { useAuthStore } from '../../../features/auth/store/authStore';
import { useSidebarStore } from '../../store/sidebarStore';
import { usePageTitle } from '../../hooks/usePageTitle';
import Breadcrumb from '../common/Breadcrumb';
import { useState, useEffect } from 'react';

const { Header, Sider, Content, Footer } = Layout;

// Role-based menu item definitions with required roles
interface MenuItemConfig {
    key: string;
    icon: React.ReactNode;
    label: string;
    path: string;
    roles: string[]; // Required roles to see this menu item
}

// All available menu items with their role requirements
const allMenuItems: MenuItemConfig[] = [
    {
        key: '/admin',
        icon: <DashboardOutlined />,
        label: 'Dashboard',
        path: '/admin',
        roles: ['admin', 'manager'],
    },
    {
        key: '/admin/movies',
        icon: <VideoCameraOutlined />,
        label: 'Quản lý phim',
        path: '/admin/movies',
        roles: ['admin'],
    },
    {
        key: '/admin/cinemas',
        icon: <DesktopOutlined />,
        label: 'Quản lý rạp',
        path: '/admin/cinemas',
        roles: ['admin'],
    },
    {
        key: '/admin/showtimes',
        icon: <CalendarOutlined />,
        label: 'Quản lý suất chiếu',
        path: '/admin/showtimes',
        roles: ['admin', 'manager'],
    },
    {
        key: '/admin/bookings',
        icon: <CalendarOutlined />,
        label: 'Quản lý đặt vé',
        path: '/admin/bookings',
        roles: ['admin', 'manager'],
    },
    {
        key: '/admin/users',
        icon: <TeamOutlined />,
        label: 'Quản lý người dùng',
        path: '/admin/users',
        roles: ['admin'],
    },
    {
        key: '/admin/staff',
        icon: <TeamOutlined />,
        label: 'Quản lý nhân viên',
        path: '/admin/staff',
        roles: ['admin', 'manager'],
    },
    {
        key: '/admin/equipment',
        icon: <ToolOutlined />,
        label: 'Quản lý thiết bị',
        path: '/admin/equipment',
        roles: ['admin', 'manager'],
    },
];

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

function AdminLayout() {
    const navigate = useNavigate();
    const { user, logout } = useAuthStore();
    const { collapsed, setCollapsed } = useSidebarStore();
    const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
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

    // Get user's primary role (first role in the array)
    const userRoles = user?.roles || [];
    const primaryRole = userRoles[0] || '';

    // Filter menu items based on user roles
    const menuItems = allMenuItems
        .filter(item => item.roles.some(role => userRoles.includes(role)))
        .map(item => ({
            key: item.key,
            icon: item.icon,
            label: <Link to={item.path}>{item.label}</Link>,
        }));

    // Handle logout with confirmation
    const handleLogout = () => {
        Modal.confirm({
            title: 'Xác nhận đăng xuất',
            content: 'Bạn có chắc chắn muốn đăng xuất khỏi hệ thống?',
            okText: 'Đăng xuất',
            cancelText: 'Hủy',
            okButtonProps: { danger: true },
            onOk: () => {
                logout();
                navigate('/login');
            },
        });
    };

    const userMenuItems: MenuProps['items'] = [
        {
            key: 'profile',
            icon: <UserOutlined />,
            label: 'Hồ sơ',
            onClick: () => navigate('/profile'),
        },
        {
            type: 'divider' as const,
        },
        {
            key: 'home',
            icon: <HomeOutlined />,
            label: 'Về trang chủ',
            onClick: () => navigate('/'),
        },
        {
            key: 'logout',
            icon: <LogoutOutlined />,
            label: 'Đăng xuất',
            onClick: handleLogout,
        },
    ];

    return (
        <Layout style={{ minHeight: '100vh' }}>
            {/* Mobile Drawer Menu */}
            {isMobile && (
                <Drawer
                    title={
                        <Link to="/admin" style={{ color: '#e50914', fontSize: 20, fontWeight: 'bold' }}>
                            ADMIN
                        </Link>
                    }
                    placement="left"
                    onClose={() => setMobileMenuOpen(false)}
                    open={mobileMenuOpen}
                    width={250}
                    styles={{ body: { padding: 0 } }}
                >
                    <Menu
                        theme="dark"
                        defaultSelectedKeys={['/admin']}
                        mode="inline"
                        items={menuItems}
                        onClick={({ key }) => {
                            navigate(key);
                            setMobileMenuOpen(false);
                        }}
                    />
                </Drawer>
            )}

            {/* Desktop Sider */}
            {!isMobile && (
                <Sider
                    collapsible
                    collapsed={collapsed}
                    onCollapse={setCollapsed}
                    breakpoint="lg"
                    collapsedWidth={80}
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
                            <Link to="/admin" style={{ color: '#e50914', fontSize: 20, fontWeight: 'bold' }}>
                                ADMIN
                            </Link>
                        )}
                        {collapsed && (
                            <span style={{ color: '#e50914', fontSize: 24, fontWeight: 'bold' }}>A</span>
                        )}
                    </div>
                    <Menu
                        theme="dark"
                        defaultSelectedKeys={['/admin']}
                        mode="inline"
                        items={menuItems}
                        onClick={({ key }) => navigate(key)}
                    />
                </Sider>
            )}

            <Layout style={{
                marginLeft: isMobile ? 0 : (collapsed ? 80 : 200),
                transition: 'margin-left 0.2s'
            }}>
                <Header style={{
                    padding: '0 24px',
                    background: '#1f1f1f',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    position: 'sticky',
                    top: 0,
                    zIndex: 99,
                }}>
                    {/* Mobile hamburger menu button */}
                    {isMobile && (
                        <MenuOutlined
                            style={{ fontSize: 20, color: '#fff', cursor: 'pointer' }}
                            onClick={() => setMobileMenuOpen(true)}
                        />
                    )}

                    {/* Role indicator */}
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                        <Tag color={getRoleColor(primaryRole)}>
                            {getRoleDisplayName(primaryRole)}
                        </Tag>
                    </div>

                    <Dropdown menu={{ items: userMenuItems }} placement="bottomRight">
                        <Space style={{ cursor: 'pointer' }}>
                            <Avatar icon={<UserOutlined />} style={{ backgroundColor: '#e50914' }} />
                            <span>{user?.username || user?.email}</span>
                        </Space>
                    </Dropdown>
                </Header>
                <Content style={{ margin: '24px 16px', padding: 24, minHeight: 280 }}>
                    <Breadcrumb style={{ marginBottom: 16 }} />
                    <Outlet />
                </Content>
                <Footer style={{ textAlign: 'center' }}>
                    Admin Dashboard ©{new Date().getFullYear()}
                </Footer>
            </Layout>
        </Layout>
    );
}

export default AdminLayout;
