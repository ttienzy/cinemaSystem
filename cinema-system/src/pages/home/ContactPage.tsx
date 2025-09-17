import React, { useState } from 'react';
import { MapPin, Phone, Mail, Send, Facebook, Instagram, Twitter, Youtube } from 'lucide-react';

const ContactPage: React.FC = () => {
    const [name, setName] = useState('');
    const [email, setEmail] = useState('');
    const [message, setMessage] = useState('');
    const [submitted, setSubmitted] = useState(false);

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        // Here you would typically send the form data to a backend
        console.log('Form submitted:', { name, email, message });
        setSubmitted(true);
        setName('');
        setEmail('');
        setMessage('');
    };

    return (
        <div className="min-h-screen bg-gray-50 py-8 px-4">
            <div className="max-w-7xl mx-auto">
                {/* Header */}
                <div className="mb-8">
                    <h1 className="text-3xl font-bold text-gray-900 mb-2">Liên Hệ Với Chúng Tôi</h1>
                    <p className="text-gray-600">Chúng tôi luôn sẵn sàng hỗ trợ bạn. Hãy liên hệ qua các kênh dưới đây hoặc gửi tin nhắn trực tiếp.</p>
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                    {/* Contact Information */}
                    <div className="bg-white rounded-lg shadow-sm p-6">
                        <h2 className="text-2xl font-bold text-gray-900 mb-6">Thông Tin Liên Hệ</h2>

                        <div className="space-y-6">
                            {/* Address */}
                            <div className="flex items-start">
                                <MapPin className="w-6 h-6 text-blue-500 mr-4 flex-shrink-0" />
                                <div>
                                    <h3 className="font-semibold text-gray-900">Địa Chỉ</h3>
                                    <p className="text-gray-600">123 Nguyễn Huệ, Quận 1, TP.HCM</p>
                                </div>
                            </div>

                            {/* Phone */}
                            <div className="flex items-start">
                                <Phone className="w-6 h-6 text-blue-500 mr-4 flex-shrink-0" />
                                <div>
                                    <h3 className="font-semibold text-gray-900">Hotline</h3>
                                    <p className="text-gray-600">1900 6017</p>
                                    <p className="text-sm text-gray-500">Hỗ trợ 24/7</p>
                                </div>
                            </div>

                            {/* Email */}
                            <div className="flex items-start">
                                <Mail className="w-6 h-6 text-blue-500 mr-4 flex-shrink-0" />
                                <div>
                                    <h3 className="font-semibold text-gray-900">Email</h3>
                                    <p className="text-gray-600">info@cinemahub.vn</p>
                                </div>
                            </div>
                        </div>

                        {/* Social Media */}
                        <div className="mt-8">
                            <h3 className="font-semibold text-gray-900 mb-4">Kết Nối Với Chúng Tôi</h3>
                            <div className="flex space-x-4">
                                <a href="#" className="text-gray-600 hover:text-blue-500">
                                    <Facebook className="w-6 h-6" />
                                </a>
                                <a href="#" className="text-gray-600 hover:text-blue-500">
                                    <Instagram className="w-6 h-6" />
                                </a>
                                <a href="#" className="text-gray-600 hover:text-blue-500">
                                    <Twitter className="w-6 h-6" />
                                </a>
                                <a href="#" className="text-gray-600 hover:text-blue-500">
                                    <Youtube className="w-6 h-6" />
                                </a>
                            </div>
                        </div>
                    </div>

                    {/* Contact Form */}
                    <div className="bg-white rounded-lg shadow-sm p-6">
                        <h2 className="text-2xl font-bold text-gray-900 mb-6">Gửi Tin Nhắn</h2>

                        {submitted ? (
                            <div className="bg-green-100 text-green-700 p-4 rounded-lg mb-6">
                                Cảm ơn bạn! Tin nhắn của bạn đã được gửi thành công.
                            </div>
                        ) : null}

                        <form onSubmit={handleSubmit} className="space-y-4">
                            <div>
                                <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
                                    Họ Tên
                                </label>
                                <input
                                    type="text"
                                    id="name"
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                                    value={name}
                                    onChange={(e) => setName(e.target.value)}
                                    required
                                />
                            </div>

                            <div>
                                <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
                                    Email
                                </label>
                                <input
                                    type="email"
                                    id="email"
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    required
                                />
                            </div>

                            <div>
                                <label htmlFor="message" className="block text-sm font-medium text-gray-700 mb-1">
                                    Tin Nhắn
                                </label>
                                <textarea
                                    id="message"
                                    rows={6}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                                    value={message}
                                    onChange={(e) => setMessage(e.target.value)}
                                    required
                                />
                            </div>

                            <button
                                type="submit"
                                className="w-full bg-blue-500 text-white py-3 rounded-lg hover:bg-blue-600 transition-colors flex items-center justify-center"
                            >
                                <Send className="w-4 h-4 mr-2" />
                                Gửi Tin Nhắn
                            </button>
                        </form>
                    </div>
                </div>

                {/* Map Placeholder */}
                <div className="mt-8 bg-white rounded-lg shadow-sm p-6">
                    <h2 className="text-2xl font-bold text-gray-900 mb-4">Vị Trí</h2>
                    <div className="w-full h-64 bg-gray-200 rounded-lg flex items-center justify-center">
                        <p className="text-gray-500">Bản đồ sẽ được hiển thị ở đây (Sử dụng Google Maps hoặc tương tự)</p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ContactPage;