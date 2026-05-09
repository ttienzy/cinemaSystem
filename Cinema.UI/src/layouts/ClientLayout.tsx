import React from 'react';
import { Layout, Menu, Button, Dropdown, Avatar } from 'antd';
import { UserOutlined, LogoutOutlined, LoginOutlined } from '@ant-design/icons';
import { Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

const { Header, Content, Footer } = Layout;

const items = [
  { key: '/', label: 'Home' },
  { key: '/movies', label: 'Now Showing' },
  { key: '/cinemas', label: 'Cinemas' },
];

const ClientLayout: React.FC = () => {
  const navigate = useNavigate();
  const { user, isAuthenticated, logout } = useAuth();

  const userMenuItems = [
    {
      key: 'profile',
      icon: <UserOutlined />,
      label: user?.email || 'User',
      disabled: true,
    },
    {
      type: 'divider' as const,
    },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Đăng xuất',
      onClick: logout,
    },
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <div style={{ display: 'flex', alignItems: 'center', flex: 1 }}>
          <div
            style={{
              color: 'white',
              fontSize: 20,
              fontWeight: 'bold',
              marginRight: 40,
              cursor: 'pointer'
            }}
            onClick={() => navigate('/')}
          >
            🎬 CinemaHub
          </div>
          <Menu
            theme="dark"
            mode="horizontal"
            defaultSelectedKeys={['/']}
            items={items}
            style={{ flex: 1, minWidth: 0 }}
            onClick={({ key }) => navigate(key)}
          />
        </div>

        <div style={{ marginLeft: 'auto' }}>
          {isAuthenticated ? (
            <Dropdown menu={{ items: userMenuItems }} placement="bottomRight">
              <div style={{
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                gap: 8,
                color: 'white'
              }}>
                <Avatar icon={<UserOutlined />} style={{ backgroundColor: '#1890ff' }} />
                <span style={{ fontWeight: 500 }}>{user?.fullName || user?.email}</span>
              </div>
            </Dropdown>
          ) : (
            <Button
              type="primary"
              icon={<LoginOutlined />}
              onClick={() => navigate('/login')}
            >
              Đăng nhập
            </Button>
          )}
        </div>
      </Header>
      <Content style={{ padding: '0 48px', marginTop: 24 }}>
        <div
          style={{
            background: '#fff',
            minHeight: 280,
            padding: 24,
            borderRadius: 8,
          }}
        >
          <Outlet />
        </div>
      </Content>
      <Footer style={{ textAlign: 'center' }}>
        CinemaHub Design ©{new Date().getFullYear()} Created by Cinema Team
      </Footer>
    </Layout>
  );
};

export default ClientLayout;
