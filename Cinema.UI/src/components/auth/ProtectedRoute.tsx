// Protected Route - Tầng 1: Hàng rào bảo vệ
import { Navigate, useLocation, Outlet } from 'react-router-dom';
import { Spin } from 'antd';
import { useAuth } from '../../hooks/useAuth';

interface ProtectedRouteProps {
    allowedRoles?: string[];
}

export const ProtectedRoute = ({ allowedRoles }: ProtectedRouteProps) => {
    const { user, isAuthenticated, isLoading } = useAuth();
    const location = useLocation();

    // Đang loading (kiểm tra token từ localStorage)
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

    // Chưa đăng nhập -> redirect về login
    if (!isAuthenticated) {
        // Lưu lại trang user định vào để sau khi login xong thì quay lại đó
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    // Kiểm tra role nếu có yêu cầu
    if (allowedRoles && allowedRoles.length > 0) {
        const hasRequiredRole = allowedRoles.some(role => user?.roles.includes(role));

        if (!hasRequiredRole) {
            return <Navigate to="/unauthorized" replace />;
        }
    }

    // Cho phép vào các trang con
    return <Outlet />;
};
