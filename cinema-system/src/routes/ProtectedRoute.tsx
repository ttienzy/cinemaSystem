import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAppSelector, useAppDispatch } from '../hooks/redux';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import { setUser } from '../store/slices/authSlice';
import { decodeTokenAndGetUser } from '../utils/decodeTokenAndGetUser';

interface ProtectedRouteProps {
    children: React.ReactNode;
    allowedRoles: string[];
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, allowedRoles }) => {
    const { isAuthenticated, user, loading } = useAppSelector((state) => state.auth);
    const dispatch = useAppDispatch();
    const location = useLocation();

    if (loading) {
        return <LoadingSpinner />;
    }

    if (!isAuthenticated) {
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    if (allowedRoles && allowedRoles.length > 0) {
        const roles = Array.isArray(user.role) ? user.role : [user.role];

        if (!roles || roles.length === 0) {
            const token = localStorage.getItem('accessToken');
            if (token) {
                dispatch(setUser());
                // Decode lại từ localStorage để có role ngay lập tức
                const userFromToken = decodeTokenAndGetUser(token);
                if (userFromToken && userFromToken.role) {
                    const rolesFromToken = Array.isArray(userFromToken.role) ? userFromToken.role : [userFromToken.role];
                    const hasPermission = rolesFromToken.some(role =>
                        allowedRoles.includes(role.trim().toLowerCase())
                    );
                    if (!hasPermission) {
                        return <Navigate to="/forbidden" replace />;
                    }
                    return <>{children}</>;
                }
            }
            return <Navigate to="/forbidden" replace />;
        }

        const hasPermission = roles.some(role => allowedRoles.includes(role.trim().toLowerCase()));

        if (!hasPermission) {
            return <Navigate to="/forbidden" replace />;
        }
    }

    return <>{children}</>;
};