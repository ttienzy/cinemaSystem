import { Navigate, useLocation } from 'react-router-dom';
import { Spin } from 'antd';
import { useAuthStore } from '../../../features/auth/store/authStore';
import { getRedirectPath } from '../../utils/authRedirect';

interface PublicRouteProps {
    children: React.ReactNode;
}

export function PublicRoute({ children }: PublicRouteProps) {
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

    // Redirect to home if already authenticated
    if (isAuthenticated) {
        // Check if there's a return URL in state
        const from = (location.state as { from?: { pathname: string } })?.from?.pathname;
        if (from && from !== '/login' && from !== '/register') {
            return <Navigate to={from} replace />;
        }

        const redirectPath = getRedirectPath(user?.roles || []);
        return <Navigate to={redirectPath} replace />;
    }

    return <>{children}</>;
}

export default PublicRoute;
