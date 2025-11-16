import type { AddShiftToEmployeeRequest, AddStaffRequest, GetShiftsMn, GetStaffsMn, Shift, Staff, StaffWorking } from "../../types/staff.types";
import { staffServices } from "../../services/staff.services";
import type { TakeAttendanceOfEmployeeRequest } from "../../types/staff.types";
import { createAsyncThunk, createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface StaffState {
    schedules: Staff[];
    shiftsInfo: Shift[];
    getShiftMn: GetShiftsMn[];
    getStaffMn: GetStaffsMn[];
    staffWorking: StaffWorking | null;
    loading: boolean;
    error: string | null;
}

const initialState: StaffState = {
    schedules: [],
    shiftsInfo: [],
    getShiftMn: [],
    getStaffMn: [],
    staffWorking: null,
    loading: false,
    error: null,
};
export const getStaffSchedule = createAsyncThunk(
    'inventory/getStaffSchedule',
    async ({ cinemaId, selectedDate }: { cinemaId: string; selectedDate?: string }) => {
        return await staffServices.getStaffSchedule(cinemaId, selectedDate);
    }
)
export const getStaffWorkingInfo = createAsyncThunk(
    'staff/GetStaffOnTime',
    async (email: string) => {
        return await staffServices.getStaffOnOnWork(email);
    }
)
export const takeAttendanceOfEmployee = createAsyncThunk(
    'staff/takeAttendanceOfEmployee',
    async (attendanceRequest: TakeAttendanceOfEmployeeRequest) => {
        return await staffServices.takeAttendanceOfEmployee(attendanceRequest);
    }
)
export const getShifts = createAsyncThunk(
    'staff/getShifts',
    async (cinemaId: string) => {
        return await staffServices.getShifts(cinemaId);
    }
)
export const getShiftsMn = createAsyncThunk(
    'staff/getShiftsMn',
    async (cinemaId: string) => {
        return await staffServices.getShiftToCinemaForManager(cinemaId);
    }
)
export const getStaffsMn = createAsyncThunk(
    'staff/getStaffsMn',
    async (cinemaId: string) => {
        return await staffServices.getStaffToCinemaForManager(cinemaId);
    }
)
export const addStaffToCinemaForManager = createAsyncThunk(
    'staff/addStaffToCinemaForManager',
    async (payload: AddStaffRequest) => {
        return await staffServices.addStaffToCinemaForManager(payload);
    }
)
export const addShiftToCinemaForManager = createAsyncThunk(
    'staff/addShiftToCinemaForManager',
    async (request: AddShiftToEmployeeRequest[]) => {
        return await staffServices.addShiftToCinemaForManager(request);
    }
)
export const updateShiftToCinemaForManager = createAsyncThunk(
    'staff/updateShiftToCinemaForManager',
    async ({ shiftId, request }: { shiftId: string; request: { cinemaId: string; startTime: string; endTime: string; name: string } }) => {
        return await staffServices.updateShiftToCinemaForManager(shiftId, request);
    }
)
export const deleteShiftToCinemaForManager = createAsyncThunk(
    'staff/deleteShiftToCinemaForManager',
    async (shiftId: string) => {
        return await staffServices.deleteShiftToCinemaForManager(shiftId);
    }
)

const staffSlice = createSlice({
    name: 'staff',
    initialState,
    reducers: {
        setInfo: (state, action: PayloadAction<StaffWorking>) => {
            state.staffWorking = action.payload;

            localStorage.setItem('staffId', action.payload.id);
            localStorage.setItem('cinemaId', action.payload.cinemaId);
        },
        updateSchedules: (state, action: PayloadAction<any>) => {
            state.schedules.forEach((schedule) => {
                if (schedule.id === action.payload.staffId) {
                    schedule.shifts.push({
                        shiftId: action.payload.shiftId,
                        endTime: action.payload.endTime,
                        startTime: action.payload.startTime,
                        name: action.payload.name
                    })
                }
            })
        }
    },
    extraReducers: (builder) => {
        builder.addCase(getStaffSchedule.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(getStaffSchedule.fulfilled, (state, action) => {
            state.schedules = action.payload;
            state.loading = false;
            state.error = null;
        });
        builder.addCase(getStaffSchedule.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to fetch staff schedule';
        });

        builder.addCase(getStaffWorkingInfo.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(getStaffWorkingInfo.fulfilled, (state, action) => {
            state.staffWorking = action.payload;
            console.log(action.payload);
            localStorage.setItem('staffId', action.payload.id);
            localStorage.setItem('cinemaId', action.payload.cinemaId);
            state.loading = false;
            state.error = null;
        });
        builder.addCase(getStaffWorkingInfo.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to fetch staff working';
        });
        builder.addCase(takeAttendanceOfEmployee.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(takeAttendanceOfEmployee.fulfilled, (state, action) => {
            staffSlice.caseReducers.updateSchedules(state, action);
            state.loading = false;
            state.error = null;
        });
        builder.addCase(takeAttendanceOfEmployee.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to take attendance';
        });
        builder.addCase(getShifts.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(getShifts.fulfilled, (state, action) => {
            state.shiftsInfo = action.payload;
            state.loading = false;
            state.error = null;
        });
        builder.addCase(getShifts.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to get shifts';
        });
        builder.addCase(getShiftsMn.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(getShiftsMn.fulfilled, (state, action) => {
            state.getShiftMn = action.payload;
            state.loading = false;
            state.error = null;
        });
        builder.addCase(getShiftsMn.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to get shifts';
        });
        builder.addCase(getStaffsMn.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(getStaffsMn.fulfilled, (state, action) => {
            state.getStaffMn = action.payload;
            state.loading = false;
            state.error = null;
        });
        builder.addCase(getStaffsMn.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to get staffs';
        });
        builder.addCase(deleteShiftToCinemaForManager.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(deleteShiftToCinemaForManager.fulfilled, (state) => {
            state.loading = false;
            state.error = null;
        });
        builder.addCase(deleteShiftToCinemaForManager.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to delete shift';
        });
        builder.addCase(addShiftToCinemaForManager.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(addShiftToCinemaForManager.fulfilled, (state) => {
            state.loading = false;
            state.error = null;
        });
        builder.addCase(addShiftToCinemaForManager.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to add shift';
        });
        builder.addCase(updateShiftToCinemaForManager.pending, (state) => {
            state.loading = true;
            state.error = null;
        });
        builder.addCase(updateShiftToCinemaForManager.fulfilled, (state) => {
            state.loading = false;
            state.error = null;
        });
        builder.addCase(updateShiftToCinemaForManager.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message || 'Failed to update shift';
        });
    }
});

export const { setInfo, updateSchedules } = staffSlice.actions;
export default staffSlice.reducer;