import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';

// Default page titles
const defaultTitles: Record<string, string> = {
    '/': 'Cinema System - Trang chủ',
    '/movies': 'Phim - Cinema System',
    '/cinemas': 'Rạp chiếu - Cinema System',
    '/booking': 'Đặt vé - Cinema System',
    '/booking/success': 'Đặt vé thành công - Cinema System',
    '/booking/failed': 'Đặt vé thất bại - Cinema System',
    '/profile': 'Hồ sơ - Cinema System',
    '/profile/history': 'Lịch sử đặt vé - Cinema System',
    '/login': 'Đăng nhập - Cinema System',
    '/register': 'Đăng ký - Cinema System',
    '/admin': 'Admin Dashboard - Cinema System',
    '/admin/movies': 'Quản lý phim - Admin',
    '/admin/cinemas': 'Quản lý rạp - Admin',
    '/admin/showtimes': 'Quản lý suất chiếu - Admin',
    '/admin/bookings': 'Quản lý đặt vé - Admin',
    '/admin/users': 'Quản lý người dùng - Admin',
    '/admin/staff': 'Quản lý nhân viên - Admin',
    '/admin/equipment': 'Quản lý thiết bị - Admin',
};

// Get default title for path
const getDefaultTitle = (pathname: string): string => {
    // Check exact match first
    if (defaultTitles[pathname]) {
        return defaultTitles[pathname];
    }

    // Check pattern match for dynamic routes
    for (const [pattern, title] of Object.entries(defaultTitles)) {
        const regexPattern = pattern
            .replace(/:[^/]+/g, '[^/]+')
            .replace(/\//g, '\\/');
        const regex = new RegExp(`^${regexPattern}$`);
        if (regex.test(pathname)) {
            return title;
        }
    }

    // Fallback
    return 'Cinema System';
};

/**
 * Custom hook for managing page titles
 * Automatically updates document.title based on current route
 * Supports custom titles for dynamic content
 */
export const usePageTitle = (customTitle?: string) => {
    const location = useLocation();

    useEffect(() => {
        if (customTitle) {
            document.title = customTitle;
        } else {
            document.title = getDefaultTitle(location.pathname);
        }
    }, [location.pathname, customTitle]);
};

/**
 * Hook to get current page title without setting it
 */
export const useCurrentPageTitle = (): string => {
    const location = useLocation();
    return getDefaultTitle(location.pathname);
};

export default usePageTitle;
