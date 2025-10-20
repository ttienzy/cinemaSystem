import React, { useState } from 'react';
import CameraScanner from './CameraScanner';
import ImageScanner from './ImageScanner';
import UrlScanner from './UrlScanner';

// Redux
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { checkInBooking, confirmBookingCheckIn } from '../../store/slices/bookingSlice';

// Components hiển thị kết quả dùng chung
import { BookingDetails } from '../booking/BookingDetails';
import { LoadingSpinner, InitialState } from '../common/StateDisplays';
import { XCircle } from 'lucide-react';

type Tab = 'camera' | 'upload' | 'url';

const QRCodeScanner: React.FC = () => {
    const [activeTab, setActiveTab] = useState<Tab>('camera');
    // State để lưu booking code vừa được quét thành công
    const [scannedCode, setScannedCode] = useState<string | null>(null);

    const dispatch = useAppDispatch();
    const { checkedIn, loading, error } = useAppSelector(state => state.booking);
    // Hàm được gọi bởi các component con khi quét thành công
    const handleScanSuccess = (code: string) => {
        if (code) {
            setScannedCode(code); // Lưu lại code để dùng cho việc confirm
            dispatch(checkInBooking(code));
        }
    };
    // Hàm để confirm check-in
    const handleConfirmCheckIn = async () => {
        if (!scannedCode) return;

        try {
            await dispatch(confirmBookingCheckIn(scannedCode)).unwrap();
            // Tải lại dữ liệu mới nhất sau khi confirm
            dispatch(checkInBooking(scannedCode));
        } catch (error) {
            console.error('Failed to confirm check-in:', error);
            alert('An error occurred during check-in. Please try again.');
        }
    };
    // Hàm chuyển tab, đồng thời reset state
    const switchTab = (tab: Tab) => {
        setActiveTab(tab);
        setScannedCode(null);
        // dispatch(clearBookingState()); // Gọi action để xóa dữ liệu booking cũ
    }

    // Render các tab
    const renderScanner = () => {
        switch (activeTab) {
            case 'camera':
                return <CameraScanner onScanSuccess={handleScanSuccess} />;
            case 'upload':
                return <ImageScanner onScanSuccess={handleScanSuccess} />;
            case 'url':
                return <UrlScanner onScanSuccess={handleScanSuccess} />;
            default:
                return null;
        }
    };
    const getTabClass = (tab: Tab) => `cursor-pointer py-2 px-4 text-center ${activeTab === tab ? 'border-b-2 border-green-500 text-green-500' : 'text-gray-500'}`;


    return (
        <div className="w-full">
            <h1 className="text-2xl font-bold text-center mb-6">Quét mã QR Code</h1>
            <div className="flex justify-around border-b">
                <div className={getTabClass('camera')} onClick={() => switchTab('camera')}>Máy ảnh</div>
                <div className={getTabClass('upload')} onClick={() => switchTab('upload')}>Tải ảnh lên</div>
                <div className={getTabClass('url')} onClick={() => switchTab('url')}>Nhập URL</div>
            </div>

            {/* Phần hiển thị scanner */}
            <div className="mt-6 p-4 border border-dashed rounded-lg">
                {renderScanner()}
            </div>

            {/* Phần hiển thị kết quả */}
            <div className="mt-8">
                {loading && <LoadingSpinner />}

                {error && (
                    <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-center gap-3 text-sm">
                        <XCircle className="h-5 w-5 text-red-500 flex-shrink-0" />
                        <span className="text-red-700">{error}</span>
                    </div>
                )}

                {!loading && !error && checkedIn && (
                    <BookingDetails data={checkedIn} onCheckIn={handleConfirmCheckIn} />
                )}

                {!loading && !checkedIn && !error && (
                    <InitialState
                        title="Sẵn sàng để quét"
                        message="Sử dụng camera hoặc tải ảnh lên để tìm thông tin vé."
                    />
                )}
            </div>
        </div>
    );
};

export default QRCodeScanner;