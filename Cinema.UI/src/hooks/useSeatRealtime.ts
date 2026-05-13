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
    enabled?: boolean;
}

interface UseSeatRealtimeReturn {
    isConnected: boolean;
    isConnecting: boolean;
    viewerCount: number;
    error: Error | null;
    reconnect: () => Promise<void>;
}

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
    const listenersRegisteredRef = useRef(false);

    // Initialize service ONCE
    useEffect(() => {
        if (!enabled) return;

        if (!serviceRef.current) {
            console.log('[useSeatRealtime] 🆕 Creating new service instance');
            serviceRef.current = new SeatSignalRService(apiBaseUrl, getAccessToken);
        }

        return () => {
            isMountedRef.current = false;
        };
    }, [apiBaseUrl, getAccessToken, enabled]);

    // Setup event handlers ONCE - never cleanup to avoid removing listeners
    useEffect(() => {
        if (!enabled || !serviceRef.current || listenersRegisteredRef.current) return;

        const service = serviceRef.current;
        console.log('[useSeatRealtime] 🔧 Registering event listeners (ONE TIME)');

        const unsubscribeConnectionState = service.onConnectionStateChanged((state, stateError) => {
            if (!isMountedRef.current) return;

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
                console.log('[useSeatRealtime] 🔒 Seat locked callback triggered');
                onSeatLocked(notification);
            });
        }

        // Seat unlocked handler
        if (onSeatUnlocked) {
            service.onSeatUnlocked((notification) => {
                console.log('[useSeatRealtime] 🔓 Seat unlocked callback triggered');
                onSeatUnlocked(notification);
            });
        }

        // Seat booked handler
        if (onSeatBooked) {
            service.onSeatBooked((notification) => {
                console.log('[useSeatRealtime] ✅ Seat booked callback triggered');
                onSeatBooked(notification);
            });
        }

        // Seat released handler
        if (onSeatReleased) {
            service.onSeatReleased((notification) => {
                console.log('[useSeatRealtime] 🔄 Seat released callback triggered');
                onSeatReleased(notification);
            });
        }

        // Viewer count handler
        service.onViewerCountUpdated((notification) => {
            console.log('[useSeatRealtime] 👥 Viewer count callback FIRED!');
            console.log('[useSeatRealtime] 📦 Notification:', JSON.stringify(notification, null, 2));
            console.log('[useSeatRealtime] 🎯 ViewerCount:', notification.ViewerCount);

            // Update state
            setViewerCount(notification.ViewerCount);

            if (notification.ShowtimeId === showtimeId) {
                console.log('[useSeatRealtime] ✅ Showtime ID matches!');
                onViewerCountUpdated?.(notification);
            }
        });

        listenersRegisteredRef.current = true;
        console.log('[useSeatRealtime] ✅ Event listeners registered successfully');

        // DON'T cleanup listeners - they're shared across all instances
        return () => {
            unsubscribeConnectionState();
        };
    }, [enabled]); // Only depend on enabled, not callbacks

    // Connect and join showtime
    useEffect(() => {
        if (!enabled || !serviceRef.current || !showtimeId) return;

        let isSubscribed = true;
        const service = serviceRef.current;

        const connectAndJoin = async () => {
            try {
                setIsConnecting(true);
                setError(null);

                console.log('[useSeatRealtime] 🚀 Starting connection...');
                await service.start();

                if (!isSubscribed) return;

                console.log('[useSeatRealtime] 🎯 Joining showtime...');
                await service.joinShowtime(showtimeId);

                console.log(`[useSeatRealtime] ✅ Successfully joined showtime ${showtimeId}`);
            } catch (err) {
                const error = err instanceof Error ? err : new Error('Unknown error');
                console.error('[useSeatRealtime] ❌ Connection error:', error);

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

        return () => {
            isSubscribed = false;

            const cleanup = async () => {
                try {
                    if (service.isConnected()) {
                        await service.leaveShowtime(showtimeId);
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
