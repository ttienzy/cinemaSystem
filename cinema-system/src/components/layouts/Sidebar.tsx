// src/pages/dashboard/Sidebar.tsx
import React from 'react';
import {
    ChevronDown,
    ChevronRight,
} from 'lucide-react';
import { menuItems, type MenuItem } from '../../types/dashboard.types';


interface SidebarProps {
    activeRole: string;
    activeItem: string;
    expandedSections: Record<string, boolean>;
    setActiveItem: (id: string) => void;
    toggleSection: (id: string) => void;
    setActiveRole: (role: string) => void;
    setExpandedSections: (sections: Record<string, boolean>) => void;
}



const Sidebar: React.FC<SidebarProps> = ({
    activeRole,
    activeItem,
    expandedSections,
    setActiveItem,
    toggleSection,
    setActiveRole,
    setExpandedSections
}) => {
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
                            setActiveItem(item.id);
                        }
                    }}
                >
                    <div className="flex items-center">
                        {Icon && <Icon className="w-5 h-5 mr-3" />}
                        <span className="text-sm font-medium">{item.title}</span>
                    </div>
                    {hasChildren && (
                        <div className="text-gray-400">
                            {isExpanded ? <ChevronDown className="w-4 h-4" /> : <ChevronRight className="w-4 h-4" />}
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

    return (
        <div className="w-80 bg-gray-800 text-white flex flex-col">
            {/* Header */}
            <div className="p-6 border-b border-gray-700">
                <h1 className="text-xl font-bold text-white">Cinema Management</h1>
                <p className="text-gray-400 text-sm mt-1">Hệ thống quản lý rạp chiếu phim</p>
            </div>

            {/* Role Selector */}
            <div className="p-4 border-b border-gray-700">
                <label className="block text-sm font-medium text-gray-300 mb-2">
                    Chọn vai trò:
                </label>
                <select
                    title='Select Role'
                    value={activeRole}
                    onChange={(e) => {
                        setActiveRole(e.target.value);
                        setActiveItem('dashboard');
                        setExpandedSections({});
                    }}
                    className="w-full p-2 bg-gray-700 border border-gray-600 rounded text-white text-sm focus:border-blue-500 focus:outline-none"
                >
                    <option value="employee">Employee (Nhân viên)</option>
                    <option value="manager">Manager (Quản lý)</option>
                    <option value="admin">Admin (Quản trị)</option>
                </select>
            </div>

            {/* Navigation Menu */}
            <nav className="flex-1 overflow-y-auto">
                <div className="py-4">
                    {menuItems[activeRole].map(item => renderMenuItem(item))}
                </div>
            </nav>

            {/* Footer */}
            <div className="p-4 border-t border-gray-700">
                <div className="text-xs text-gray-400">
                    <div className="font-medium mb-1">Đăng nhập với vai trò:</div>
                    <div className="capitalize text-blue-400 font-medium">
                        {activeRole === 'employee' && 'Nhân viên'}
                        {activeRole === 'manager' && 'Quản lý rạp'}
                        {activeRole === 'admin' && 'Quản trị hệ thống'}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Sidebar;