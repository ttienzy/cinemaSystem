

import React, { useState, useEffect } from 'react';
import { Users, Phone, Mail, BadgeCheck, BadgeX, PlusCircle } from 'lucide-react';

import Modal from '../../../components/common/Modal';
import AddStaffForm from '../../../components/manager-cpnts/AddStaffForm';
import { type GetStaffs } from '../../../types/staff.types'; // Sử 



// 2. Dữ liệu Mock (đã cập nhật salary)
const mockStaffData: GetStaffs[] = [
    {
        id: 'staff-001',
        fullName: 'Nguyễn Văn An',
        position: 'Trưởng phòng kinh doanh',
        department: 'Kinh Doanh',
        phone: '0987654321',
        email: 'annv@cinema.com',
        address: '123 Đường ABC, Quận 1, TP. HCM',
        hireDate: '2022-08-20T00:00:00Z',
        salary: 120000, // 120k/giờ
        status: 'Active',
    },
    {
        id: 'staff-002',
        fullName: 'Trần Thị Bích',
        position: 'Nhân viên bán vé',
        department: 'Quầy Vé',
        phone: '0912345678',
        email: 'bichtp@cinema.com',
        address: '456 Đường XYZ, Quận 3, TP. HCM',
        hireDate: '2023-01-10T00:00:00Z',
        salary: 45000, // 45k/giờ
        status: 'Active',
    },
    {
        id: 'staff-003',
        fullName: 'Lê Minh Cường',
        position: 'Nhân viên kỹ thuật',
        department: 'Kỹ Thuật',
        phone: '0905112233',
        email: 'cuonglm@cinema.com',
        address: '789 Đường LMN, Quận Gò Vấp, TP. HCM',
        hireDate: '2021-11-05T00:00:00Z',
        salary: 60000, // 60k/giờ
        status: 'Terminated',
    },
    {
        id: 'staff-004',
        fullName: 'Phạm Thị Dung',
        position: 'Nhân viên soát vé',
        department: 'Sảnh Chờ',
        phone: '0938445566',
        email: 'dungpt@cinema.com',
        address: '101 Đường OPQ, Quận Tân Bình, TP. HCM',
        hireDate: '2023-03-12T00:00:00Z',
        salary: 42000, // 42k/giờ
        status: 'Active',
    },
];

const ManagerStaffList: React.FC = () => {
    const [staffs, setStaffs] = useState<GetStaffs[]>([]);
    const [isModalOpen, setIsModalOpen] = useState(false);

    useEffect(() => {
        localStorage.setItem('cinemaId', '3fa85f64-5717-4562-b3fc-2c963f66afa6');
        setStaffs(mockStaffData);
    }, []);

    const handleStatusChange = (staff: GetStaffs) => {
        const newStatus = staff.status === 'Active' ? 'Vô hiệu hóa' : 'Kích hoạt';
        const confirmation = window.confirm(
            `Bạn có chắc chắn muốn ${newStatus} tài khoản của nhân viên "${staff.fullName}" không?`
        );

        if (confirmation) {
            alert(`Đã gửi yêu cầu ${newStatus} cho nhân viên "${staff.fullName}".`);
        }
    };
    const handleAddStaff = (newStaff: GetStaffs) => {
        setStaffs(prevStaffs => [newStaff, ...prevStaffs]);
        setIsModalOpen(false);
    };

    return (
        <div className="bg-white rounded-lg shadow-md border border-gray-200 p-6">
            <div className="mb-6 flex items-center justify-between">
                <h2 className="text-2xl font-bold text-gray-800 flex items-center gap-3">
                    <Users className="w-8 h-8 text-blue-600" />
                    Quản Lý Nhân Viên
                </h2>
                <button
                    onClick={() => setIsModalOpen(true)}
                    className="flex items-center gap-2 bg-blue-600 text-white font-semibold px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors"
                >
                    <PlusCircle size={20} />
                    Thêm nhân viên
                </button>
            </div>

            <div className="overflow-x-auto rounded-lg border">
                <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                        <tr>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Họ Tên</th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Phòng Ban & Vị Trí</th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Thông Tin Liên Hệ</th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Ngày Vào Làm</th>
                            {/* Tiêu đề cột lương đã được cập nhật */}
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Mức Lương</th>
                            <th scope="col" className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Trạng Thái</th>
                        </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                        {staffs.map((staff) => (
                            <tr key={staff.id} className="hover:bg-gray-50 transition-colors duration-200">
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <div className="text-sm font-medium text-gray-900">{staff.fullName}</div>
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <div className="text-sm text-gray-900">{staff.department}</div>
                                    <div className="text-sm text-gray-500">{staff.position}</div>
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                                    <div className="flex items-center gap-2">
                                        <Phone size={14} /> {staff.phone}
                                    </div>
                                    <div className="flex items-center gap-2 mt-1">
                                        <Mail size={14} /> {staff.email}
                                    </div>
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700">
                                    {new Date(staff.hireDate).toLocaleDateString('vi-VN')}
                                </td>

                                {/* === PHẦN HIỂN THỊ LƯƠNG ĐÃ ĐƯỢC THAY ĐỔI === */}
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700">
                                    {`${staff.salary.toLocaleString('vi-VN')} đ / giờ`}
                                </td>

                                <td className="px-6 py-4 whitespace-nowrap text-center">
                                    <button
                                        onClick={() => handleStatusChange(staff)}
                                        className={`inline-flex items-center gap-2 text-xs font-semibold px-3 py-1.5 rounded-full transition-transform transform hover:scale-105 ${staff.status === 'Active'
                                            ? 'bg-green-100 text-green-800 hover:bg-green-200'
                                            : 'bg-red-100 text-red-800 hover:bg-red-200'
                                            }`}
                                    >
                                        {staff.status === 'Active' ? <BadgeCheck size={14} /> : <BadgeX size={14} />}
                                        {staff.status === 'Active' ? 'Đang hoạt động' : 'Đã nghỉ việc'}
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
            {staffs.length === 0 && (
                <div className="text-center py-10 text-gray-500">
                    Không tìm thấy thông tin nhân viên.
                </div>
            )}
            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title="Thêm nhân viên mới"
            >
                <AddStaffForm
                    onCancel={() => setIsModalOpen(false)}
                    onSubmit={handleAddStaff}
                />
            </Modal>
        </div>
    );
};

export default ManagerStaffList;