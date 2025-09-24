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
            // Decode user từ token trong payload thay vì dùng state
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
        <div className="min-h-screen bg-gray-100 flex flex-col justify-center items-center">
            <div className="bg-white p-8 rounded-lg shadow-md w-full max-w-md">
                <h1 className="text-2xl font-bold mb-6 text-center">Đăng nhập</h1>

                {error && (
                    <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative mb-4" role="alert">
                        <span className="block sm:inline">{error}</span>
                    </div>
                )}

                <form onSubmit={handleSubmit}>
                    <div className="mb-4">
                        <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="email">
                            Email:
                        </label>
                        <input
                            id="email"
                            title='email'
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                        />
                    </div>

                    <div className="mb-6">
                        <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="password">
                            Mật khẩu:
                        </label>
                        <input
                            id="password"
                            title='password'
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 mb-3 leading-tight focus:outline-none focus:shadow-outline"
                        />
                    </div>

                    <div className="flex items-center justify-between">
                        <button
                            type="submit"
                            disabled={loading}
                            className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline disabled:bg-blue-300"
                        >
                            {loading ? 'Đang đăng nhập...' : 'Đăng nhập'}
                        </button>
                    </div>
                </form>

                <p className="text-center text-gray-500 text-xs mt-4">
                    Chưa có tài khoản? <Link to="/register" className="text-blue-500 hover:text-blue-800">Đăng ký</Link>
                </p>
            </div>
        </div>
    );
}