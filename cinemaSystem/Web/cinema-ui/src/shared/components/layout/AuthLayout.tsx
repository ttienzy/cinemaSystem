import { Outlet, Link } from 'react-router-dom';
import { Layout } from 'antd';

const { Content } = Layout;

function AuthLayout() {
    return (
        <Layout style={{ minHeight: '100vh', background: '#0a0a0a' }}>
            <div style={{
                position: 'absolute',
                top: 0,
                left: 0,
                right: 0,
                bottom: 0,
                backgroundImage: 'linear-gradient(rgba(0, 0, 0, 0.7), rgba(0, 0, 0, 0.9)), url(/bg-auth.jpg)',
                backgroundSize: 'cover',
                backgroundPosition: 'center',
                zIndex: 0,
            }} />
            <Content style={{
                position: 'relative',
                zIndex: 1,
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                padding: '24px',
            }}>
                <div style={{
                    width: '100%',
                    maxWidth: 440,
                }}>
                    <div style={{ textAlign: 'center', marginBottom: 32 }}>
                        <Link to="/" style={{ color: '#e50914', fontSize: 36, fontWeight: 'bold' }}>
                            CINEMA
                        </Link>
                        <p style={{ color: '#a3a3a3', marginTop: 8 }}>
                            Hệ thống đặt vé xem phim
                        </p>
                    </div>
                    <Outlet />
                </div>
            </Content>
        </Layout>
    );
}

export default AuthLayout;
