import React, { use, useState } from 'react';
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
        <div style={{ padding: '2rem', maxWidth: '400px', margin: '0 auto' }}>
        <h1>Đăng ký</h1>
        
        {error && (
            <div style={{ color: 'red', marginBottom: '1rem' }}>
            {error}
            </div>
        )}
        
        <form onSubmit={handleSubmit}>
            <div style={{ marginBottom: '1rem' }}>
                <label>User Name:</label>
                <input
                    title='userName'
                    type="text"
                    name="userName"
                    value={formData.userName}
                    onChange={handleChange}
                    required
                    style={{ width: '100%', padding: '0.5rem' }}
                />
            </div>
            
            <div style={{ marginBottom: '1rem' }}>
                <label>Email:</label>
                <input
                    title='email'
                    type="email"
                    name="email"
                    value={formData.email}
                    onChange={handleChange}
                    required
                    style={{ width: '100%', padding: '0.5rem' }}
                />
            </div>
            
            <div style={{ marginBottom: '1rem' }}>
                <label>Password:</label>
                <input
                    title='password'
                    type="password"
                    name="password"
                    value={formData.password}
                    onChange={handleChange}
                    required
                    style={{ width: '100%', padding: '0.5rem' }}
                />
            </div>
            
            <div style={{ marginBottom: '1rem' }}>
                <label>Phone Number:</label>
                <input
                    title='phoneNumber'
                    type="text"
                    name="phoneNumber"
                    value={formData.phoneNumber}
                    onChange={handleChange}
                    required
                    style={{ width: '100%', padding: '0.5rem' }}
                />
            </div>
            
            <button type="submit" disabled={loading}>
            {loading ? 'Đang đăng ký...' : 'Đăng ký'}
            </button>
        </form>
        
        <p>
            Đã có tài khoản? <Link to="/login">Đăng nhập</Link>
        </p>
        </div>
    );
};