import { useEffect, useRef, useState, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import {
    SeatSignalRService,
    type SeatStatusChangedNotification,
    type ViewerCountNotification,
} from '../services/signalr/seatSignalRService';

interface UseSeatRealtimeOptions {
    showtimeId: string;
    apiBaseUrl: string;
    getAccessToken: () => string | null;
    onSeatLocked?: (notification: SeatStatusChangedNotification) => void;
    onSeatUnlocked?: (notification: SeatStatusChangedNotification) => void;
    onSeatBooked?: (notification: SeatStatusChangedNotification) => void;
    onSeatReleased?: (notification: SeatStatusChangedNotification) => void;
    onViewerCountUpdated?: (notification: ViewerCountNotification) => void;
    onConnectionError?: (error: Error) => void;
    enabled?: boolean; // Allow disabling the hook
}

interface UseSeatRealtimeReturn {
    isConnected: boolean;
    isConnecting: boolean;
    viewerCount: number;
    error: Error | null;
    reconnect: () => Promise<void>;
}

/**
 * React hook for real-time seat availability updates via SignalR
 * 
 * @example
 * ```tsx
 * const { isConnected, viewerCount } = useSeatRealtime({
 *   showtimeId: 'abc-123',
 *   apiBaseUrl: 'https://api.example.com',
 *   getAccessToken: () => localStorage.getItem('token'),
 *   onSeatLocked: (notification) => {
 *     console.log('Seat locked:', notification.seatIds);
 *     updateSeatStatus(notification.seatIds, 'locked');
 *   },
 *   onSeatUnlocked: (notification) => {
 *     updateSeatStatus(notification.seatIds, 'available');
 *   },
 * });
 * ```
 */
export function useSeatRealtime(options: UseSeatRealtimeOptions): UseSeatRealtimeReturn {
    const {
        showtimeId,
        apiBaseUrl,
        getAccessToken,
        onSeatLocked,
        onSeatUnlocked,
        onSeatBooked,
        onSeatReleased,
        onViewerCountUpdated,
        onConnectionError,
        enabled = true,
    } = options;

    const [isConnected, setIsConnected] = useState(false);
    const [isConnecting, setIsConnecting] = useState(false);
    const [viewerCount, setViewerCount] = useState(0);
    const [error, setError] = useState<Error | null>(null);

    const serviceRef = useRef<SeatSignalRService | null>(null);
    const isMountedRef = useRef(true);

    // Initialize service
    useEffect(() => {
        if (!enabled) return;

        serviceRef.current = new SeatSignalRService(apiBaseUrl, getAccessToken);

        return () => {
            isMountedRef.current = false;
        };
    }, [apiBaseUrl, getAccessToken, enabled]);

    // Setup event handlers
    useEffect(() => {
        if (!enabled || !serviceRef.current) return;

        const service = serviceRef.current;
        const unsubscribeConnectionState = service.onConnectionStateChanged((state, stateError) => {
            if (!isMountedRef.current) {
                return;
            }

            setIsConnected(state === signalR.HubConnectionState.Connected);
            setIsConnecting(
                state === signalR.HubConnectionState.Connecting ||
                state === signalR.HubConnectionState.Reconnecting
            );

            if (stateError) {
                setError(stateError);
            } else if (state === signalR.HubConnectionState.Connected) {
                setError(null);
            }
        });

        // Seat locked handler
        if (onSeatLocked) {
            service.onSeatLocked((notification) => {
                if (isMountedRef.current) {
                    console.log('[useSeatRealtime] Seat locked:', notification);
                    onSeatLocked(notification);
                }
            });
        }

        // Seat unlocked handler
        if (onSeatUnlocked) {
            service.onSeatUnlocked((notification) => {
                if (isMountedRef.current) {
                    console.log('[useSeatRealtime] Seat unlocked:', notification);
                    onSeatUnlocked(notification);
                }
            });
        }

        // Seat booked handler
        if (onSeatBooked) {
            service.onSeatBooked((notification) => {
                if (isMountedRef.current) {
                    console.log('[useSeatRealtime] Seat booked:', notification);
                    onSeatBooked(notification);
                }
            });
        }

        // Seat released handler
        if (onSeatReleased) {
            service.onSeatReleased((notification) => {
                if (isMountedRef.current) {
                    console.log('[useSeatRealtime] Seat released:', notification);
                    onSeatReleased(notification);
                }
            });
        }

        // Viewer count handler
        service.onViewerCountUpdated((notification) => {
            if (isMountedRef.current && notification.showtimeId === showtimeId) {
                console.log('[useSeatRealtime] Viewer count updated:', notification.viewerCount);
                setViewerCount(notification.viewerCount);
                onViewerCountUpdated?.(notification);
            }
        });

        return () => {
            unsubscribeConnectionState();
            service.offAll();
        };
    }, [
        enabled,
        showtimeId,
        onSeatLocked,
        onSeatUnlocked,
        onSeatBooked,
        onSeatReleased,
        onViewerCountUpdated,
    ]);

    // Connect and join showtime
    useEffect(() => {
        if (!enabled || !serviceRef.current || !showtimeId) return;

        let isSubscribed = true;
        const service = serviceRef.current;

        const connectAndJoin = async () => {
            try {
                setIsConnecting(true);
                setError(null);

                // Start connection
                await service.start();

                if (!isSubscribed) return;

                // Join showtime group
                await service.joinShowtime(showtimeId);

                console.log(`[useSeatRealtime] Successfully joined showtime ${showtimeId}`);
            } catch (err) {
                const error = err instanceof Error ? err : new Error('Unknown error');
                console.error('[useSeatRealtime] Connection error:', error);

                if (isSubscribed) {
                    setError(error);
                    setIsConnected(false);
                    setIsConnecting(false);
                    onConnectionError?.(error);
                }
            } finally {
                if (isSubscribed) {
                    setIsConnecting(false);
                }
            }
        };

        connectAndJoin();

        // Cleanup
        return () => {
            isSubscribed = false;

            const cleanup = async () => {
                try {
                    if (service.isConnected()) {
                        await service.leaveShowtime(showtimeId);
                        await service.stop();
                    }
                } catch (err) {
                    console.error('[useSeatRealtime] Cleanup error:', err);
                }
            };

            cleanup();
        };
    }, [enabled, showtimeId, onConnectionError]);

    // Reconnect function
    const reconnect = useCallback(async () => {
        if (!serviceRef.current) return;

        try {
            setError(null);

            await serviceRef.current.stop();
            await serviceRef.current.start();
            await serviceRef.current.joinShowtime(showtimeId);

            setIsConnected(true);
            console.log('[useSeatRealtime] Reconnected successfully');
        } catch (err) {
            const error = err instanceof Error ? err : new Error('Reconnection failed');
            setError(error);
            setIsConnected(false);
            console.error('[useSeatRealtime] Reconnection error:', error);
        } finally {
            setIsConnecting(false);
        }
    }, [showtimeId]);

    return {
        isConnected,
        isConnecting,
        viewerCount,
        error,
        reconnect,
    };
}
