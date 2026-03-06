import { Navigate, useLocation } from 'react-router-dom';
import { Spin } from 'antd';
import { useAuthStore } from '../../../features/auth/store/authStore';

interface ProtectedRouteProps {
    children: React.ReactNode;
    allowedRoles?: string[];
}

export function ProtectedRoute({ children, allowedRoles }: ProtectedRouteProps) {
    const location = useLocation();
    const { isAuthenticated, isLoading, user } = useAuthStore();

    // Show loading spinner while checking auth
    if (isLoading) {
        return (
            <div style={{
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                height: '100vh'
            }}>
                <Spin size="large" />
            </div>
        );
    }

    // Redirect to login if not authenticated
    if (!isAuthenticated) {
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    // Check roles if specified (case-insensitive comparison)
    if (allowedRoles && allowedRoles.length > 0) {
        const userRoles = user?.roles?.map(r => r.toLowerCase()) || [];
        const normalizedAllowedRoles = allowedRoles.map(r => r.toLowerCase());
        const hasAllowedRole = userRoles.some(role =>
            normalizedAllowedRoles.includes(role)
        );
        if (!hasAllowedRole) {
            return <Navigate to="/403" replace />;
        }
    }

    return <>{children}</>;
}

export default ProtectedRoute;
