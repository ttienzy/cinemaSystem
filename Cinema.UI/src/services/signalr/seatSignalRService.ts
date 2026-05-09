import * as signalR from '@microsoft/signalr';

/**
 * Notification payload for seat status changes
 */
export interface SeatStatusChangedNotification {
    showtimeId: string;
    seatIds: string[];
    status: 'locked' | 'available' | 'booked';
    userId?: string;
    lockedUntil?: string;
    timestamp: string;
}

/**
 * Notification payload for viewer count updates
 */
export interface ViewerCountNotification {
    showtimeId: string;
    viewerCount: number;
    timestamp: string;
}

/**
 * SignalR service for real-time seat availability updates
 * Manages WebSocket connection to Booking.API SignalR hub
 */
export class SeatSignalRService {
    private connection: signalR.HubConnection | null = null;
    private readonly hubUrl: string;
    private readonly getAccessToken: () => string | null;
    private currentShowtimeId: string | null = null;
    private readonly connectionStateListeners = new Set<
        (state: signalR.HubConnectionState, error?: Error) => void
    >();

    constructor(apiBaseUrl: string, getAccessToken: () => string | null) {
        this.hubUrl = `${apiBaseUrl}/hubs/seats`;
        this.getAccessToken = getAccessToken;
    }

    /**
     * Initialize and start the SignalR connection
     */
    async start(): Promise<void> {
        if (!this.connection) {
            this.connection = this.buildConnection();
        }

        if (
            this.connection.state === signalR.HubConnectionState.Connected ||
            this.connection.state === signalR.HubConnectionState.Connecting ||
            this.connection.state === signalR.HubConnectionState.Reconnecting
        ) {
            console.log('[SignalR] Already connected');
            return;
        }

        try {
            this.notifyConnectionState(signalR.HubConnectionState.Connecting);
            await this.connection.start();
            this.notifyConnectionState(signalR.HubConnectionState.Connected);
            console.log('[SignalR] Connected successfully');
        } catch (error) {
            console.error('[SignalR] Failed to start connection', error);
            this.notifyConnectionState(
                signalR.HubConnectionState.Disconnected,
                error instanceof Error ? error : new Error('Failed to start SignalR connection')
            );
            throw error;
        }
    }

    /**
     * Stop the SignalR connection
     */
    async stop(): Promise<void> {
        if (this.connection) {
            try {
                this.currentShowtimeId = null;
                await this.connection.stop();
                console.log('[SignalR] Connection stopped');
            } catch (error) {
                console.error('[SignalR] Error stopping connection', error);
            }
        }
    }

    /**
     * Join a showtime group to receive real-time updates
     */
    async joinShowtime(showtimeId: string): Promise<void> {
        if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
            throw new Error('SignalR connection not established');
        }

        try {
            this.currentShowtimeId = showtimeId;
            await this.connection.invoke('JoinShowtime', showtimeId);
            console.log(`[SignalR] Joined showtime ${showtimeId}`);
        } catch (error) {
            console.error(`[SignalR] Failed to join showtime ${showtimeId}`, error);
            throw error;
        }
    }

    /**
     * Leave a showtime group
     */
    async leaveShowtime(showtimeId: string): Promise<void> {
        if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
            return; // Already disconnected
        }

        try {
            await this.connection.invoke('LeaveShowtime', showtimeId);
            if (this.currentShowtimeId === showtimeId) {
                this.currentShowtimeId = null;
            }
            console.log(`[SignalR] Left showtime ${showtimeId}`);
        } catch (error) {
            console.error(`[SignalR] Failed to leave showtime ${showtimeId}`, error);
        }
    }

    onConnectionStateChanged(
        callback: (state: signalR.HubConnectionState, error?: Error) => void
    ): () => void {
        this.connectionStateListeners.add(callback);
        return () => {
            this.connectionStateListeners.delete(callback);
        };
    }

    /**
     * Subscribe to seat locked events
     */
    onSeatLocked(callback: (notification: SeatStatusChangedNotification) => void): void {
        if (!this.connection) return;
        this.connection.on('SeatLocked', callback);
    }

    /**
     * Subscribe to seat unlocked events
     */
    onSeatUnlocked(callback: (notification: SeatStatusChangedNotification) => void): void {
        if (!this.connection) return;
        this.connection.on('SeatUnlocked', callback);
    }

    /**
     * Subscribe to seat booked events
     */
    onSeatBooked(callback: (notification: SeatStatusChangedNotification) => void): void {
        if (!this.connection) return;
        this.connection.on('SeatBooked', callback);
    }

    /**
     * Subscribe to seat released events
     */
    onSeatReleased(callback: (notification: SeatStatusChangedNotification) => void): void {
        if (!this.connection) return;
        this.connection.on('SeatReleased', callback);
    }

    /**
     * Subscribe to viewer count updates
     */
    onViewerCountUpdated(callback: (notification: ViewerCountNotification) => void): void {
        if (!this.connection) return;
        this.connection.on('ViewerCountUpdated', callback);
    }

    /**
     * Unsubscribe from all events (cleanup)
     */
    offAll(): void {
        if (!this.connection) return;
        this.connection.off('SeatLocked');
        this.connection.off('SeatUnlocked');
        this.connection.off('SeatBooked');
        this.connection.off('SeatReleased');
        this.connection.off('ViewerCountUpdated');
    }

    /**
     * Get current connection state
     */
    getConnectionState(): signalR.HubConnectionState | null {
        return this.connection?.state ?? null;
    }

    /**
     * Check if connected
     */
    isConnected(): boolean {
        return this.connection?.state === signalR.HubConnectionState.Connected;
    }

    private buildConnection(): signalR.HubConnection {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(this.hubUrl, {
                accessTokenFactory: () => {
                    const token = this.getAccessToken();
                    if (!token) {
                        console.warn('[SignalR] No access token available');
                        return '';
                    }
                    return token;
                },
                withCredentials: false,
            })
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: (retryContext) => {
                    if (retryContext.previousRetryCount === 0) return 0;
                    if (retryContext.previousRetryCount === 1) return 2000;
                    if (retryContext.previousRetryCount === 2) return 10000;
                    if (retryContext.previousRetryCount === 3) return 30000;
                    return 60000;
                },
            })
            .configureLogging(
                import.meta.env.DEV ? signalR.LogLevel.Information : signalR.LogLevel.Warning
            )
            .build();

        connection.onreconnecting((error) => {
            console.warn('[SignalR] Connection lost. Reconnecting...', error);
            this.notifyConnectionState(
                signalR.HubConnectionState.Reconnecting,
                error instanceof Error ? error : undefined
            );
        });

        connection.onreconnected(async (connectionId) => {
            console.log('[SignalR] Reconnected successfully', connectionId);

            if (this.currentShowtimeId) {
                try {
                    await connection.invoke('JoinShowtime', this.currentShowtimeId);
                    console.log(`[SignalR] Rejoined showtime ${this.currentShowtimeId}`);
                } catch (error) {
                    console.error(
                        `[SignalR] Failed to rejoin showtime ${this.currentShowtimeId}`,
                        error
                    );
                    this.notifyConnectionState(
                        signalR.HubConnectionState.Disconnected,
                        error instanceof Error ? error : new Error('Failed to rejoin showtime')
                    );
                    return;
                }
            }

            this.notifyConnectionState(signalR.HubConnectionState.Connected);
        });

        connection.onclose((error) => {
            console.error('[SignalR] Connection closed', error);
            this.notifyConnectionState(
                signalR.HubConnectionState.Disconnected,
                error instanceof Error ? error : undefined
            );
        });

        return connection;
    }

    private notifyConnectionState(
        state: signalR.HubConnectionState,
        error?: Error
    ): void {
        this.connectionStateListeners.forEach((listener) => listener(state, error));
    }
}
