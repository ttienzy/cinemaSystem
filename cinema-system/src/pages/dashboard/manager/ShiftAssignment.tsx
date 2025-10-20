// src/pages/dashboard/manager/ShiftAssignment.tsx

import React, { useState, useEffect } from 'react';
import { Clock, Plus, Edit, Trash2 } from 'lucide-react';

// 1. Định nghĩa Interface cho dữ liệu ca làm việc
interface Shift {
    shiftId: string;
    name: string;
    startTime: string; // Format "HH:mm"
    endTime: string;   // Format "HH:mm"
}

// 2. Dữ liệu Mock (Mô phỏng dữ liệu nhận từ API)
const mockShifts: Shift[] = [
    { shiftId: 'shift-01', name: 'Ca Sáng', startTime: '08:00', endTime: '12:00' },
    { shiftId: 'shift-02', name: 'Ca Chiều', startTime: '13:00', endTime: '17:00' },
    { shiftId: 'shift-03', name: 'Ca Tối', startTime: '18:00', endTime: '22:00' },
];

const ManagerShiftAssignment: React.FC = () => {
    // State quản lý danh sách các ca
    const [shifts, setShifts] = useState<Shift[]>([]);

    // State quản lý trạng thái của modal (mở/đóng, thêm/sửa)
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [modalMode, setModalMode] = useState<'add' | 'edit'>('add');
    const [currentShift, setCurrentShift] = useState<Shift | null>(null);

    // State cho dữ liệu trong form
    const [shiftForm, setShiftForm] = useState({ name: '', startTime: '', endTime: '' });
    const [formErrors, setFormErrors] = useState({ name: '', startTime: '', endTime: '' });

    // Tải dữ liệu mock khi component được mount
    useEffect(() => {
        setShifts(mockShifts);
    }, []);

    // --- CÁC HÀM XỬ LÝ MODAL VÀ FORM ---

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setShiftForm(prev => ({ ...prev, [name]: value }));
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
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!validateForm()) return;

        if (modalMode === 'add') {
            const newShift: Shift = {
                shiftId: `shift-${Date.now()}`, // Tạo ID tạm thời
                ...shiftForm
            };
            setShifts(prev => [...prev, newShift]);
        } else if (currentShift) {
            setShifts(prev => prev.map(s => s.shiftId === currentShift.shiftId ? { ...s, ...shiftForm } : s));
        }

        handleCloseModal();
    };

    const handleDelete = (shiftId: string) => {
        if (window.confirm("Bạn có chắc chắn muốn xóa ca làm việc này không?")) {
            setShifts(prev => prev.filter(s => s.shiftId !== shiftId));
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
                            {shifts.map((shift) => (
                                <tr key={shift.shiftId} className="hover:bg-gray-50 transition-colors">
                                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{shift.name}</td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700 font-mono">{shift.startTime}</td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700 font-mono">{shift.endTime}</td>
                                    <td className="px-6 py-4 whitespace-nowrap text-center text-sm font-medium">
                                        <button
                                            title='Chiềnh Sửa'
                                            onClick={() => handleOpenEditModal(shift)} className="text-indigo-600 hover:text-indigo-900 p-2">
                                            <Edit size={16} />
                                        </button>
                                        <button
                                            title='Xoa'
                                            onClick={() => handleDelete(shift.shiftId)} className="text-red-600 hover:text-red-900 ml-4 p-2">
                                            <Trash2 size={16} />
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
                {shifts.length === 0 && (
                    <div className="text-center py-10 text-gray-500">
                        Chưa có ca làm việc nào được tạo.
                    </div>
                )}
            </div>

            {/* Modal for Add/Edit Shift */}
            {isModalOpen && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
                    <div className="bg-white rounded-lg shadow-xl w-full max-w-lg p-6">
                        <form onSubmit={handleSubmit}>
                            <h3 className="text-xl font-semibold mb-6">
                                {modalMode === 'add' ? 'Thêm Ca Làm Việc Mới' : 'Chỉnh Sửa Ca Làm Việc'}
                            </h3>

                            <div className="space-y-4">
                                <div>
                                    <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">Tên ca</label>
                                    <input type="text" name="name" id="name" value={shiftForm.name} onChange={handleInputChange} className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm" />
                                    {formErrors.name && <p className="text-red-500 text-xs mt-1">{formErrors.name}</p>}
                                </div>
                                <div>
                                    <label htmlFor="startTime" className="block text-sm font-medium text-gray-700 mb-1">Giờ bắt đầu</label>
                                    <input type="time" name="startTime" id="startTime" value={shiftForm.startTime} onChange={handleInputChange} className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm" />
                                    {formErrors.startTime && <p className="text-red-500 text-xs mt-1">{formErrors.startTime}</p>}
                                </div>
                                <div>
                                    <label htmlFor="endTime" className="block text-sm font-medium text-gray-700 mb-1">Giờ kết thúc</label>
                                    <input type="time" name="endTime" id="endTime" value={shiftForm.endTime} onChange={handleInputChange} className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm" />
                                    {formErrors.endTime && <p className="text-red-500 text-xs mt-1">{formErrors.endTime}</p>}
                                </div>
                            </div>

                            <div className="mt-8 flex justify-end gap-3">
                                <button type="button" onClick={handleCloseModal} className="px-4 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300">Hủy</button>
                                <button type="submit" className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">Lưu</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </>
    );
};

export default ManagerShiftAssignment;