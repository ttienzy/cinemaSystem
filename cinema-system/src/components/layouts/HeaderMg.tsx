import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { LogOut, User, ChevronDown } from 'lucide-react'; // Assuming you use lucide-react for icons
import { jwtDecode } from 'jwt-decode';

// --- COMPONENT PROPS INTERFACE ---
interface HeaderProps {
    activeRole: string;
    activeItem: string;
    menuItems: Record<string, any[]>; // Using 'any' for flexibility with menu structure
}

const HeaderMg: React.FC<HeaderProps> = ({ activeRole, activeItem, menuItems }) => {
    // --- STATE MANAGEMENT ---
    // This state controls the visibility of the user dropdown menu.
    const [isDropdownOpen, setIsDropdownOpen] = useState(false);

    // --- HOOKS ---
    // The 'useNavigate' hook from react-router-dom allows for programmatic navigation.
    const navigate = useNavigate();
    // The 'useRef' hook is used to get a reference to the dropdown DOM element.
    const dropdownRef = useRef<HTMLDivElement>(null);
    const decodedToken = jwtDecode<any>(localStorage.getItem('accessToken') || '');

    // --- MOCK USER DATA ---
    // In a real application, this user data might come from a global state (Context/Redux).
    // For this example, we'll try to get it from localStorage.
    const user = {
        name: decodedToken?.name || 'Belong System',
        email: decodedToken?.email || 'admin@example.com',
    };

    // --- SIDE EFFECTS ---
    // This useEffect hook handles closing the dropdown when the user clicks outside of it.
    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            // Check if the click is outside the dropdown container.
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setIsDropdownOpen(false);
            }
        };

        // Add the event listener only when the dropdown is open.
        if (isDropdownOpen) {
            document.addEventListener('mousedown', handleClickOutside);
        }

        // Cleanup function: remove the event listener when the component unmounts or the dropdown closes.
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, [isDropdownOpen]);

    // --- EVENT HANDLERS ---
    // This function handles the entire logout process.
    const handleLogout = () => {
        localStorage.clear();
        // Step 2: Navigate the user to the /login page.
        navigate('/login');
    };

    // --- HELPER FUNCTIONS ---
    // This function maps the role key (e.g., 'manager') to a user-friendly display name.
    const getRoleName = (role: string): string => {
        const roleMap: Record<string, string> = {
            employee: 'Nhân viên',
            manager: 'Quản lý rạp',
            admin: 'Quản trị hệ thống',
        };
        return roleMap[role] || 'Unknown Role';
    };

    // This function recursively finds the title of the currently active menu item.
    const getTitle = (): string => {
        const findTitleRecursively = (items: any[]): string | undefined => {
            for (const item of items) {
                if (item.id === activeItem) {
                    return item.title;
                }
                if (item.children) {
                    const childTitle = findTitleRecursively(item.children);
                    if (childTitle) {
                        return childTitle;
                    }
                }
            }
            return undefined;
        };

        // Start the search from the menu items corresponding to the user's active role.
        const itemsForRole = menuItems[activeRole] || [];
        return findTitleRecursively(itemsForRole) || 'Dashboard'; // Provide a default title.
    };

    // --- RENDER ---
    return (
        <header className="sticky top-0 z-30 bg-white border-b border-gray-200 px-4 sm:px-6 py-3">
            <div className="flex items-center justify-between">
                {/* Left Section: Page Title and Current Role */}
                <div>
                    <h1 className="text-xl font-semibold text-gray-800">{getTitle()}</h1>
                    <p className="text-sm text-gray-500 mt-1">
                        Vai trò hiện tại: <span className="font-medium text-indigo-600">{getRoleName(activeRole)}</span>
                    </p>
                </div>

                {/* Right Section: User Menu and Logout Functionality */}
                <div className="relative" ref={dropdownRef}>
                    {/* Button to toggle the dropdown menu visibility */}
                    <button
                        onClick={() => setIsDropdownOpen(!isDropdownOpen)}
                        className="flex items-center gap-2 p-2 rounded-lg hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-colors"
                    >
                        <div className="w-9 h-9 bg-gray-200 rounded-full flex items-center justify-center">
                            <User className="w-5 h-5 text-gray-600" />
                        </div>
                        <div className="hidden md:flex flex-col items-start">
                            <span className="text-sm font-semibold text-gray-700">{user.name}</span>
                            <span className="text-xs text-gray-500">{getRoleName(activeRole)}</span>
                        </div>
                        <ChevronDown
                            className={`w-4 h-4 text-gray-500 transition-transform duration-200 ${isDropdownOpen ? 'rotate-180' : ''}`}
                        />
                    </button>

                    {/* Dropdown Menu: Conditionally rendered based on 'isDropdownOpen' state */}
                    {isDropdownOpen && (
                        <div className="absolute right-0 mt-2 w-56 bg-white rounded-lg shadow-xl border border-gray-200 overflow-hidden animate-fade-in-down">
                            {/* User info section inside the dropdown */}
                            <div className="p-4 border-b border-gray-100">
                                <p className="font-semibold text-gray-800 text-sm truncate">{user.name}</p>
                                <p className="text-xs text-gray-500 truncate">{user.email}</p>
                            </div>

                            {/* Logout Button */}
                            <button
                                onClick={handleLogout}
                                className="w-full text-left px-4 py-3 text-sm text-gray-700 hover:bg-red-50 hover:text-red-600 flex items-center gap-3 transition-colors"
                            >
                                <LogOut className="w-4 h-4" />
                                <span>Đăng xuất</span>
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </header>
    );
};

export default HeaderMg;