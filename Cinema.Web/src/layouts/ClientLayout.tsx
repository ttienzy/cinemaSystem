import { useMemo } from 'react';
import { Avatar, Button, Dropdown, Layout, Menu, Space, Typography } from 'antd';
import {
  HomeOutlined,
  LoginOutlined,
  LogoutOutlined,
  MenuOutlined,
  UserOutlined,
  VideoCameraOutlined,
} from '@ant-design/icons';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../features/auth/useAuth';

const { Header, Content } = Layout;
const { Text } = Typography;

const navItems = [
  { key: '/', icon: <HomeOutlined />, label: 'Home' },
  { key: '/movies', icon: <VideoCameraOutlined />, label: 'Movies' },
];

export default function ClientLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout, isAuthenticated } = useAuth();

  const selectedKey = useMemo(() => {
    const match = navItems
      .filter((item) => item.key !== '/')
      .find((item) => location.pathname.startsWith(item.key));

    return match?.key ?? '/';
  }, [location.pathname]);

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header className="client-header">
        <Space size={12} className="brand" onClick={() => navigate('/')}>
          <MenuOutlined />
          <span>Cinema Web</span>
        </Space>

        <Menu
          mode="horizontal"
          selectedKeys={[selectedKey]}
          items={navItems}
          onClick={({ key }) => navigate(key)}
          className="client-nav"
        />

        {isAuthenticated ? (
          <Dropdown
            trigger={['click']}
            menu={{
              items: [
                {
                  key: 'user',
                  icon: <UserOutlined />,
                  label: user?.email ?? 'Customer',
                  disabled: true,
                },
                { type: 'divider' },
                {
                  key: 'logout',
                  icon: <LogoutOutlined />,
                  label: 'Logout',
                  onClick: logout,
                },
              ],
            }}
          >
            <Space style={{ cursor: 'pointer' }}>
              <Avatar icon={<UserOutlined />} />
              <Text className="account-name">{user?.fullName || user?.email}</Text>
            </Space>
          </Dropdown>
        ) : (
          <Button icon={<LoginOutlined />} type="primary" onClick={() => navigate('/login')}>
            Login
          </Button>
        )}
      </Header>

      <Content>
        <Outlet />
      </Content>
    </Layout>
  );
}
