import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { loginUser, clearError } from '../../store/slices/authSlice';
import { decodeTokenAndGetUser } from '../../utils/decodeTokenAndGetUser';

export const LoginPage: React.FC = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');

    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const { loading, error } = useAppSelector((state) => state.auth);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        dispatch(clearError());

        const result = await dispatch(loginUser({ email, password }));
        if (result.meta.requestStatus === 'fulfilled') {
            const userFromToken = decodeTokenAndGetUser(result.payload.token.accessToken);
            console.log('Decoded User from Token:', userFromToken);

            setTimeout(() => {
                if (userFromToken && userFromToken.role.includes('Admin')) {
                    navigate('/dashboard/admin');
                } else if (userFromToken && userFromToken.role.includes('Manager')) {
                    navigate('/dashboard/manager');
                } else if (userFromToken && userFromToken.role.includes('Employee')) {
                    navigate('/dashboard/employee');
                } else {
                    console.warn('User role is undefined or unrecognized:', userFromToken?.role);
                    navigate('/');
                }
            }, 500)
        }
    };

    return (
        <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
            <div className="w-full max-w-md">
                {/* Logo and Title */}
                <div className="text-center mb-8">
                    <h1 className="text-2xl font-semibold text-gray-900 mb-2">Cinema Management</h1>
                    <p className="text-sm text-gray-600">Hệ thống quản lý rạp chiếu phim</p>
                </div>

                {/* Login Card */}
                <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-8">
                    <h2 className="text-xl font-medium text-gray-900 mb-6">Đăng nhập</h2>

                    {error && (
                        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-6 text-sm" role="alert">
                            <span className="block sm:inline">{error}</span>
                        </div>
                    )}

                    <form onSubmit={handleSubmit}>
                        <div className="mb-5">
                            <label className="block text-sm font-medium text-gray-700 mb-2" htmlFor="email">
                                Email
                            </label>
                            <input
                                id="email"
                                title='email'
                                type="email"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                required
                                placeholder="email@example.com"
                                className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm"
                            />
                        </div>

                        <div className="mb-5">
                            <label className="block text-sm font-medium text-gray-700 mb-2" htmlFor="password">
                                Mật khẩu
                            </label>
                            <input
                                id="password"
                                title='password'
                                type="password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                required
                                placeholder="Nhập mật khẩu"
                                className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm"
                            />
                        </div>

                        <div className="flex items-center justify-between mb-6">
                            <label className="flex items-center text-sm text-gray-600 cursor-pointer">
                                <input
                                    type="checkbox"
                                    className="mr-2 w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                                />
                                Ghi nhớ đăng nhập
                            </label>
                            <Link to="/forgot-password" className="text-sm text-blue-600 hover:text-blue-700">
                                Quên mật khẩu?
                            </Link>
                        </div>

                        <button
                            type="submit"
                            disabled={loading}
                            className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2.5 px-4 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition text-sm"
                        >
                            {loading ? 'Đang đăng nhập...' : 'Đăng nhập'}
                        </button>
                    </form>

                    <p className="text-center text-sm text-gray-600 mt-6">
                        Chưa có tài khoản?{' '}
                        <Link to="/register" className="text-blue-600 hover:text-blue-700 font-medium">
                            Đăng ký ngay
                        </Link>
                    </p>
                </div>

                {/* Footer */}
                <p className="text-center text-xs text-gray-500 mt-8">
                    © 2025 Cinema Management System
                </p>
            </div>
        </div>
    );
}