import { create } from 'zustand';
import type { SeatStatusDto } from './bookingApi';

interface BookingSession {
  showtimeId: string | null;
  selectedSeats: SeatStatusDto[];
  lockedUntil: string | null;
}

interface BookingState extends BookingSession {
  setBookingSession: (showtimeId: string, selectedSeats: SeatStatusDto[], lockedUntil: string) => void;
  clearBookingSession: () => void;
  hydrateBookingSession: () => void;
}

const storageKey = 'cinema.web.booking-session';

function loadSession(): BookingSession {
  try {
    const raw = sessionStorage.getItem(storageKey);
    if (!raw) return { showtimeId: null, selectedSeats: [], lockedUntil: null };
    return JSON.parse(raw) as BookingSession;
  } catch {
    return { showtimeId: null, selectedSeats: [], lockedUntil: null };
  }
}

function saveSession(session: BookingSession): void {
  sessionStorage.setItem(storageKey, JSON.stringify(session));
}

function clearSession(): void {
  sessionStorage.removeItem(storageKey);
}

export const useBookingStore = create<BookingState>((set) => ({
  ...loadSession(),
  setBookingSession: (showtimeId, selectedSeats, lockedUntil) => {
    const session = { showtimeId, selectedSeats, lockedUntil };
    saveSession(session);
    set(session);
  },
  clearBookingSession: () => {
    clearSession();
    set({ showtimeId: null, selectedSeats: [], lockedUntil: null });
  },
  hydrateBookingSession: () => set(loadSession()),
}));
