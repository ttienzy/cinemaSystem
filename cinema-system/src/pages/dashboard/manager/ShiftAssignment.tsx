// src/pages/dashboard/manager/ShiftAssignment.tsx

import React, { useState, useEffect } from 'react';
import { Clock, Plus, Edit, Trash2 } from 'lucide-react';
import { useAppDispatch, useAppSelector } from '../../../hooks/redux';
import type { AddShiftToEmployeeRequest, Shift } from '../../../types/staff.types';
import { addShiftToCinemaForManager, getShiftsMn } from '../../../store/slices/staffSlice';

const ManagerShiftAssignment: React.FC = () => {
    // State quản lý trạng thái của modal (mở/đóng, thêm/sửa)
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [modalMode, setModalMode] = useState<'add' | 'edit'>('add');
    const [currentShift, setCurrentShift] = useState<Shift | null>(null);
    const { getShiftMn, loading, error } = useAppSelector((state) => state.staff);
    const dispatch = useAppDispatch();

    // State cho dữ liệu trong form
    const [shiftForm, setShiftForm] = useState({ name: '', startTime: '', endTime: '' });
    const [formErrors, setFormErrors] = useState({ name: '', startTime: '', endTime: '' });
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Tải dữ liệu khi component được mount
    useEffect(() => {
        const cinemaId = localStorage.getItem('cinemaId');
        if (cinemaId) {
            dispatch(getShiftsMn(cinemaId));
        }
    }, [dispatch]);

    // --- CÁC HÀM XỬ LÝ MODAL VÀ FORM ---

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setShiftForm(prev => ({ ...prev, [name]: value }));
        // Xóa lỗi khi người dùng bắt đầu nhập
        if (formErrors[name as keyof typeof formErrors]) {
            setFormErrors(prev => ({ ...prev, [name]: '' }));
        }
    };

    const validateForm = (): boolean => {
        const errors = { name: '', startTime: '', endTime: '' };
        let isValid = true;

        if (!shiftForm.name.trim()) {
            errors.name = "Tên ca không được để trống.";
            isValid = false;
        }
        if (!shiftForm.startTime) {
            errors.startTime = "Giờ bắt đầu không được để trống.";
            isValid = false;
        }
        if (!shiftForm.endTime) {
            errors.endTime = "Giờ kết thúc không được để trống.";
            isValid = false;
        } else if (shiftForm.startTime && shiftForm.endTime <= shiftForm.startTime) {
            errors.endTime = "Giờ kết thúc phải sau giờ bắt đầu.";
            isValid = false;
        }

        setFormErrors(errors);
        return isValid;
    };

    const handleOpenAddModal = () => {
        setModalMode('add');
        setCurrentShift(null);
        setShiftForm({ name: '', startTime: '', endTime: '' });
        setFormErrors({ name: '', startTime: '', endTime: '' });
        setIsModalOpen(true);
    };

    const handleOpenEditModal = (shift: Shift) => {
        setModalMode('edit');
        setCurrentShift(shift);
        setShiftForm({ name: shift.name, startTime: shift.startTime, endTime: shift.endTime });
        setFormErrors({ name: '', startTime: '', endTime: '' });
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
        setShiftForm({ name: '', startTime: '', endTime: '' });
        setFormErrors({ name: '', startTime: '', endTime: '' });
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!validateForm()) return;

        const cinemaId = localStorage.getItem('cinemaId');
        if (!cinemaId) {
            alert('Không tìm thấy thông tin rạp. Vui lòng đăng nhập lại.');
            return;
        }

        setIsSubmitting(true);

        try {
            if (modalMode === 'add') {
                // Tạo payload theo định dạng API yêu cầu
                const shiftData: AddShiftToEmployeeRequest[] = [{
                    cinemaId: cinemaId,
                    startTime: shiftForm.startTime,
                    endTime: shiftForm.endTime,
                    name: shiftForm.name
                }];

                console.log('Dữ liệu gửi đi:', shiftData);
                dispatch(addShiftToCinemaForManager(shiftData));
                // TODO: Gọi API thêm shift
                // const response = await fetch('YOUR_API_ENDPOINT/shifts', {
                //     method: 'POST',
                //     headers: {
                //         'Content-Type': 'application/json',
                //         // 'Authorization': `Bearer ${token}` // Nếu cần
                //     },
                //     body: JSON.stringify(shiftData)
                // });
                // 
                // if (!response.ok) {
                //     throw new Error('Thêm ca làm việc thất bại');
                // }
                // 
                // const result = await response.json();

                // Giả lập API call
                await new Promise(resolve => setTimeout(resolve, 1000));

                alert('Thêm ca làm việc thành công!');

                // Tải lại danh sách shifts
                dispatch(getShiftsMn(cinemaId));

            } else if (currentShift) {
                // TODO: Gọi API cập nhật shift
                const updateData = {
                    cinemaId: cinemaId,
                    startTime: shiftForm.startTime,
                    endTime: shiftForm.endTime,
                    name: shiftForm.name
                };

                console.log('Dữ liệu cập nhật:', updateData);

                // const response = await fetch(`YOUR_API_ENDPOINT/shifts/${currentShift.shiftId}`, {
                //     method: 'PUT',
                //     headers: {
                //         'Content-Type': 'application/json',
                //     },
                //     body: JSON.stringify(updateData)
                // });
                //
                // if (!response.ok) {
                //     throw new Error('Cập nhật ca làm việc thất bại');
                // }

                await new Promise(resolve => setTimeout(resolve, 1000));

                alert('Cập nhật ca làm việc thành công!');

                // Tải lại danh sách shifts
                dispatch(getShiftsMn(cinemaId));
            }

            handleCloseModal();
        } catch (error) {
            console.error('Lỗi khi xử lý ca làm việc:', error);
            alert('Có lỗi xảy ra. Vui lòng thử lại!');
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDelete = async (shiftId: string) => {
        if (!window.confirm("Bạn có chắc chắn muốn xóa ca làm việc này không?")) {
            return;
        }

        try {
            // TODO: Gọi API xóa shift
            // const response = await fetch(`YOUR_API_ENDPOINT/shifts/${shiftId}`, {
            //     method: 'DELETE',
            //     headers: {
            //         'Content-Type': 'application/json',
            //     }
            // });
            //
            // if (!response.ok) {
            //     throw new Error('Xóa ca làm việc thất bại');
            // }

            console.log('Xóa shift ID:', shiftId);
            await new Promise(resolve => setTimeout(resolve, 500));

            alert('Xóa ca làm việc thành công!');

            // Tải lại danh sách shifts
            const cinemaId = localStorage.getItem('cinemaId');
            if (cinemaId) {
                dispatch(getShiftsMn(cinemaId));
            }
        } catch (error) {
            console.error('Lỗi khi xóa ca làm việc:', error);
            alert('Có lỗi xảy ra khi xóa. Vui lòng thử lại!');
        }
    };

    // --- RENDER COMPONENT ---

    return (
        <>
            {/* Main Content */}
            <div className="bg-white rounded-lg shadow-md border border-gray-200 p-6">
                <div className="mb-6 flex items-center justify-between">
                    <h2 className="text-2xl font-bold text-gray-800 flex items-center gap-3">
                        <Clock className="w-8 h-8 text-blue-600" />
                        Quản Lý Ca Làm Việc
                    </h2>
                    <button
                        onClick={handleOpenAddModal}
                        className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors"
                    >
                        <Plus size={16} />
                        Thêm ca mới
                    </button>
                </div>

                {/* Loading State */}
                {loading && (
                    <div className="text-center py-10">
                        <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                        <p className="mt-2 text-gray-600">Đang tải dữ liệu...</p>
                    </div>
                )}

                {/* Error State */}
                {error && (
                    <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-4">
                        <p className="font-medium">Có lỗi xảy ra:</p>
                        <p className="text-sm">{error}</p>
                    </div>
                )}

                {/* Table */}
                {!loading && !error && (
                    <div className="overflow-x-auto rounded-lg border">
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50">
                                <tr>
                                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tên Ca</th>
                                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Bắt Đầu</th>
                                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Kết Thúc</th>
                                    <th scope="col" className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Hành Động</th>
                                </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-200">
                                {getShiftMn.map((shift) => (
                                    <tr key={shift.shiftId} className="hover:bg-gray-50 transition-colors">
                                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{shift.name}</td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700 font-mono">{shift.startTime}</td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700 font-mono">{shift.endTime}</td>
                                        <td className="px-6 py-4 whitespace-nowrap text-center text-sm font-medium">
                                            <button
                                                title='Chỉnh Sửa'
                                                onClick={() => handleOpenEditModal(shift)}
                                                className="text-indigo-600 hover:text-indigo-900 p-2 hover:bg-indigo-50 rounded transition-colors"
                                            >
                                                <Edit size={16} />
                                            </button>
                                            <button
                                                title='Xóa'
                                                onClick={() => handleDelete(shift.shiftId)}
                                                className="text-red-600 hover:text-red-900 ml-4 p-2 hover:bg-red-50 rounded transition-colors"
                                            >
                                                <Trash2 size={16} />
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}

                {/* Empty State */}
                {!loading && !error && getShiftMn.length === 0 && (
                    <div className="text-center py-10 text-gray-500">
                        <Clock className="w-12 h-12 mx-auto mb-3 text-gray-400" />
                        <p className="text-lg font-medium">Chưa có ca làm việc nào được tạo.</p>
                        <p className="text-sm mt-1">Nhấn "Thêm ca mới" để bắt đầu tạo ca làm việc.</p>
                    </div>
                )}
            </div>

            {/* Modal for Add/Edit Shift */}
            {isModalOpen && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-xl shadow-2xl w-full max-w-lg">
                        {/* Modal Header */}
                        <div className="bg-gradient-to-r from-blue-600 to-blue-700 text-white px-6 py-4 rounded-t-xl">
                            <h3 className="text-xl font-bold flex items-center gap-2">
                                <Clock size={24} />
                                {modalMode === 'add' ? 'Thêm Ca Làm Việc Mới' : 'Chỉnh Sửa Ca Làm Việc'}
                            </h3>
                        </div>

                        {/* Modal Body */}
                        <div className="p-6">
                            <div className="space-y-5">
                                {/* Tên ca */}
                                <div>
                                    <label htmlFor="name" className="block text-sm font-semibold text-gray-700 mb-2">
                                        Tên ca <span className="text-red-500">*</span>
                                    </label>
                                    <input
                                        type="text"
                                        name="name"
                                        id="name"
                                        value={shiftForm.name}
                                        onChange={handleInputChange}
                                        placeholder="Ví dụ: Ca sáng, Ca chiều, Ca tối"
                                        className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${formErrors.name ? 'border-red-500' : 'border-gray-300'
                                            }`}
                                    />
                                    {formErrors.name && (
                                        <p className="text-red-500 text-xs mt-1">{formErrors.name}</p>
                                    )}
                                </div>

                                {/* Giờ bắt đầu */}
                                <div>
                                    <label htmlFor="startTime" className="block text-sm font-semibold text-gray-700 mb-2">
                                        Giờ bắt đầu <span className="text-red-500">*</span>
                                    </label>
                                    <input
                                        type="time"
                                        name="startTime"
                                        id="startTime"
                                        value={shiftForm.startTime}
                                        onChange={handleInputChange}
                                        className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${formErrors.startTime ? 'border-red-500' : 'border-gray-300'
                                            }`}
                                    />
                                    {formErrors.startTime && (
                                        <p className="text-red-500 text-xs mt-1">{formErrors.startTime}</p>
                                    )}
                                </div>

                                {/* Giờ kết thúc */}
                                <div>
                                    <label htmlFor="endTime" className="block text-sm font-semibold text-gray-700 mb-2">
                                        Giờ kết thúc <span className="text-red-500">*</span>
                                    </label>
                                    <input
                                        type="time"
                                        name="endTime"
                                        id="endTime"
                                        value={shiftForm.endTime}
                                        onChange={handleInputChange}
                                        className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${formErrors.endTime ? 'border-red-500' : 'border-gray-300'
                                            }`}
                                    />
                                    {formErrors.endTime && (
                                        <p className="text-red-500 text-xs mt-1">{formErrors.endTime}</p>
                                    )}
                                </div>
                            </div>

                            {/* Modal Footer */}
                            <div className="mt-8 flex justify-end gap-3 pt-5 border-t">
                                <button
                                    type="button"
                                    onClick={handleCloseModal}
                                    disabled={isSubmitting}
                                    className="px-5 py-2.5 bg-gray-200 text-gray-800 font-medium rounded-lg hover:bg-gray-300 transition-colors disabled:opacity-50"
                                >
                                    Hủy
                                </button>
                                <button
                                    type="button"
                                    onClick={handleSubmit}
                                    disabled={isSubmitting}
                                    className={`px-5 py-2.5 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition-colors ${isSubmitting ? 'opacity-50 cursor-not-allowed' : ''
                                        }`}
                                >
                                    {isSubmitting ? 'Đang xử lý...' : 'Lưu'}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
};

export default ManagerShiftAssignment;