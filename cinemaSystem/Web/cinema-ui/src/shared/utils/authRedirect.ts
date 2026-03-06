// Auth Redirect Helper - Calculate redirect path based on user roles
// Role priority: Admin > Manager > Customer

export type UserRole = 'Admin' | 'Manager' | 'Customer';

/**
 * Get the highest priority redirect path based on user roles
 * Uses role hierarchy: Admin > Manager > Customer
 */
export function getRedirectPath(roles: string[]): string {
    if (!roles || roles.length === 0) {
        return '/';
    }

    // Normalize roles to lowercase for consistent comparison
    const normalizedRoles = roles.map(r => r.toLowerCase());

    // Check roles in priority order (case-insensitive)
    if (normalizedRoles.includes('admin')) {
        return '/admin';
    }

    if (normalizedRoles.includes('manager')) {
        return '/admin';
    }

    // Default redirect for customers or any other role
    return '/';
}

/**
 * Get role priority number (lower = higher priority)
 */
export function getRolePriority(role: string): number {
    const normalized = role.toUpperCase();

    switch (normalized) {
        case 'ADMIN':
            return 1;
        case 'MANAGER':
            return 2;
        default:
            return 3;
    }
}

/**
 * Get the highest priority role from a list of roles
 */
export function getHighestPriorityRole(roles: string[]): string | null {
    if (!roles || roles.length === 0) {
        return null;
    }

    return roles.reduce((highest, current) => {
        const currentPriority = getRolePriority(current);
        const highestPriority = getRolePriority(highest);
        return currentPriority < highestPriority ? current : highest;
    }, roles[0]);
}

/**
 * Check if user has admin access (case-insensitive)
 */
export function hasAdminAccess(roles: string[]): boolean {
    if (!roles || roles.length === 0) {
        return false;
    }

    // Normalize roles to lowercase for comparison
    const normalizedRoles = roles.map(r => r.toLowerCase());

    return normalizedRoles.includes('admin') || normalizedRoles.includes('manager');
}

export default {
    getRedirectPath,
    getRolePriority,
    getHighestPriorityRole,
    hasAdminAccess,
};
