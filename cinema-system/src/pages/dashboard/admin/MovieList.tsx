// src/pages/dashboard/admin/MovieList.tsx
import React from 'react';
import { FileText } from 'lucide-react';

const AdminMovieList: React.FC = () => {
    return (
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-8">
            <div className="text-center">
                <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
                    <FileText className="w-8 h-8 text-blue-600" />
                </div>
                <h3 className="text-lg font-medium text-gray-900 mb-2">
                    Nội dung chức năng
                </h3>
                <p className="text-gray-600 max-w-2xl mx-auto leading-relaxed">
                    Quản lý thông tin các bộ phim trong hệ thống
                </p>
                <div className="mt-6 p-4 bg-blue-50 rounded-lg">
                    <p className="text-sm text-blue-800">
                        <strong>Ghi chú:</strong> Đây là mô tả chức năng cho từng menu item.
                        Trong thực tế, đây sẽ là các component và trang thực tế của ứng dụng.
                    </p>
                </div>
            </div>
        </div>
    );
};

export default AdminMovieList;