// Booking Store - Zustand for managing booking flow state
import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { Showtime, SeatingPlan } from '../../showtime/types/showtime.types';

export interface SelectedSeat {
    id: string;
    seatNumber: string;
    row: string;
    column: number;
    seatType: string;
    price: number;
}

export type BookingStep = 'select-showtime' | 'select-seats' | 'confirm' | 'payment' | 'complete';

interface BookingState {
    // Current booking step
    currentStep: BookingStep;
    setCurrentStep: (step: BookingStep) => void;

    // Selected movie
    movieId: string | null;
    movieTitle: string | null;
    moviePoster: string | null;
    setMovie: (id: string, title: string, poster?: string) => void;
    clearMovie: () => void;

    // Selected showtime
    showtime: Showtime | null;
    setShowtime: (showtime: Showtime) => void;
    clearShowtime: () => void;

    // Seating plan
    seatingPlan: SeatingPlan | null;
    setSeatingPlan: (plan: SeatingPlan) => void;
    clearSeatingPlan: () => void;

    // Selected seats
    selectedSeats: SelectedSeat[];
    addSeat: (seat: SelectedSeat) => void;
    removeSeat: (seatId: string) => void;
    toggleSeat: (seat: SelectedSeat) => void;
    clearSelectedSeats: () => void;

    // Validation
    validateSeats: () => { isValid: boolean; errors: string[] };

    // Pricing
    totalAmount: number;
    discountAmount: number;
    finalAmount: number;
    updatePricing: (total: number, discount?: number) => void;

    // Booking expiration timer
    expiresAt: string | null;
    setExpiresAt: (time: string) => void;
    clearExpiresAt: () => void;

    // Booking result
    bookingId: string | null;
    bookingCode: string | null;
    setBookingResult: (id: string, code: string) => void;
    clearBookingResult: () => void;

    // Reset booking flow
    resetBooking: () => void;
}

const initialState = {
    currentStep: 'select-showtime' as BookingStep,
    movieId: null,
    movieTitle: null,
    moviePoster: null,
    showtime: null,
    seatingPlan: null,
    selectedSeats: [],
    totalAmount: 0,
    discountAmount: 0,
    finalAmount: 0,
    expiresAt: null,
    bookingId: null,
    bookingCode: null,
};

export const useBookingStore = create<BookingState>()(
    persist(
        (set, get) => ({
            ...initialState,

            setCurrentStep: (step) => set({ currentStep: step }),

            setMovie: (id, title, poster) =>
                set({ movieId: id, movieTitle: title, moviePoster: poster }),

            clearMovie: () =>
                set({ movieId: null, movieTitle: null, moviePoster: null }),

            setShowtime: (showtime) => set({ showtime }),

            clearShowtime: () => set({ showtime: null }),

            setSeatingPlan: (plan) => set({ seatingPlan: plan }),

            clearSeatingPlan: () => set({ seatingPlan: null }),

            addSeat: (seat) =>
                set((state) => {
                    // Validation: Check if seat exists in seating plan
                    if (state.seatingPlan) {
                        const seatInPlan = state.seatingPlan.seats?.find((s) => s.id === seat.id);
                        if (!seatInPlan) {
                            console.error('Seat validation failed: Seat not found in seating plan', seat.id);
                            return state;
                        }
                        // Validation: Check if price matches seating plan
                        if (seatInPlan.price !== seat.price) {
                            console.error('Seat validation failed: Price mismatch', {
                                expected: seatInPlan.price,
                                received: seat.price,
                                seatId: seat.id
                            });
                            return state;
                        }
                    }

                    return {
                        selectedSeats: [...state.selectedSeats, seat],
                        totalAmount: state.totalAmount + seat.price,
                        finalAmount: state.totalAmount + seat.price - state.discountAmount,
                    };
                }),

            removeSeat: (seatId) =>
                set((state) => {
                    const seat = state.selectedSeats.find((s) => s.id === seatId);
                    if (!seat) return state;
                    return {
                        selectedSeats: state.selectedSeats.filter((s) => s.id !== seatId),
                        totalAmount: state.totalAmount - seat.price,
                        finalAmount: state.totalAmount - seat.price - state.discountAmount,
                    };
                }),

            toggleSeat: (seat) => {
                const { selectedSeats, addSeat, removeSeat } = get();
                const exists = selectedSeats.find((s) => s.id === seat.id);
                if (exists) {
                    removeSeat(seat.id);
                } else {
                    addSeat(seat);
                }
            },

            clearSelectedSeats: () =>
                set({
                    selectedSeats: [],
                    totalAmount: 0,
                    discountAmount: 0,
                    finalAmount: 0,
                }),

            validateSeats: () => {
                const state = get();
                const errors: string[] = [];

                if (!state.seatingPlan) {
                    errors.push('Seating plan not loaded');
                    return { isValid: false, errors };
                }

                if (state.selectedSeats.length === 0) {
                    errors.push('No seats selected');
                    return { isValid: false, errors };
                }

                // Validate each selected seat
                for (const selectedSeat of state.selectedSeats) {
                    const seatInPlan = state.seatingPlan.seats?.find((s) => s.id === selectedSeat.id);

                    if (!seatInPlan) {
                        errors.push(`Seat ${selectedSeat.seatNumber} not found in seating plan`);
                        continue;
                    }

                    if (seatInPlan.price !== selectedSeat.price) {
                        errors.push(`Price mismatch for seat ${selectedSeat.seatNumber}: expected ${seatInPlan.price}, got ${selectedSeat.price}`);
                    }

                    if (seatInPlan.status !== 'Available') {
                        errors.push(`Seat ${selectedSeat.seatNumber} is not available (status: ${seatInPlan.status})`);
                    }
                }

                return { isValid: errors.length === 0, errors };
            },

            updatePricing: (total, discount = 0) =>
                set({
                    totalAmount: total,
                    discountAmount: discount,
                    finalAmount: total - discount,
                }),

            setExpiresAt: (time) => set({ expiresAt: time }),

            clearExpiresAt: () => set({ expiresAt: null }),

            setBookingResult: (id, code) =>
                set({ bookingId: id, bookingCode: code }),

            clearBookingResult: () =>
                set({ bookingId: null, bookingCode: null }),

            resetBooking: () => set(initialState),
        }),
        {
            name: 'booking-storage',
            partialize: (state) => ({
                // Only persist these for potential recovery
                movieId: state.movieId,
                movieTitle: state.movieTitle,
                selectedSeats: state.selectedSeats,
            }),
        }
    )
);

export default useBookingStore;
