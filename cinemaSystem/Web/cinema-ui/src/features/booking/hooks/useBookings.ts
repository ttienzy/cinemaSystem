// Bookings Hooks - React Query hooks for bookings
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { bookingsApi } from '../api/bookingsApi';
import type { CreateBookingRequest, MyBookingsQuery } from '../types/booking.types';

// Query keys
const bookingKeys = {
    all: ['bookings'] as const,
    lists: () => [...bookingKeys.all, 'list'] as const,
    myList: (params: MyBookingsQuery) => [...bookingKeys.all, 'my', params] as const,
    details: () => [...bookingKeys.all, 'detail'] as const,
    detail: (id: string) => [...bookingKeys.details(), id] as const,
};

export { bookingKeys };

// Hook: Get my bookings (user)
export const useMyBookings = (params?: MyBookingsQuery) => {
    return useQuery({
        queryKey: bookingKeys.myList(params || {}),
        queryFn: () => bookingsApi.getMyBookings(params),
        staleTime: 2 * 60 * 1000,
    });
};

// Hook: Get booking by ID
export const useBooking = (id: string) => {
    return useQuery({
        queryKey: bookingKeys.detail(id),
        queryFn: () => bookingsApi.getBookingById(id),
        enabled: !!id,
        staleTime: 2 * 60 * 1000,
    });
};

// Hook: Create booking
export const useCreateBooking = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (data: CreateBookingRequest) => bookingsApi.createBooking(data),
        onSuccess: () => {
            // Invalidate my bookings list
            queryClient.invalidateQueries({ queryKey: bookingKeys.all });
        },
    });
};

// Hook: Cancel booking
export const useCancelBooking = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: string) => bookingsApi.cancelBooking(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: bookingKeys.all });
        },
    });
};

// Hook: Request refund
export const useRequestRefund = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: string) => bookingsApi.requestRefund(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: bookingKeys.all });
        },
    });
};

// Hook: Check-in
export const useCheckIn = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: string) => bookingsApi.checkIn(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: bookingKeys.all });
        },
    });
};

export default {
    useMyBookings,
    useBooking,
    useCreateBooking,
    useCancelBooking,
    useRequestRefund,
    useCheckIn,
};
