// Public Route - Redirect nếu đã login (tránh vào /login khi đã đăng nhập)
import { Navigate } from 'react-router-dom';
import { Spin } from 'antd';
import { useAuth } from '../../hooks/useAuth';

interface PublicRouteProps {
    children: React.ReactNode;
}

export const PublicRoute = ({ children }: PublicRouteProps) => {
    const { isAuthenticated, isLoading, user } = useAuth();

    // Đang loading
    if (isLoading) {
        return (
            <div style={{
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                height: '100vh'
            }}>
                <Spin size="large" tip="Đang tải..." />
            </div>
        );
    }

    // Đã login -> redirect về trang phù hợp
    if (isAuthenticated) {
        // Redirect dựa vào role
        if (user?.roles.includes('Admin')) {
            return <Navigate to="/admin" replace />;
        }
        return <Navigate to="/" replace />;
    }

    // Chưa login -> cho vào trang public
    return <>{children}</>;
};
