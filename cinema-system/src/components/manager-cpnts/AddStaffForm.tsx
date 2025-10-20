// src/components/manager/AddStaffForm.tsx

import React, { useState } from 'react';
// Import các types mới
import type { AddStaffPayload, AddStaffFormInput, GetStaffs } from '../../types/staff.types';

interface AddStaffFormProps {
    onCancel: () => void;
    // Callback onSubmit bây giờ sẽ trả về một đối tượng Staff hoàn chỉnh
    // giống như cách API sẽ trả về sau khi tạo thành công.
    onSubmit: (newlyCreatedStaff: GetStaffs) => void;
}

const AddStaffForm: React.FC<AddStaffFormProps> = ({ onCancel, onSubmit }) => {
    // State cho form sẽ dựa trên AddStaffFormInput
    const [formData, setFormData] = useState<Partial<AddStaffFormInput>>({});
    const [errors, setErrors] = useState<Partial<Record<keyof AddStaffFormInput, string>>>({});

    const validateForm = (): boolean => {
        const newErrors: Partial<Record<keyof AddStaffFormInput, string>> = {};
        if (!formData.fullName?.trim()) newErrors.fullName = "Họ tên không được để trống.";
        if (!formData.email?.trim()) {
            newErrors.email = "Email không được để trống.";
        } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
            newErrors.email = "Email không hợp lệ.";
        }
        if (!formData.password?.trim()) newErrors.password = "Mật khẩu không được để trống.";
        else if (formData.password.length < 6) newErrors.password = "Mật khẩu phải có ít nhất 6 ký tự.";
        if (!formData.phoneNumber?.trim()) newErrors.phoneNumber = "Số điện thoại không được để trống.";
        if (!formData.position?.trim()) newErrors.position = "Vị trí không được để trống.";
        if (!formData.department?.trim()) newErrors.department = "Phòng ban không được để trống.";
        if (!formData.salary || formData.salary <= 0) newErrors.salary = "Mức lương phải là một số dương.";

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value, type } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'number' ? parseFloat(value) || 0 : value,
        }));
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!validateForm()) return;

        // 1. Xây dựng payload để gửi đi (giống hệt API request)
        const payload: AddStaffPayload = {
            cinemaId: localStorage.getItem('cinemaId') || 'unknown-cinema-id',
            fullName: formData.fullName!,
            email: formData.email!,
            password: formData.password!,
            phoneNumber: formData.phoneNumber!,
            address: formData.address || '',
            position: formData.position!,
            department: formData.department!,
            salary: formData.salary!,
            hireDate: new Date().toISOString(),
            roles: ["Employee"], // Mặc định
        };

        console.log("SENDING TO API (MOCK):", payload);

        // 2. MOCK API RESPONSE: Giả lập việc API trả về dữ liệu nhân viên vừa được tạo
        // API sẽ không trả về password, roles, cinemaId. Thay vào đó nó trả về id, status...
        const newlyCreatedStaff: GetStaffs = {
            id: `staff-${new Date().getTime()}`,
            fullName: payload.fullName,
            email: payload.email,
            phone: payload.phoneNumber,
            address: payload.address,
            position: payload.position,
            department: payload.department,
            salary: payload.salary,
            hireDate: payload.hireDate,
            status: 'Active', // Nhân viên mới luôn active
        };

        // 3. Gửi đối tượng Staff hoàn chỉnh về cho component cha để cập nhật UI
        onSubmit(newlyCreatedStaff);
    };

    return (
        <form onSubmit={handleSubmit} noValidate>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Full Name */}
                <div>
                    <label className="block text-sm font-medium text-gray-700">Họ và Tên</label>
                    <input type="text" name="fullName" onChange={handleChange} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" />
                    {errors.fullName && <p className="text-xs text-red-500 mt-1">{errors.fullName}</p>}
                </div>
                {/* Email */}
                <div>
                    <label className="block text-sm font-medium text-gray-700">Email</label>
                    <input type="email" name="email" onChange={handleChange} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" />
                    {errors.email && <p className="text-xs text-red-500 mt-1">{errors.email}</p>}
                </div>
                {/* Password */}
                <div>
                    <label className="block text-sm font-medium text-gray-700">Mật khẩu</label>
                    <input type="password" name="password" onChange={handleChange} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" />
                    {errors.password && <p className="text-xs text-red-500 mt-1">{errors.password}</p>}
                </div>
                {/* Phone Number (đổi name thành phoneNumber) */}
                <div>
                    <label className="block text-sm font-medium text-gray-700">Số điện thoại</label>
                    <input type="tel" name="phoneNumber" onChange={handleChange} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" />
                    {errors.phoneNumber && <p className="text-xs text-red-500 mt-1">{errors.phoneNumber}</p>}
                </div>
                {/* ... các trường còn lại không đổi ... */}
                {/* Department */}
                <div>
                    <label className="block text-sm font-medium text-gray-700">Phòng ban</label>
                    <input type="text" name="department" onChange={handleChange} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" required />
                    {errors.department && <p className="text-xs text-red-500 mt-1">{errors.department}</p>}
                </div>
                {/* Position */}
                <div>
                    <label className="block text-sm font-medium text-gray-700">Vị trí</label>
                    <input type="text" name="position" onChange={handleChange} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" required />
                    {errors.position && <p className="text-xs text-red-500 mt-1">{errors.position}</p>}
                </div>
                {/* Salary */}
                <div>
                    <label className="block text-sm font-medium text-gray-700">Mức lương (VND/giờ)</label>
                    <input type="number" name="salary" onChange={handleChange} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" required />
                    {errors.salary && <p className="text-xs text-red-500 mt-1">{errors.salary}</p>}
                </div>
                {/* Address */}
                <div className="md:col-span-2">
                    <label className="block text-sm font-medium text-gray-700">Địa chỉ</label>
                    <input type="text" name="address" onChange={handleChange} className="mt-1 block w-full border-gray-300 rounded-md shadow-sm" />
                </div>
            </div>

            {/* Form Actions */}
            <div className="mt-8 flex justify-end gap-4">
                <button type="button" onClick={onCancel} className="px-4 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300">Hủy</button>
                <button type="submit" className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">Thêm nhân viên</button>
            </div>
        </form>
    );
};

export default AddStaffForm;