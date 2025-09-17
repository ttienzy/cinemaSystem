import { Coffee, Ticket } from "lucide-react";

interface TabNavigationProps {
    activeTab: 'combo' | 'concession';
    onTabChange: (tab: 'combo' | 'concession') => void;
}

const TabNavigation: React.FC<TabNavigationProps> = ({ activeTab, onTabChange }) => {
    return (
        <div className="bg-white border-b border-gray-200">
            <div className="px-6">
                <div className="flex space-x-8">
                    <button
                        className={`py-4 px-2 border-b-2 font-medium text-sm ${activeTab === 'combo'
                            ? 'border-blue-500 text-blue-600'
                            : 'border-transparent text-gray-500 hover:text-gray-700'
                            }`}
                        onClick={() => onTabChange('combo')}
                    >
                        <div className="flex items-center">
                            <Ticket className="w-5 h-5 mr-2" />
                            Combo Vé + Đồ ăn
                        </div>
                    </button>
                    <button
                        className={`py-4 px-2 border-b-2 font-medium text-sm ${activeTab === 'concession'
                            ? 'border-blue-500 text-blue-600'
                            : 'border-transparent text-gray-500 hover:text-gray-700'
                            }`}
                        onClick={() => onTabChange('concession')}
                    >
                        <div className="flex items-center">
                            <Coffee className="w-5 h-5 mr-2" />
                            Chỉ bán đồ ăn uống
                        </div>
                    </button>
                </div>
            </div>
        </div>
    );
};

export default TabNavigation;