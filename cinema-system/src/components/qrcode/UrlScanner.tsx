import React, { useState } from 'react';
import jsQR from 'jsqr';
import { Link } from 'lucide-react';

interface UrlScannerProps {
    onScanSuccess: (data: string) => void;
}

const UrlScanner: React.FC<UrlScannerProps> = ({ onScanSuccess }) => {
    const [imageUrl, setImageUrl] = useState<string>('');
    const [isLoading, setIsLoading] = useState<boolean>(false);

    const handleScanUrl = () => {
        if (!imageUrl.trim()) {
            alert('Vui lòng nhập URL hình ảnh.');
            return;
        }

        setIsLoading(true);
        const image = new Image();
        // Quan trọng: Cần thuộc tính này để cố gắng vượt qua lỗi CORS khi vẽ ảnh lên canvas
        image.crossOrigin = 'Anonymous';
        image.src = imageUrl;

        image.onload = () => {
            const canvas = document.createElement('canvas');
            canvas.width = image.width;
            canvas.height = image.height;
            const ctx = canvas.getContext('2d');
            if (ctx) {
                ctx.drawImage(image, 0, 0, image.width, image.height);
                const imageData = ctx.getImageData(0, 0, image.width, image.height);
                const code = jsQR(imageData.data, imageData.width, imageData.height);

                if (code) {
                    onScanSuccess(code.data);
                } else {
                    alert('Không tìm thấy mã QR trong hình ảnh từ URL.');
                }
            }
            setIsLoading(false);
        };

        image.onerror = () => {
            alert('Không thể tải hình ảnh. Vui lòng kiểm tra lại URL hoặc hình ảnh có thể bị chặn bởi chính sách CORS.');
            setIsLoading(false);
        };
    };

    return (
        <div className="flex flex-col items-center p-4">
            <div className="w-full flex max-w-md">
                <div className="relative flex-grow">
                    <Link className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
                    <input
                        type="text"
                        value={imageUrl}
                        onChange={(e) => setImageUrl(e.target.value)}
                        placeholder="Dán URL hình ảnh mã QR vào đây"
                        className="w-full pl-10 p-2 border border-gray-300 rounded-l-md focus:ring-green-500 focus:border-green-500"
                        disabled={isLoading}
                    />
                </div>
                <button
                    onClick={handleScanUrl}
                    disabled={isLoading}
                    className="px-4 py-2 border border-transparent text-sm font-medium rounded-r-md shadow-sm text-white bg-green-600 hover:bg-green-700 disabled:bg-gray-400"
                >
                    {isLoading ? 'Đang xử lý...' : 'Quét'}
                </button>
            </div>
        </div>
    );
};

export default UrlScanner;