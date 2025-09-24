import React from 'react';
import { useNavigate } from 'react-router-dom';
import { ChevronDown, ChevronRight } from 'lucide-react';
import { menuItems, type MenuItem } from '../../types/dashboard.types';

interface SidebarProps {
    userRole: string;
    activeItem: string;
    expandedSections: Record<string, boolean>;
    toggleSection: (id: string) => void;
}

const Sidebar: React.FC<SidebarProps> = ({
    userRole,
    activeItem,
    expandedSections,
    toggleSection,
}) => {
    const navigate = useNavigate();

    const getRouteForItem = (itemId: string) => {
        // Map menu item IDs to their corresponding routes based on user role
        const routeMap: Record<string, string> = {
            // Dashboard routes for all roles
            'dashboard': `/dashboard/${userRole}`,

            // Employee routes
            'new-sale': '/dashboard/employee/new-sale',
            'sales-history': '/dashboard/employee/sales-history',
            'cash-payment': '/dashboard/employee/cash-payment',
            'card-payment': '/dashboard/employee/card-payment',
            'checkin': '/dashboard/employee/checkin',
            'schedule': '/dashboard/employee/schedule',
            'equipment-report': '/dashboard/employee/equipment-report',

            // Manager routes
            'create-showtime': '/dashboard/manager/create-showtime',
            'manage-showtimes': '/dashboard/manager/manage-showtimes',
            'showtime-pricing': '/dashboard/manager/showtime-pricing',
            'staff-list': '/dashboard/manager/staff-list',
            'shift-assignment': '/dashboard/manager/shift-assignment',
            'staff-performance': '/dashboard/manager/staff-performance',
            'inventory-list': '/dashboard/manager/inventory-list',
            'stock-control': '/dashboard/manager/stock-control',
            'restock': '/dashboard/manager/restock',
            'equipment-list': '/dashboard/manager/equipment-list',
            'maintenance-schedule': '/dashboard/manager/maintenance-schedule',
            'maintenance-history': '/dashboard/manager/maintenance-history',
            'daily-revenue': '/dashboard/manager/daily-revenue',
            'monthly-report': '/dashboard/manager/monthly-report',
            'movie-performance': '/dashboard/manager/movie-performance',

            // Admin routes
            'cinema-list': '/dashboard/admin/cinema-list',
            'cinema-create': '/dashboard/admin/cinema-create',
            'cinema-settings': '/dashboard/admin/cinema-settings',
            'screen-management': '/dashboard/admin/screen-management',
            'seat-layout': '/dashboard/admin/seat-layout',
            'seat-types': '/dashboard/admin/seat-types',
            'movie-list': '/dashboard/admin/movie-list',
            'movie-create': '/dashboard/admin/movie-create',
            'movie-genres': '/dashboard/admin/movie-genres',
            'movie-cast-crew': '/dashboard/admin/movie-cast-crew',
            'pricing-tiers': '/dashboard/admin/pricing-tiers',
            'seat-pricing': '/dashboard/admin/seat-pricing',
            'time-slots': '/dashboard/admin/time-slots',
            'overall-revenue': '/dashboard/admin/overall-revenue',
            'cinema-performance': '/dashboard/admin/cinema-performance',
            'system-analytics': '/dashboard/admin/system-analytics',
            'general-settings': '/dashboard/admin/general-settings',
            'user-roles': '/dashboard/admin/user-roles',
            'system-backup': '/dashboard/admin/system-backup'
        };

        return routeMap[itemId] || `/dashboard/${userRole}`;
    };

    const renderMenuItem = (item: MenuItem, depth = 0) => {
        const Icon = item.icon;
        const hasChildren = item.children && item.children.length > 0;
        const isExpanded = expandedSections[item.id];
        const isActive = activeItem === item.id;

        return (
            <div key={item.id}>
                <div
                    className={`flex items-center justify-between px-4 py-2 cursor-pointer transition-colors ${isActive
                        ? 'bg-blue-600 text-white'
                        : 'text-gray-300 hover:bg-gray-700 hover:text-white'
                        } ${depth > 0 ? 'pl-8' : ''}`}
                    onClick={() => {
                        if (hasChildren) {
                            toggleSection(item.id);
                        } else {
                            navigate(getRouteForItem(item.id));
                        }
                    }}
                >
                    <div className="flex items-center">
                        {Icon && <Icon className="w-5 h-5 mr-3" />}
                        <span className="text-sm font-medium">{item.title}</span>
                    </div>
                    {hasChildren && (
                        <div className="text-gray-400">
                            {isExpanded ? (
                                <ChevronDown className="w-4 h-4" />
                            ) : (
                                <ChevronRight className="w-4 h-4" />
                            )}
                        </div>
                    )}
                </div>
                {hasChildren && isExpanded && (
                    <div className="bg-gray-750">
                        {item.children?.map(child => renderMenuItem(child, depth + 1))}
                    </div>
                )}
            </div>
        );
    };

    const getRoleDisplayName = (role: string) => {
        const roleNames = {
            employee: 'Nhân viên',
            manager: 'Quản lý rạp',
            admin: 'Quản trị hệ thống'
        };
        return roleNames[role as keyof typeof roleNames] || role;
    };

    return (
        <div className="w-80 bg-gray-800 text-white flex flex-col">
            {/* Header */}
            <div className="p-6 border-b border-gray-700">
                <h1 className="text-xl font-bold text-white">Cinema Management</h1>
                <p className="text-gray-400 text-sm mt-1">Hệ thống quản lý rạp chiếu phim</p>
            </div>

            {/* Navigation Menu */}
            <nav className="flex-1 overflow-y-auto">
                <div className="py-4">
                    {menuItems[userRole] && menuItems[userRole].map(item => renderMenuItem(item))}
                </div>
            </nav>

            {/* Footer */}
            <div className="p-4 border-t border-gray-700">
                <div className="text-xs text-gray-400">
                    <div className="font-medium mb-1">Đăng nhập với vai trò:</div>
                    <div className="capitalize text-blue-400 font-medium">
                        {getRoleDisplayName(userRole)}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Sidebar;