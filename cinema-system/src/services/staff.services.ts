import { api } from "../configs/api";
import type { TakeAttendanceOfEmployeeRequest } from "../types/staff.types";



export const staffServices = {
    getStaffSchedule: async (cinemaId: string, selectedDate?: string) => {
        const response = await api.get(`/staffs/schedules/${cinemaId}?ShiftDate=${selectedDate}`);
        return response.data;
    },
    getStaffOnOnWork: async (email: string) => {
        const response = await api.get(`/staffs/staff-on-time?email=${email}`)
        return response.data;
    },
    getShifts: async (cinemaId: string) => {
        const response = await api.get(`/staffs/${cinemaId}/shifts`);
        return response.data;
    },
    takeAttendanceOfEmployee: async (attendanceRequest: TakeAttendanceOfEmployeeRequest) => {
        const response = await api.post(`/staffs/add-shift-to-employee`, attendanceRequest);
        return response.data;
    },
    getStaffToCinemaForManager: async (cinemaId: string) => {
        const response = await api.get(`/staffs/staffs-manager/${cinemaId}`);
        return response.data;
    },
    getShiftToCinemaForManager: async (cinemaId: string) => {
        const response = await api.get(`/staffs/shifts-manager/${cinemaId}`);
        return response.data;
    },

}