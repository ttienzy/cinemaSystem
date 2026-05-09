import { create } from 'zustand';
import type { SeatStatusDto } from '../api/bookingApi';

interface BookingState {
  showtimeId: string | null;
  selectedSeats: SeatStatusDto[];
  lockedUntil: string | null;
  setBookingSession: (showtimeId: string, seats: SeatStatusDto[], lockedUntil: string) => void;
  clearBookingSession: () => void;
}

export const useBookingStore = create<BookingState>((set) => ({
  showtimeId: null,
  selectedSeats: [],
  lockedUntil: null,
  setBookingSession: (showtimeId, seats, lockedUntil) => set({ showtimeId, selectedSeats: seats, lockedUntil }),
  clearBookingSession: () => set({ showtimeId: null, selectedSeats: [], lockedUntil: null })
}));
