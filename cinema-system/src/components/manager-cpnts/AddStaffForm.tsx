import React, { useState } from 'react';
import { X, User, Mail, Phone, MapPin, Briefcase, Calendar, DollarSign, Lock, Shield } from 'lucide-react';
import { useAppDispatch } from '../../hooks/redux';
import { addStaffToCinemaForManager, getStaffsMn } from '../../store/slices/staffSlice';
import type { AddStaffRequest } from '../../types/staff.types';
interface AddStaffFormProps {
    onClose: () => void;
    onSubmit: (staff: any) => void;
}

const AddStaffForm: React.FC<AddStaffFormProps> = ({ onClose, onSubmit }) => {
    const [formData, setFormData] = useState({
        fullName: '',
        email: '',
        phoneNumber: '',
        address: '',
        position: '',
        department: '',
        hireDate: '',
        salary: '',
        password: '',
        roles: ['Employee']
    });

    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);

    const departments = ['Bán vé', 'Bán đồ ăn', 'Kỹ thuật', 'Bảo vệ', 'Vệ sinh'];
    const positions = ['Nhân viên', 'Trưởng ca', 'Giám sát', 'Quản lý'];
    const roleOptions = ['Employee', 'Manager', 'Admin'];
    const dispatch = useAppDispatch();

    const validateForm = () => {
        const newErrors: Record<string, string> = {};

        if (!formData.fullName.trim()) {
            newErrors.fullName = 'Vui lòng nhập họ tên';
        }

        if (!formData.email.trim()) {
            newErrors.email = 'Vui lòng nhập email';
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
            newErrors.email = 'Email không hợp lệ';
        }

        if (!formData.phoneNumber.trim()) {
            newErrors.phoneNumber = 'Vui lòng nhập số điện thoại';
        } else if (!/^[0-9]{10,11}$/.test(formData.phoneNumber)) {
            newErrors.phoneNumber = 'Số điện thoại không hợp lệ (10-11 số)';
        }

        if (!formData.address.trim()) {
            newErrors.address = 'Vui lòng nhập địa chỉ';
        }

        if (!formData.position) {
            newErrors.position = 'Vui lòng chọn vị trí';
        }

        if (!formData.department) {
            newErrors.department = 'Vui lòng chọn phòng ban';
        }

        if (!formData.hireDate) {
            newErrors.hireDate = 'Vui lòng chọn ngày vào làm';
        }

        if (!formData.salary || Number(formData.salary) <= 0) {
            newErrors.salary = 'Vui lòng nhập mức lương hợp lệ';
        }

        if (!formData.password.trim()) {
            newErrors.password = 'Vui lòng nhập mật khẩu';
        } else if (formData.password.length < 6) {
            newErrors.password = 'Mật khẩu phải có ít nhất 6 ký tự';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
        // Xóa lỗi khi người dùng bắt đầu nhập
        if (errors[name]) {
            setErrors(prev => ({
                ...prev,
                [name]: ''
            }));
        }
    };

    const handleRoleChange = (role: string) => {
        setFormData(prev => ({
            ...prev,
            roles: prev.roles.includes(role)
                ? prev.roles.filter(r => r !== role)
                : [...prev.roles, role]
        }));
    };

    const handleSubmit = async (e: React.MouseEvent<HTMLButtonElement>) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        setIsSubmitting(true);

        try {
            const cinemaId = localStorage.getItem('cinemaId');

            if (!cinemaId) {
                alert('Không tìm thấy thông tin rạp. Vui lòng đăng nhập lại.');
                setIsSubmitting(false);
                return;
            }

            const staffData: AddStaffRequest = {
                cinemaId: cinemaId,
                email: formData.email,
                fullName: formData.fullName,
                phoneNumber: formData.phoneNumber,
                address: formData.address,
                position: formData.position,
                department: formData.department,
                hireDate: new Date(formData.hireDate).toISOString(),
                salary: Number(formData.salary),
                password: formData.password,
                roles: formData.roles
            };
            dispatch(addStaffToCinemaForManager(staffData));
            dispatch(getStaffsMn(cinemaId));

            // TODO: Gọi API ở đây
            // const response = await fetch('YOUR_API_ENDPOINT', {
            //   method: 'POST',
            //   headers: {
            //     'Content-Type': 'application/json',
            //   },
            //   body: JSON.stringify(staffData)
            // });
            // const result = await response.json();

            console.log('Dữ liệu gửi đi:', staffData);

            // Giả lập API call
            await new Promise(resolve => setTimeout(resolve, 1000));

            onSubmit(staffData);
            alert('Thêm nhân viên thành công!');
            onClose();
        } catch (error) {
            console.error('Lỗi khi thêm nhân viên:', error);
            alert('Có lỗi xảy ra. Vui lòng thử lại!');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-xl shadow-2xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
                {/* Header */}
                <div className="sticky top-0 bg-gradient-to-r from-blue-600 to-blue-700 text-white p-6 flex justify-between items-center">
                    <h2 className="text-2xl font-bold">Thêm Nhân Viên Mới</h2>
                    <button
                        onClick={onClose}
                        className="hover:bg-white hover:bg-opacity-20 rounded-full p-2 transition-colors"
                    >
                        <X size={24} />
                    </button>
                </div>

                {/* Form */}
                <div className="p-6">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        {/* Họ tên */}
                        <div>
                            <label className="flex items-center gap-2 text-sm font-semibold text-gray-700 mb-2">
                                <User size={18} className="text-blue-600" />
                                Họ và tên <span className="text-red-500">*</span>
                            </label>
                            <input
                                type="text"
                                name="fullName"
                                value={formData.fullName}
                                onChange={handleChange}
                                className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.fullName ? 'border-red-500' : 'border-gray-300'
                                    }`}
                                placeholder="Nguyễn Văn A"
                            />
                            {errors.fullName && (
                                <p className="text-red-500 text-xs mt-1">{errors.fullName}</p>
                            )}
                        </div>

                        {/* Email */}
                        <div>
                            <label className="flex items-center gap-2 text-sm font-semibold text-gray-700 mb-2">
                                <Mail size={18} className="text-blue-600" />
                                Email <span className="text-red-500">*</span>
                            </label>
                            <input
                                type="email"
                                name="email"
                                value={formData.email}
                                onChange={handleChange}
                                className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.email ? 'border-red-500' : 'border-gray-300'
                                    }`}
                                placeholder="example@email.com"
                            />
                            {errors.email && (
                                <p className="text-red-500 text-xs mt-1">{errors.email}</p>
                            )}
                        </div>

                        {/* Số điện thoại */}
                        <div>
                            <label className="flex items-center gap-2 text-sm font-semibold text-gray-700 mb-2">
                                <Phone size={18} className="text-blue-600" />
                                Số điện thoại <span className="text-red-500">*</span>
                            </label>
                            <input
                                type="tel"
                                name="phoneNumber"
                                value={formData.phoneNumber}
                                onChange={handleChange}
                                className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.phoneNumber ? 'border-red-500' : 'border-gray-300'
                                    }`}
                                placeholder="0123456789"
                            />
                            {errors.phoneNumber && (
                                <p className="text-red-500 text-xs mt-1">{errors.phoneNumber}</p>
                            )}
                        </div>

                        {/* Địa chỉ */}
                        <div>
                            <label className="flex items-center gap-2 text-sm font-semibold text-gray-700 mb-2">
                                <MapPin size={18} className="text-blue-600" />
                                Địa chỉ <span className="text-red-500">*</span>
                            </label>
                            <input
                                type="text"
                                name="address"
                                value={formData.address}
                                onChange={handleChange}
                                className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.address ? 'border-red-500' : 'border-gray-300'
                                    }`}
                                placeholder="123 Đường ABC, Quận XYZ"
                            />
                            {errors.address && (
                                <p className="text-red-500 text-xs mt-1">{errors.address}</p>
                            )}
                        </div>

                        {/* Phòng ban */}
                        <div>
                            <label className="flex items-center gap-2 text-sm font-semibold text-gray-700 mb-2">
                                <Briefcase size={18} className="text-blue-600" />
                                Phòng ban <span className="text-red-500">*</span>
                            </label>
                            <select
                                name="department"
                                value={formData.department}
                                onChange={handleChange}
                                className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.department ? 'border-red-500' : 'border-gray-300'
                                    }`}
                            >
                                <option value="">Chọn phòng ban</option>
                                {departments.map(dept => (
                                    <option key={dept} value={dept}>{dept}</option>
                                ))}
                            </select>
                            {errors.department && (
                                <p className="text-red-500 text-xs mt-1">{errors.department}</p>
                            )}
                        </div>

                        {/* Vị trí */}
                        <div>
                            <label className="flex items-center gap-2 text-sm font-semibold text-gray-700 mb-2">
                                <Briefcase size={18} className="text-blue-600" />
                                Vị trí <span className="text-red-500">*</span>
                            </label>
                            <select
                                name="position"
                                value={formData.position}
                                onChange={handleChange}
                                className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.position ? 'border-red-500' : 'border-gray-300'
                                    }`}
                            >
                                <option value="">Chọn vị trí</option>
                                {positions.map(pos => (
                                    <option key={pos} value={pos}>{pos}</option>
                                ))}
                            </select>
                            {errors.position && (
                                <p className="text-red-500 text-xs mt-1">{errors.position}</p>
                            )}
                        </div>

                        {/* Ngày vào làm */}
                        <div>
                            <label className="flex items-center gap-2 text-sm font-semibold text-gray-700 mb-2">
                                <Calendar size={18} className="text-blue-600" />
                                Ngày vào làm <span className="text-red-500">*</span>
                            </label>
                            <input
                                type="date"
                                name="hireDate"
                                value={formData.hireDate}
                                onChange={handleChange}
                                className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.hireDate ? 'border-red-500' : 'border-gray-300'
                                    }`}
                            />
                            {errors.hireDate && (
                                <p className="text-red-500 text-xs mt-1">{errors.hireDate}</p>
                            )}
                        </div>

                        {/* Mức lương */}
                        <div>
                            <label className="flex items-center gap-2 text-sm font-semibold text-gray-700 mb-2">
                                <DollarSign size={18} className="text-blue-600" />
                                Mức lương (đ/giờ) <span className="text-red-500">*</span>
                            </label>
                            <input
                                type="number"
                                name="salary"
                                value={formData.salary}
                                onChange={handleChange}
                                className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.salary ? 'border-red-500' : 'border-gray-300'
                                    }`}
                                placeholder="50000"
                                min="0"
                            />
                            {errors.salary && (
                                <p className="text-red-500 text-xs mt-1">{errors.salary}</p>
                            )}
                        </div>

                        {/* Mật khẩu */}
                        <div>
                            <label className="flex items-center gap-2 text-sm font-semibold text-gray-700 mb-2">
                                <Lock size={18} className="text-blue-600" />
                                Mật khẩu <span className="text-red-500">*</span>
                            </label>
                            <input
                                type="password"
                                name="password"
                                value={formData.password}
                                onChange={handleChange}
                                className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${errors.password ? 'border-red-500' : 'border-gray-300'
                                    }`}
                                placeholder="Nhập mật khẩu"
                            />
                            {errors.password && (
                                <p className="text-red-500 text-xs mt-1">{errors.password}</p>
                            )}
                        </div>

                        {/* Vai trò */}
                        <div className="md:col-span-2">
                            <label className="flex items-center gap-2 text-sm font-semibold text-gray-700 mb-2">
                                <Shield size={18} className="text-blue-600" />
                                Vai trò <span className="text-red-500">*</span>
                            </label>
                            <div className="flex flex-wrap gap-3">
                                {roleOptions.map(role => (
                                    <label
                                        key={role}
                                        className="flex items-center gap-2 cursor-pointer bg-gray-50 px-4 py-2 rounded-lg hover:bg-gray-100 transition-colors"
                                    >
                                        <input
                                            type="checkbox"
                                            checked={formData.roles.includes(role)}
                                            onChange={() => handleRoleChange(role)}
                                            className="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
                                        />
                                        <span className="text-sm font-medium text-gray-700">{role}</span>
                                    </label>
                                ))}
                            </div>
                        </div>
                    </div>

                    {/* Buttons */}
                    <div className="flex gap-4 mt-8 pt-6 border-t">
                        <button
                            type="button"
                            onClick={onClose}
                            className="flex-1 px-6 py-3 border border-gray-300 text-gray-700 font-semibold rounded-lg hover:bg-gray-50 transition-colors"
                        >
                            Hủy
                        </button>
                        <button
                            type="button"
                            onClick={handleSubmit}
                            disabled={isSubmitting}
                            className={`flex-1 px-6 py-3 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition-colors ${isSubmitting ? 'opacity-50 cursor-not-allowed' : ''
                                }`}
                        >
                            {isSubmitting ? 'Đang xử lý...' : 'Thêm nhân viên'}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default AddStaffForm;