import React from 'react';

const LoadingSpinner: React.FC = () => (
    // Nền trắng chủ đạo, chữ màu xám đậm
    <div className="flex flex-col justify-center items-center min-h-screen bg-white text-gray-800">

        {/* Spinner chính: Tinh tế và hiện đại hơn */}
        <div className="relative w-16 h-16 mb-6">
            {/* Vòng quay chính màu đỏ */}
            <div className="w-full h-full border-4 border-gray-200 border-t-red-600 border-solid rounded-full animate-spin"></div>
            {/* Logo hoặc icon ở giữa (tùy chọn) */}
            <div className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 text-red-600">
                {/* Bạn có thể đặt icon vé xem phim hoặc logo nhỏ ở đây */}
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="opacity-75"><path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"></path><polyline points="14 2 14 8 20 8"></polyline><path d="m10 13-2 2 2 2"></path><path d="m14 17 2-2-2-2"></path></svg>
            </div>
        </div>

        {/* Chữ "Đang tải" với màu đỏ chủ đạo */}
        <h2 className="text-xl font-semibold text-red-600 mb-3 tracking-wide">
            Đang tải dữ liệu
        </h2>

        {/* Thanh tiến trình với hiệu ứng chạy mượt mà */}
        <div className="w-64 h-2 bg-gray-200 rounded-full overflow-hidden">
            <div className="h-full bg-red-600 rounded-full animate-progress-indeterminate"></div>
        </div>

        {/* Dải phim trang trí với màu sắc mới */}
        <div className="mt-8 flex space-x-2 opacity-50">
            <div className="w-3 h-8 bg-gray-300 rounded-sm"></div>
            <div className="w-3 h-8 bg-red-500 rounded-sm"></div>
            <div className="w-3 h-8 bg-gray-300 rounded-sm"></div>
            <div className="w-3 h-8 bg-red-500 rounded-sm"></div>
            <div className="w-3 h-8 bg-gray-300 rounded-sm"></div>
        </div>
    </div>
);

export default LoadingSpinner;