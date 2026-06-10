import { Layout, Menu, Button, Avatar, Dropdown, Space, Typography, Grid } from 'antd';
import {
  AppstoreOutlined,
  BankOutlined,
  CalendarOutlined,
  DashboardOutlined,
  LogoutOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  TagOutlined,
  UserOutlined,
  VideoCameraOutlined,
} from '@ant-design/icons';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useMemo, useState } from 'react';
import { useAuth } from '../features/auth/useAuth';

const { Header, Sider, Content } = Layout;
const { Text } = Typography;

const navItems = [
  { key: '/', icon: <DashboardOutlined />, label: 'Dashboard' },
  { key: '/movies', icon: <VideoCameraOutlined />, label: 'Movies' },
  { key: '/cinemas', icon: <BankOutlined />, label: 'Cinemas' },
  { key: '/showtimes', icon: <CalendarOutlined />, label: 'Showtimes' },
  { key: '/tickets', icon: <TagOutlined />, label: 'Tickets' },
];

export default function AdminLayout() {
  const [collapsed, setCollapsed] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const screens = Grid.useBreakpoint();
  const { user, logout } = useAuth();

  const selectedKey = useMemo(() => {
    const match = navItems
      .filter((item) => item.key !== '/')
      .find((item) => location.pathname.startsWith(item.key));

    return match?.key ?? '/';
  }, [location.pathname]);

  const siderCollapsed = screens.md ? collapsed : true;

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        trigger={null}
        collapsible
        collapsed={siderCollapsed}
        width={248}
        style={{ borderRight: '1px solid rgba(255,255,255,0.08)' }}
      >
        <div
          style={{
            height: 64,
            display: 'flex',
            alignItems: 'center',
            gap: 10,
            padding: '0 18px',
            color: '#fff',
            fontWeight: 700,
          }}
        >
          <AppstoreOutlined style={{ fontSize: 22 }} />
          {!siderCollapsed && <span>Cinema Admin</span>}
        </div>

        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[selectedKey]}
          items={navItems}
          onClick={({ key }) => navigate(key)}
        />
      </Sider>

      <Layout>
        <Header
          style={{
            height: 64,
            padding: '0 20px',
            background: '#fff',
            borderBottom: '1px solid #eaecf0',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
          }}
        >
          <Button
            type="text"
            aria-label="Toggle menu"
            icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            onClick={() => setCollapsed((value) => !value)}
            style={{ width: 40, height: 40 }}
          />

          <Dropdown
            trigger={['click']}
            menu={{
              items: [
                {
                  key: 'user',
                  icon: <UserOutlined />,
                  label: user?.email ?? 'Admin',
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
              {screens.sm && <Text strong>{user?.fullName || user?.email}</Text>}
            </Space>
          </Dropdown>
        </Header>

        <Content style={{ padding: 24, minHeight: 0 }}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
}
