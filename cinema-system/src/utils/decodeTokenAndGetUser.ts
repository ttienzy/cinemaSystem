import { jwtDecode } from "jwt-decode";
import type { DecodedToken } from "../types/auth.types";

const decodeTokenAndGetUser = (token: string | null): DecodedToken | null => {
    if (!token) return null;
    try {
        const decodedUser: DecodedToken = jwtDecode(token);
        return decodedUser;
    } catch (error) {
        console.error("Failed to decode token:", error);
        return null;
    }
};

const getRole = (roleInput: string | string[]): string => {
    const validRoles: string[] = ['admin', 'manager', 'employee'];

    // Đưa role về dạng mảng để dễ xử lý
    const roles: string[] = Array.isArray(roleInput) ? roleInput : [roleInput];

    return (
        roles.find(r => validRoles.includes(r.toLowerCase()))?.toLowerCase() || ''
    );
}

const getEmailFromToken = (): string => {
    const token = localStorage.getItem('accessToken') || '';
    try {
        const decodedUser = jwtDecode<any>(token);
        return decodedUser.email;
    } catch (error) {
        console.error("Failed to decode token:", error);
        return '';
    }
};

export { decodeTokenAndGetUser, getRole, getEmailFromToken };