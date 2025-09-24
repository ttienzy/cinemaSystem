import { useState } from 'react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAppSelector } from '../../hooks/redux';
import { getRole } from '../../utils/decodeTokenAndGetUser';
import MainMg from '../../components/layouts/MainMg';

const DashboardLayout: React.FC = () => {
    const { user } = useAppSelector(state => state.auth);
    const location = useLocation();
    const [expandedSections, setExpandedSections] = useState<Record<string, boolean>>({});
    const navigate = useNavigate();

    // Determine user role from auth state
    const roles = Array.isArray(user.role) ? user.role : [user.role];
    const priorities = ['admin', 'manager', 'employee'];

    const userRole =
        priorities.find(priority =>
            roles.some(role => role.trim().toLowerCase() === priority)
        ) || 'other';

    if (userRole === 'other') {
        navigate('/forbidden');
    }


    // Extract active item from current path
    const getActiveItemFromPath = () => {
        const pathSegments = location.pathname.split('/').filter(Boolean);
        return pathSegments[pathSegments.length - 1] || 'dashboard';
    };

    const activeItem = getActiveItemFromPath();

    const toggleSection = (sectionId: string) => {
        setExpandedSections(prev => ({
            ...prev,
            [sectionId]: !prev[sectionId]
        }));
    };

    return (
        <>
            <MainMg userRole={userRole} activeItem={activeItem} expandedSections={expandedSections} toggleSection={toggleSection} />
        </>
    );
};

export default DashboardLayout;