import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { registerUser, clearError } from '../../store/slices/authSlice';

export const RegisterPage: React.FC = () => {
    const [formData, setFormData] = useState({
        userName: '',
        email: '',
        password: '',
        phoneNumber: '',
    });

    const dispatch = useAppDispatch();
    const navigate = useNavigate();

    const { loading, error } = useAppSelector((state) => state.auth);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value,
        });
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        dispatch(clearError());

        const result = await dispatch(registerUser(formData));
        if (result.meta.requestStatus === 'fulfilled') {
            navigate('/login', { replace: true });
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

                {/* Register Card */}
                <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-8">
                    <h2 className="text-xl font-medium text-gray-900 mb-6">Đăng ký tài khoản</h2>

                    {error && (
                        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-6 text-sm" role="alert">
                            <span className="block sm:inline">{error}</span>
                        </div>
                    )}

                    <form onSubmit={handleSubmit}>
                        <div className="mb-5">
                            <label className="block text-sm font-medium text-gray-700 mb-2" htmlFor="userName">
                                Tên người dùng
                            </label>
                            <input
                                id="userName"
                                title='userName'
                                type="text"
                                name="userName"
                                value={formData.userName}
                                onChange={handleChange}
                                required
                                placeholder="Nhập tên người dùng"
                                className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm"
                            />
                        </div>

                        <div className="mb-5">
                            <label className="block text-sm font-medium text-gray-700 mb-2" htmlFor="email">
                                Email
                            </label>
                            <input
                                id="email"
                                title='email'
                                type="email"
                                name="email"
                                value={formData.email}
                                onChange={handleChange}
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
                                name="password"
                                value={formData.password}
                                onChange={handleChange}
                                required
                                placeholder="Nhập mật khẩu"
                                className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm"
                            />
                        </div>

                        <div className="mb-6">
                            <label className="block text-sm font-medium text-gray-700 mb-2" htmlFor="phoneNumber">
                                Số điện thoại
                            </label>
                            <input
                                id="phoneNumber"
                                title='phoneNumber'
                                type="text"
                                name="phoneNumber"
                                value={formData.phoneNumber}
                                onChange={handleChange}
                                required
                                placeholder="0123456789"
                                className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm"
                            />
                        </div>

                        <button
                            type="submit"
                            disabled={loading}
                            className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2.5 px-4 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition text-sm"
                        >
                            {loading ? 'Đang đăng ký...' : 'Đăng ký'}
                        </button>
                    </form>

                    <p className="text-center text-sm text-gray-600 mt-6">
                        Đã có tài khoản?{' '}
                        <Link to="/login" className="text-blue-600 hover:text-blue-700 font-medium">
                            Đăng nhập ngay
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
};