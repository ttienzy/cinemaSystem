import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { Spin } from 'antd';
import { useAuthStore } from '../../features/auth/authStore';

interface ProtectedRouteProps {
  roles?: string[];
}

export function ProtectedRoute({ roles = ['Admin'] }: ProtectedRouteProps) {
  const location = useLocation();
  const { user, isAuthenticated, isLoading } = useAuthStore();

  if (isLoading) {
    return (
      <div style={{ minHeight: '100vh', display: 'grid', placeItems: 'center' }}>
        <Spin size="large" />
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  const hasRole = roles.length === 0 || roles.some((role) => user?.roles.includes(role));
  if (!hasRole) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <Outlet />;
}
