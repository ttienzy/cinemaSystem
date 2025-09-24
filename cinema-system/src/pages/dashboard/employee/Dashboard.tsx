// src/pages/dashboard/employee/Dashboard.tsx
import React, { useEffect } from 'react';
import DashboardEmployee from '../../../components/concession/DashboardEmployee';
import { useAppDispatch } from '../../../hooks/redux';
import { getStaffWorkingInfo } from '../../../store/slices/inventorySlice';
import { getEmailFromToken } from '../../../utils/decodeTokenAndGetUser';



const EmployeeDashboard: React.FC = () => {
    const dispatch = useAppDispatch();

    useEffect(() => {
        dispatch(getStaffWorkingInfo(getEmailFromToken()));
    }, [dispatch]);
    return (
        <DashboardEmployee />
    );
};

export default EmployeeDashboard;