// src/pages/dashboard/Header.tsx
import React from 'react';

interface HeaderProps {
    activeRole: string;
    activeItem: string;
    menuItems: Record<string, any[]>;
}

const HeaderMg: React.FC<HeaderProps> = ({ activeRole, activeItem, menuItems }) => {
    const getTitle = () => {
        const findTitle = (items: any[]): string | undefined => {
            for (const item of items) {
                if (item.id === activeItem) return item.title;
                if (item.children) {
                    const childTitle = findTitle(item.children);
                    if (childTitle) return childTitle;
                }
            }
            return undefined;
        };
        return findTitle(menuItems[activeRole]) || 'Dashboard';
    };

    return (
        <div className="bg-white border-b border-gray-200 px-6 py-4">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-xl font-semibold text-gray-800">
                        {getTitle()}
                    </h2>
                    <p className="text-sm text-gray-600 mt-1">
                        Vai trò hiện tại: <span className="font-medium text-blue-600">
                            {activeRole === 'employee' && 'Nhân viên'}
                            {activeRole === 'manager' && 'Quản lý rạp'}
                            {activeRole === 'admin' && 'Quản trị hệ thống'}
                        </span>
                    </p>
                </div>
                {/* Add user info, logout here if needed */}
            </div>
        </div>
    );
};

export default HeaderMg;