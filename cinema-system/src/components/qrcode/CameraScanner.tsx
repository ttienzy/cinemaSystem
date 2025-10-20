// src/components/qrcode/CameraScanner.tsx

import React, { useRef, useState, useCallback, useEffect } from 'react';
import Webcam from 'react-webcam';
import jsQR from 'jsqr';
import { Camera } from 'lucide-react';

interface CameraScannerProps {
    onScanSuccess: (data: string) => void;
}

const CameraScanner: React.FC<CameraScannerProps> = ({ onScanSuccess }) => {
    const webcamRef = useRef<Webcam>(null);
    const requestRef = useRef<number | null>(null);
    const [isCameraOpen, setIsCameraOpen] = useState<boolean>(false);
    const [cameraError, setCameraError] = useState<string>('');

    // Hàm quét mã QR liên tục bằng requestAnimationFrame để tối ưu hiệu năng
    const scanQRCode = useCallback(() => {
        if (webcamRef.current) {
            const video = webcamRef.current.video;
            if (video && video.readyState === video.HAVE_ENOUGH_DATA) {
                const canvas = document.createElement('canvas');
                canvas.width = video.videoWidth;
                canvas.height = video.videoHeight;
                const ctx = canvas.getContext('2d');

                if (ctx) {
                    ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
                    const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                    const code = jsQR(imageData.data, imageData.width, imageData.height);

                    if (code) {
                        onScanSuccess(code.data); // Gửi kết quả về cho cha
                        setIsCameraOpen(false);   // Tự động đóng camera
                        return; // Dừng vòng lặp
                    }
                }
            }
        }
        // Tiếp tục vòng lặp nếu chưa tìm thấy mã
        requestRef.current = requestAnimationFrame(scanQRCode);
    }, [onScanSuccess]);

    // Effect để quản lý vòng lặp quét
    useEffect(() => {
        if (isCameraOpen) {
            requestRef.current = requestAnimationFrame(scanQRCode);
        }
        // Hàm dọn dẹp: Hủy vòng lặp khi component unmount hoặc camera đóng
        return () => {
            if (requestRef.current) {
                cancelAnimationFrame(requestRef.current);
            }
        };
    }, [isCameraOpen, scanQRCode]);

    const handleOpenCamera = () => {
        setCameraError('');
        setIsCameraOpen(true);
    };

    const handleUserMediaError = () => {
        setCameraError('Không thể truy cập camera. Vui lòng kiểm tra và cấp quyền trong trình duyệt của bạn.');
        setIsCameraOpen(false);
    };

    return (
        <div className="flex flex-col items-center">
            {isCameraOpen ? (
                <div className="w-full max-w-md">
                    <Webcam
                        audio={false}
                        ref={webcamRef}
                        screenshotFormat="image/jpeg"
                        className="rounded-lg w-full"
                        onUserMediaError={handleUserMediaError}
                        videoConstraints={{ facingMode: "environment" }}
                    />
                </div>
            ) : (
                <div className="text-center p-8 border-dashed border-2 border-gray-300 rounded-lg w-full">
                    <Camera className="mx-auto h-12 w-12 text-gray-400" />
                    <p className="mt-2 text-sm text-gray-600">Bật camera và hướng vào mã QR.</p>
                    <button
                        onClick={handleOpenCamera}
                        className="mt-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-green-600 hover:bg-green-700 focus:outline-none"
                    >
                        Mở Camera
                    </button>
                </div>
            )}
            {cameraError && <p className="text-red-500 mt-4 text-center">{cameraError}</p>}
        </div>
    );
};

export default CameraScanner;