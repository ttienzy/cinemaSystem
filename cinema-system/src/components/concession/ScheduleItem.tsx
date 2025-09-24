import React, { useState, useEffect, useMemo } from 'react';
import { Calendar, Clock, User, MapPin, Briefcase, Phone, Mail } from 'lucide-react';
import { useAppSelector, useAppDispatch } from '../../hooks/redux';
import { getStaffSchedule } from '../../store/slices/inventorySlice';




const EmployeeSchedule: React.FC = () => {

    const [selectedDate, setSelectedDate] = useState<string>(
        new Date().toISOString().split('T')[0]
    );

    const { loading, schedules } = useAppSelector((state) => state.inventory);
    const dispatch = useAppDispatch();

    // Lấy cinemaId từ localStorage một cách an toàn
    const cinemaId = useMemo(() => localStorage.getItem('cinemaId'), []);

    // -------------------------------------------------------------
    // Data Fetching
    // -------------------------------------------------------------
    useEffect(() => {
        if (cinemaId) {
            // Gọi API khi component mount hoặc khi cinemaId/ngày thay đổi
            // Bạn có thể thêm selectedDate vào dependency array nếu muốn load lại lịch khi đổi ngày
            dispatch(getStaffSchedule(cinemaId));
        }
    }, [dispatch, cinemaId]); // Thêm selectedDate vào đây nếu cần: [dispatch, cinemaId, selectedDate]

    // -------------------------------------------------------------
    // Logic & Tính toán (Sử dụng useMemo để tối ưu)
    // -------------------------------------------------------------

    // Tính toán các số liệu tổng quan, chỉ tính lại khi schedules thay đổi
    const summaryStats = useMemo(() => {
        if (!schedules || schedules.length === 0) {
            return { totalShifts: 0, employeeCount: 0, departmentCount: 0 };
        }
        const totalShifts = schedules.reduce((sum, staff) => sum + (staff.shifts?.length || 0), 0);
        const uniqueDepartments = new Set(schedules.map(s => s.department));

        return {
            totalShifts,
            employeeCount: schedules.length,
            departmentCount: uniqueDepartments.size,
        };
    }, [schedules]);

    // -------------------------------------------------------------
    // Utility Functions (Hàm hỗ trợ)
    // -------------------------------------------------------------

    // Format thời gian (VD: 9:00 -> 09:00)
    const formatTime = (time: string): string => {
        if (!time) return 'N/A';
        const parts = time.split(':');
        const hour = parts[0].padStart(2, '0');
        const minute = parts[1].padStart(2, '0');
        return `${hour}:${minute}`;
    };

    // Tính toán thời lượng ca làm việc
    const calculateDuration = (startTime: string, endTime: string): string => {
        if (!startTime || !endTime) return 'N/A';
        const start = new Date(`1970-01-01T${startTime}:00`);
        const end = new Date(`1970-01-01T${endTime}:00`);
        let diff = end.getTime() - start.getTime();

        // Xử lý trường hợp qua đêm
        if (diff < 0) {
            diff += 24 * 60 * 60 * 1000;
        }

        const hours = Math.floor(diff / (1000 * 60 * 60));
        const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));

        return `${hours}h${minutes > 0 ? ` ${minutes}p` : ''}`;
    };

    // -------------------------------------------------------------
    // Render UI
    // -------------------------------------------------------------

    if (!cinemaId) {
        return (
            <div className="max-w-6xl mx-auto p-6 bg-white rounded-lg shadow-lg text-center">
                <h2 className="text-xl font-semibold text-red-600">Lỗi cấu hình</h2>
                <p className="text-gray-600 mt-2">
                    Không tìm thấy thông tin rạp chiếu. Vui lòng liên hệ quản trị viên.
                </p>
            </div>
        );
    }

    return (
        <div className="max-w-7xl mx-auto p-4 md:p-6 bg-gray-50 font-sans">
            <div className="bg-white rounded-xl shadow-md overflow-hidden">
                {/* Header */}
                <div className="p-6 border-b border-gray-200">
                    <h1 className="text-2xl md:text-3xl font-bold text-gray-800 flex items-center gap-3">
                        <Calendar className="text-indigo-500" size={28} />
                        Lịch Làm Việc Nhân Viên
                    </h1>
                    <p className="text-gray-500 mt-1">
                        Tổng quan lịch làm việc theo ngày.
                    </p>
                </div>

                {/* Filters */}
                <div className="p-6">
                    <div className="flex flex-col w-full sm:w-auto">
                        <label htmlFor="schedule-date" className="text-sm font-semibold text-gray-700 mb-2">
                            Chọn ngày làm việc
                        </label>
                        <input
                            id="schedule-date"
                            title='Select date'
                            type="date"
                            value={selectedDate}
                            onChange={(e) => setSelectedDate(e.target.value)}
                            className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 transition-shadow"
                        />
                    </div>
                </div>

                {/* Loading State */}
                {loading && (
                    <div className="flex justify-center items-center py-16">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
                        <p className="ml-4 text-gray-600">Đang tải dữ liệu...</p>
                    </div>
                )}

                {/* Schedule Table */}
                {!loading && (
                    <div className="overflow-x-auto">
                        <table className="w-full text-sm text-left text-gray-600">
                            <thead className="bg-gray-100 text-xs text-gray-700 uppercase tracking-wider">
                                <tr>
                                    <th scope="col" className="px-6 py-4"><User className="inline-block mr-2" size={16} />Nhân viên</th>
                                    <th scope="col" className="px-6 py-4"><MapPin className="inline-block mr-2" size={16} />Phòng ban</th>
                                    <th scope="col" className="px-6 py-4"><Briefcase className="inline-block mr-2" size={16} />Vị trí</th>
                                    <th scope="col" className="px-6 py-4"><Clock className="inline-block mr-2" size={16} />Bắt đầu</th>
                                    <th scope="col" className="px-6 py-4"><Clock className="inline-block mr-2" size={16} />Kết thúc</th>
                                    <th scope="col" className="px-6 py-4">Thời lượng</th>
                                    <th scope="col" className="px-6 py-4">Liên hệ</th>
                                </tr>
                            </thead>
                            <tbody>
                                {schedules.length === 0 ? (
                                    <tr>
                                        <td colSpan={7} className="px-6 py-12 text-center text-gray-500">
                                            Không có lịch làm việc cho ngày đã chọn.
                                        </td>
                                    </tr>
                                ) : (
                                    schedules.flatMap((staff) =>
                                        staff.shifts && staff.shifts.length > 0 ? (
                                            staff.shifts.map((shift, shiftIndex) => (
                                                <tr key={`${staff.email}-${shift.startTime}`} className="bg-white border-b hover:bg-gray-50 transition-colors duration-200">
                                                    {shiftIndex === 0 && (
                                                        <td rowSpan={staff.shifts.length} className="px-6 py-4 font-semibold text-gray-900 border-r align-top">
                                                            {staff.fullName}
                                                        </td>
                                                    )}
                                                    {shiftIndex === 0 && (
                                                        <td rowSpan={staff.shifts.length} className="px-6 py-4 border-r align-top">
                                                            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-indigo-100 text-indigo-800">
                                                                {staff.department}
                                                            </span>
                                                        </td>
                                                    )}
                                                    {shiftIndex === 0 && (
                                                        <td rowSpan={staff.shifts.length} className="px-6 py-4 border-r align-top">{staff.position}</td>
                                                    )}
                                                    <td className="px-6 py-4 font-mono text-blue-600">{formatTime(shift.startTime)}</td>
                                                    <td className="px-6 py-4 font-mono text-red-600">{formatTime(shift.endTime)}</td>
                                                    <td className="px-6 py-4">
                                                        <span className="inline-flex items-center px-2 py-1 rounded bg-gray-200 text-gray-800 text-xs font-medium">
                                                            {calculateDuration(shift.startTime, shift.endTime)}
                                                        </span>
                                                    </td>
                                                    {shiftIndex === 0 && (
                                                        <td rowSpan={staff.shifts.length} className="px-6 py-4 align-top">
                                                            <div className="flex items-center gap-2 text-gray-500 hover:text-gray-800">
                                                                <Phone size={14} /><a href={`tel:${staff.phone}`}>{staff.phone}</a>
                                                            </div>
                                                            <div className="flex items-center gap-2 mt-1 text-gray-500 hover:text-gray-800">
                                                                <Mail size={14} /><a href={`mailto:${staff.email}`}>{staff.email}</a>
                                                            </div>
                                                        </td>
                                                    )}
                                                </tr>
                                            ))
                                        ) : null // Bỏ qua nhân viên không có ca làm
                                    )
                                )}
                            </tbody>
                        </table>
                    </div>
                )}

                {/* Summary Section */}
                {!loading && summaryStats.totalShifts > 0 && (
                    <div className="p-6 bg-gray-50 border-t">
                        <h3 className="text-lg font-semibold text-gray-800 mb-4">Tổng quan ngày {selectedDate}</h3>
                        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                            <div className="bg-white p-4 rounded-lg border flex items-center gap-4">
                                <div className="p-3 bg-blue-100 rounded-full text-blue-600"><Briefcase size={24} /></div>
                                <div>
                                    <div className="text-3xl font-bold text-blue-600">{summaryStats.totalShifts}</div>
                                    <div className="font-medium text-gray-500">Tổng số ca làm</div>
                                </div>
                            </div>
                            <div className="bg-white p-4 rounded-lg border flex items-center gap-4">
                                <div className="p-3 bg-green-100 rounded-full text-green-600"><User size={24} /></div>
                                <div>
                                    <div className="text-3xl font-bold text-green-600">{summaryStats.employeeCount}</div>
                                    <div className="font-medium text-gray-500">Nhân viên đi làm</div>
                                </div>
                            </div>
                            <div className="bg-white p-4 rounded-lg border flex items-center gap-4">
                                <div className="p-3 bg-purple-100 rounded-full text-purple-600"><MapPin size={24} /></div>
                                <div>
                                    <div className="text-3xl font-bold text-purple-600">{summaryStats.departmentCount}</div>
                                    <div className="font-medium text-gray-500">Phòng ban tham gia</div>
                                </div>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default EmployeeSchedule;