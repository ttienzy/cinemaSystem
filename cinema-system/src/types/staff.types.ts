interface Staff {
    id: string;
    fullName: string;
    position: string;
    department: string;
    phone: string;
    email: string;
    shifts: Shift[];
}
interface Shift {
    shiftId: string;
    startTime: string;
    endTime: string;
    shiftDate: string;
}

interface GetShiftsMn extends Shift {
    name: string
}


interface StaffWorking {
    id: string;
    cinemaId: string;
}
interface GetStaffsMn {
    id: string;
    fullName: string;
    position: string;
    department: string;
    phone: string;
    email: string;
    address: string;
    hireDate: string;
    salary: number;
    status: string;
}

interface TakeAttendanceOfEmployeeRequest {
    staffId: string;
    shiftId: string;
}

/**
 * @description Đại diện cho dữ liệu của một nhân viên khi hiển thị trong danh sách.
 * Đây là kiểu dữ liệu bạn nhận được từ API GET /staffs.
 */
interface GetStaffs {
    id: string;
    fullName: string;
    position: string;
    department: string;
    phone: string;
    email: string;
    address: string;
    hireDate: string; // ISO string format
    salary: number;
    status: string;
}

/**
 * @description Đại diện cho payload (dữ liệu gửi đi) khi tạo một nhân viên mới.
 * Đây là kiểu dữ liệu mà API POST /add-emp-to-cinema yêu cầu.
 */
interface AddStaffPayload {
    cinemaId: string;
    email: string;
    fullName: string;
    phoneNumber: string;
    address: string;
    position: string;
    department: string;
    hireDate: string; // ISO string format
    salary: number;
    password: string;
    roles: string[];
}

/**
 * @description Đại diện cho các trường dữ liệu người dùng nhập vào trong form.
 * Nó không chứa các trường tự sinh hoặc lấy từ hệ thống như cinemaId, roles, hireDate.
 */
interface AddStaffFormInput {
    fullName: string;
    email: string;
    password: string;
    phoneNumber: string;
    address: string;
    position: string;
    department: string;
    salary: number;
}

export type { Staff, Shift, StaffWorking, TakeAttendanceOfEmployeeRequest, GetShiftsMn, GetStaffsMn, GetStaffs, AddStaffPayload, AddStaffFormInput };