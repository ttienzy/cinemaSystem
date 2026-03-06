import { Breadcrumb as AntBreadcrumb } from 'antd';
import { Link, useLocation } from 'react-router-dom';
import { HomeOutlined } from '@ant-design/icons';

// Route to title mapping for automatic path-to-title conversion
const routeTitleMap: Record<string, string> = {
    // Admin routes
    '/admin': 'Dashboard',
    '/admin/movies': 'Quản lý phim',
    '/admin/movies/create': 'Thêm phim mới',
    '/admin/movies/:id': 'Chi tiết phim',
    '/admin/cinemas': 'Quản lý rạp',
    '/admin/cinemas/create': 'Thêm rạp mới',
    '/admin/cinemas/:id': 'Chi tiết rạp',
    '/admin/showtimes': 'Quản lý suất chiếu',
    '/admin/showtimes/create': 'Thêm suất chiếu',
    '/admin/bookings': 'Quản lý đặt vé',
    '/admin/bookings/:id': 'Chi tiết đặt vé',
    '/admin/users': 'Quản lý người dùng',
    '/admin/users/create': 'Thêm người dùng',
    '/admin/staff': 'Quản lý nhân viên',
    '/admin/equipment': 'Quản lý thiết bị',

    // App routes
    '/': 'Trang chủ',
    '/movies': 'Phim',
    '/movies/:id': 'Chi tiết phim',
    '/cinemas': 'Rạp chiếu',
    '/cinemas/:id': 'Chi tiết rạp',
    '/booking': 'Đặt vé',
    '/booking/success': 'Đặt vé thành công',
    '/booking/failed': 'Đặt vé thất bại',
    '/profile': 'Hồ sơ',
    '/profile/history': 'Lịch sử đặt vé',
    '/profile/booking/:id': 'Chi tiết đặt vé',

    // Auth routes
    '/login': 'Đăng nhập',
    '/register': 'Đăng ký',
};

// Breadcrumb item interface
export interface BreadcrumbItem {
    title: string;
    path?: string;
    isHome?: boolean;
}

interface BreadcrumbProps {
    customItems?: BreadcrumbItem[];
    autoGenerate?: boolean;
    style?: React.CSSProperties;
}

/**
 * Convert path pattern to regex for matching dynamic routes
 */
const pathToRegex = (pattern: string, path: string): boolean => {
    const regexPattern = pattern
        .replace(/:[^/]+/g, '[^/]+')
        .replace(/\//g, '\\/');
    const regex = new RegExp(`^${regexPattern}$`);
    return regex.test(path);
};

/**
 * Get title for a path from the route map
 */
const getTitleForPath = (path: string): string => {
    // Check exact match first
    if (routeTitleMap[path]) {
        return routeTitleMap[path];
    }

    // Check pattern match for dynamic routes
    for (const [pattern, title] of Object.entries(routeTitleMap)) {
        if (pathToRegex(pattern, path)) {
            return title;
        }
    }

    // Fallback: capitalize the last segment
    const segments = path.split('/').filter(Boolean);
    if (segments.length > 0) {
        const lastSegment = segments[segments.length - 1];
        return lastSegment.charAt(0).toUpperCase() + lastSegment.slice(1);
    }

    return path;
};

/**
 * Generate breadcrumb items from current path
 */
const generateBreadcrumbs = (pathname: string): BreadcrumbItem[] => {
    const items: BreadcrumbItem[] = [];

    // Always add home first
    items.push({
        title: 'Trang chủ',
        path: '/',
        isHome: true,
    });

    // Skip auth routes
    if (pathname.startsWith('/login') || pathname.startsWith('/register')) {
        return items;
    }

    // Split path into segments
    const segments = pathname.split('/').filter(Boolean);
    let currentPath = '';

    // Determine base based on admin or app route
    const isAdminRoute = segments[0] === 'admin';

    if (isAdminRoute) {
        items.push({
            title: 'Admin',
            path: '/admin',
        });
    }

    for (let i = 0; i < segments.length; i++) {
        const segment = segments[i];

        // Skip 'admin' as it's handled above
        if (i === 0 && segment === 'admin') continue;

        currentPath += `/${segment}`;

        // Skip if it's a dynamic parameter (starts with :)
        if (segment.startsWith(':')) continue;

        const title = getTitleForPath(currentPath);

        // Don't add duplicate if same as previous
        const lastItem = items[items.length - 1];
        if (lastItem && lastItem.title === title && lastItem.path === currentPath) {
            continue;
        }

        items.push({
            title,
            path: currentPath,
        });
    }

    return items;
};

/**
 * Breadcrumb component with automatic path-to-title mapping
 * Supports custom items for dynamic content like movie names
 */
function Breadcrumb({ customItems, autoGenerate = true, style }: BreadcrumbProps) {
    const location = useLocation();

    // Use custom items if provided, otherwise auto-generate from path
    const items = customItems || (autoGenerate ? generateBreadcrumbs(location.pathname) : []);

    // Don't render if only home
    if (items.length <= 1) {
        return null;
    }

    return (
        <AntBreadcrumb style={style} items={items.map((item) => ({
            key: item.path || item.title,
            title: item.isHome ? (
                <Link to={item.path || '/'}>
                    <HomeOutlined />
                </Link>
            ) : item.path ? (
                <Link to={item.path}>{item.title}</Link>
            ) : (
                item.title
            ),
        }))} />
    );
}

export default Breadcrumb;
export type { BreadcrumbProps };
