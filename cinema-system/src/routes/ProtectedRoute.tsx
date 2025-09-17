import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAppSelector } from '../hooks/redux';

interface ProtectedRouteProps {
    children: React.ReactNode;
    requiredRole?: string; // Optional role check
}
export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, requiredRole }) => {
    const { isAuthenticated, user } = useAppSelector((state) => state.auth);
    const location = useLocation();

    if (!isAuthenticated) {
        console.log('User is not authenticated');
        // slow 2 seconds before redirecting
        setTimeout(() => { }, 2000);
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    if (requiredRole && user?.role !== requiredRole) {
        setTimeout(() => { }, 2000);
        return <Navigate to="/unauthorized" replace />;
    }
    return <>{children}</>;
}