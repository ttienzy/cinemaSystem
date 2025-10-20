// src/components/qrcode/ImageScanner.tsx
import React from 'react';
import jsQR from 'jsqr';
import { Upload } from 'lucide-react';

interface ImageScannerProps {
    onScanSuccess: (data: string) => void;
}

const ImageScanner: React.FC<ImageScannerProps> = ({ onScanSuccess }) => {

    const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (!file) {
            return;
        }

        const reader = new FileReader();
        reader.onload = (e) => {
            const image = new Image();
            image.src = e.target?.result as string;
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
                        alert('Không tìm thấy mã QR trong hình ảnh đã chọn.');
                    }
                }
            };
            image.onerror = () => {
                alert('Không thể đọc file hình ảnh. Vui lòng thử file khác.');
            };
        };
        reader.readAsDataURL(file);

        // Reset input để cho phép chọn lại cùng 1 file
        event.target.value = '';
    };

    return (
        <div className="flex flex-col items-center text-center p-8 border-dashed border-2 border-gray-300 rounded-lg w-full">
            <Upload className="mx-auto h-12 w-12 text-gray-400" />
            <p className="mt-2 mb-4 text-sm text-gray-600">Chọn một file ảnh có chứa mã QR để quét.</p>
            <input
                title='select file'
                type="file"
                accept="image/*"
                onChange={handleFileChange}
                className="block w-full max-w-xs text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-green-50 file:text-green-700 hover:file:bg-green-100 cursor-pointer"
            />
        </div>
    );
};

export default ImageScanner;    