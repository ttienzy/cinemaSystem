import { Outlet } from "react-router-dom";
import HeaderMg from "./HeaderMg";
import Sidebar from "./Sidebar";
import { menuItems } from '../../types/dashboard.types';


interface MainMgProps {
    userRole: string;
    activeItem: string;
    expandedSections: Record<string, boolean>;
    toggleSection: (id: string) => void;
}
const MainMg: React.FC<MainMgProps> = ({
    userRole,
    activeItem,
    expandedSections,
    toggleSection,
}) => {
    return (
        <div className="flex h-screen bg-gray-100">
            <Sidebar
                userRole={userRole}
                activeItem={activeItem}
                expandedSections={expandedSections}
                toggleSection={toggleSection}
            />
            <div className="flex-1 flex flex-col">
                <HeaderMg activeRole={userRole} activeItem={activeItem} menuItems={menuItems} />
                <div className="flex-1 p-6 bg-gray-50 overflow-y-auto">
                    <Outlet />
                </div>
            </div>
        </div>
    );
}
export default MainMg;