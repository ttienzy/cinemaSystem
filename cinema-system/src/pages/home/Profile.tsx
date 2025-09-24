import React, { useState, useEffect } from 'react';
import { User, Mail, Phone, Shield, Calendar, Film, CreditCard, Ticket, LogOut } from 'lucide-react';
import { jwtDecode } from 'jwt-decode';
import type { DecodedToken } from '../../types/auth.types';
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { getCurrentUser, purchaseHistory } from '../../store//slices/profileSice';
import { logout } from '../../store/slices/authSlice';
import { useNavigate } from 'react-router-dom';



const Profile: React.FC = () => {

    const [currentTab, setCurrentTab] = useState<'info' | 'history'>('info');

    const token = localStorage.getItem('accessToken');
    const decoded = token ? jwtDecode<DecodedToken>(token) : null;
    const navigate = useNavigate();

    const dispatch = useAppDispatch();
    const { userInfo, purchases, loading, error } = useAppSelector(state => state.profile);
    // Load user info on mount
    useEffect(() => {
        // Simulate API call for user info
        setTimeout(() => {
            if (!decoded) {
                return;
            }
            dispatch(getCurrentUser(decoded.nameid));
        }, 1000);
    }, []);

    // Load purchase history when history tab is selected
    useEffect(() => {
        if (currentTab === 'history' && purchases.length === 0) {
            // Simulate API call for purchase history
            setTimeout(() => {
                if (!decoded) {
                    return;
                }
                dispatch(purchaseHistory(decoded.nameid));
            }, 1000);
        }
    }, [currentTab, purchases.length]);

    const handleLogout = (): void => {
        dispatch(logout());
        navigate('/login');
    };

    const formatCurrency = (amount: number): string => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    };

    const formatDateTime = (dateString?: string): string => {
        if (!dateString) return '';
        return new Date(dateString).toLocaleString('vi-VN');
    };

    return (
        <div className="min-h-screen bg-gray-50 py-8">
            <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
                {/* Header */}
                <div className="mb-8">
                    <div className="flex items-center justify-between">
                        <h1 className="text-3xl font-bold text-gray-900">Hồ sơ cá nhân</h1>
                        <button
                            onClick={handleLogout}
                            className="flex items-center space-x-2 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
                        >
                            <LogOut className="h-4 w-4" />
                            <span>Đăng xuất</span>
                        </button>
                    </div>
                </div>

                {/* Error */}
                {error && (
                    <div style={{ color: 'red', marginBottom: '1rem' }}>
                        {error}
                    </div>
                )}

                {/* Tabs */}
                <div className="mb-6">
                    <nav className="flex space-x-4 border-b border-gray-200">
                        <button
                            onClick={() => setCurrentTab('info')}
                            className={`px-4 py-2 text-sm font-medium ${currentTab === 'info'
                                ? 'border-b-2 border-blue-600 text-blue-600'
                                : 'text-gray-500 hover:text-gray-700'
                                }`}
                        >
                            Thông tin cá nhân
                        </button>
                        <button
                            onClick={() => setCurrentTab('history')}
                            className={`px-4 py-2 text-sm font-medium ${currentTab === 'history'
                                ? 'border-b-2 border-blue-600 text-blue-600'
                                : 'text-gray-500 hover:text-gray-700'
                                }`}
                        >
                            Lịch sử mua vé
                        </button>
                    </nav>
                </div>

                {/* Content */}
                {currentTab === 'info' && (
                    <>
                        {loading ? (
                            <div className="flex items-center justify-center py-12">
                                <div className="text-center">
                                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                                    <p className="mt-4 text-gray-600">Đang tải thông tin...</p>
                                </div>
                            </div>
                        ) : (
                            <div className="bg-white rounded-xl shadow-sm border border-gray-200">
                                <div className="px-6 py-4 border-b border-gray-200">
                                    <h2 className="text-xl font-semibold text-gray-900">Thông tin cá nhân</h2>
                                </div>
                                <div className="p-6">
                                    <div className="flex items-center mb-6">
                                        <div className="w-16 h-16 bg-blue-600 text-white rounded-full flex items-center justify-center text-2xl font-bold mr-4">
                                            {userInfo?.userName.charAt(0).toUpperCase()}
                                        </div>
                                        <div>
                                            <h3 className="text-xl font-semibold text-gray-900">{userInfo?.userName}</h3>
                                            <p className="text-gray-600">Ngày tạo: {formatDateTime(userInfo?.createdAt)}</p>
                                        </div>
                                    </div>

                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                        <div className="flex items-center space-x-3">
                                            <User className="h-5 w-5 text-gray-400" />
                                            <div>
                                                <p className="text-sm font-medium text-gray-900">Tên người dùng</p>
                                                <p className="text-gray-600">{userInfo?.userName}</p>
                                            </div>
                                        </div>

                                        <div className="flex items-center space-x-3">
                                            <Mail className="h-5 w-5 text-gray-400" />
                                            <div>
                                                <p className="text-sm font-medium text-gray-900">Email</p>
                                                <p className="text-gray-600">{userInfo?.email}</p>
                                            </div>
                                        </div>

                                        <div className="flex items-center space-x-3">
                                            <Phone className="h-5 w-5 text-gray-400" />
                                            <div>
                                                <p className="text-sm font-medium text-gray-900">Số điện thoại</p>
                                                <p className="text-gray-600">{userInfo?.phoneNumber}</p>
                                            </div>
                                        </div>

                                        <div className="flex items-center space-x-3">
                                            <Shield className="h-5 w-5 text-gray-400" />
                                            <div>
                                                <p className="text-sm font-medium text-gray-900">Vai trò</p>
                                                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                                                    {userInfo?.roles.join(', ')}
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        )}
                    </>
                )}

                {currentTab === 'history' && (
                    <>
                        {loading ? (
                            <div className="flex items-center justify-center py-12">
                                <div className="text-center">
                                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                                    <p className="mt-4 text-gray-600">Đang tải lịch sử...</p>
                                </div>
                            </div>
                        ) : (
                            <div className="bg-white rounded-xl shadow-sm border border-gray-200">
                                <div className="px-6 py-4 border-b border-gray-200">
                                    <h2 className="text-xl font-semibold text-gray-900">Lịch sử mua vé</h2>
                                </div>
                                <div className="p-6">
                                    {purchaseHistory.length === 0 ? (
                                        <div className="text-center py-8">
                                            <Film className="h-12 w-12 text-gray-400 mx-auto mb-4" />
                                            <p className="text-gray-500">Chưa có lịch sử mua vé nào</p>
                                        </div>
                                    ) : (
                                        <div className="space-y-4">
                                            {purchases.map((purchase) => (
                                                <div key={purchase.bookingId} className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow">
                                                    <div className="flex items-start justify-between">
                                                        <div className="flex-1">
                                                            <div className="flex items-center space-x-3 mb-2">
                                                                <Film className="h-5 w-5 text-blue-600" />
                                                                <h3 className="font-semibold text-gray-900">{purchase.movieTitle}</h3>
                                                            </div>

                                                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm text-gray-600">
                                                                <div className="flex items-center space-x-2">
                                                                    <Calendar className="h-4 w-4" />
                                                                    <span>Mua vé: {formatDateTime(purchase.bookingTime)}</span>
                                                                </div>

                                                                <div className="flex items-center space-x-2">
                                                                    <Calendar className="h-4 w-4" />
                                                                    <span>Suất chiếu: {formatDateTime(purchase.showTime)}</span>
                                                                </div>

                                                                <div className="flex items-center space-x-2">
                                                                    <Ticket className="h-4 w-4" />
                                                                    <span>Số vé: {purchase.totalTickets}</span>
                                                                </div>

                                                                <div className="flex items-center space-x-2">
                                                                    <CreditCard className="h-4 w-4" />
                                                                    <span>Tổng tiền: {formatCurrency(purchase.totalAmount)}</span>
                                                                </div>
                                                            </div>

                                                            <p className="text-sm text-gray-500 mt-2">Rạp: {purchase.cinemaName}</p>
                                                        </div>

                                                        <div className="ml-4">
                                                            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                                                                {purchase.status}
                                                            </span>
                                                        </div>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                </div>
                            </div>
                        )}
                    </>
                )}
            </div>
        </div>
    );
};

export default Profile;