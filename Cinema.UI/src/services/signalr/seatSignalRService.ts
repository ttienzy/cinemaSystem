import * as signalR from '@microsoft/signalr';
import { signalRManager } from './SignalRConnectionManager';

/**
 * Notification payload for seat status changes
 */
export interface SeatStatusChangedNotification {
    ShowtimeId: string;
    SeatIds: string[];
    Status: string;
    UserId?: string;
    LockedUntil?: string;
    Timestamp: string;
}

/**
 * Notification payload for viewer count updates
 */
export interface ViewerCountNotification {
    ShowtimeId: string;
    ViewerCount: number;
    Timestamp?: string;
}

/**
 * SignalR service for real-time seat availability updates
 * Uses singleton connection manager to prevent duplicate connections
 */
export class SeatSignalRService {
    private readonly hubUrl: string;
    private readonly getAccessToken: () => string | null;
    private currentShowtimeId: string | null = null;
    private readonly connectionStateListeners = new Set<
        (state: signalR.HubConnectionState, error?: Error) => void
    >();

    constructor(apiBaseUrl: string, getAccessToken: () => string | null) {
        this.hubUrl = `${apiBaseUrl}/hubs/seats`;
        this.getAccessToken = getAccessToken;
        console.log('[SeatSignalRService] 🔧 Service initialized for:', this.hubUrl);
    }

    /**
     * Get the singleton connection instance
     */
    private getConnection(): signalR.HubConnection {
        return signalRManager.getConnection(this.hubUrl, this.getAccessToken);
    }

    /**
     * Initialize and start the SignalR connection
     */
    async start(): Promise<void> {
        console.log('[SeatSignalRService] 🚀 Starting connection...');
        await signalRManager.startConnection(this.hubUrl);
        console.log('[SeatSignalRService] ✅ Connection started');
    }

    /**
     * Stop the SignalR connection
     */
    async stop(): Promise<void> {
        console.log('[SeatSignalRService] 🛑 Stopping connection...');
        await signalRManager.stopConnection(this.hubUrl);
        this.currentShowtimeId = null;
    }

    /**
     * Join a showtime group to receive real-time updates
     */
    async joinShowtime(showtimeId: string): Promise<void> {
        const connection = this.getConnection();

        if (connection.state !== signalR.HubConnectionState.Connected) {
            throw new Error('SignalR connection not established');
        }

        try {
            this.currentShowtimeId = showtimeId;
            await connection.invoke('JoinShowtime', showtimeId);
            console.log(`[SeatSignalRService] ✅ Joined showtime ${showtimeId}`);
        } catch (error) {
            console.error(`[SeatSignalRService] ❌ Failed to join showtime ${showtimeId}`, error);
            throw error;
        }
    }

    /**
     * Leave a showtime group
     */
    async leaveShowtime(showtimeId: string): Promise<void> {
        const connection = this.getConnection();

        if (connection.state !== signalR.HubConnectionState.Connected) {
            return;
        }

        try {
            await connection.invoke('LeaveShowtime', showtimeId);
            if (this.currentShowtimeId === showtimeId) {
                this.currentShowtimeId = null;
            }
            console.log(`[SeatSignalRService] ✅ Left showtime ${showtimeId}`);
        } catch (error) {
            console.error(`[SeatSignalRService] ❌ Failed to leave showtime ${showtimeId}`, error);
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
        const connection = this.getConnection();
        console.log('[SeatSignalRService] 📌 Registering handler: seatlocked (lowercase)');
        connection.on('seatlocked', (notification) => {
            console.log('[SeatSignalRService] 🔒 SeatLocked event received:', notification);
            callback(notification);
        });
    }

    /**
     * Subscribe to seat unlocked events
     */
    onSeatUnlocked(callback: (notification: SeatStatusChangedNotification) => void): void {
        const connection = this.getConnection();
        console.log('[SeatSignalRService] 📌 Registering handler: seatunlocked (lowercase)');
        connection.on('seatunlocked', (notification) => {
            console.log('[SeatSignalRService] 🔓 SeatUnlocked event received:', notification);
            callback(notification);
        });
    }

    /**
     * Subscribe to seat booked events
     */
    onSeatBooked(callback: (notification: SeatStatusChangedNotification) => void): void {
        const connection = this.getConnection();
        console.log('[SeatSignalRService] 📌 Registering handler: seatbooked (lowercase)');
        connection.on('seatbooked', (notification) => {
            console.log('[SeatSignalRService] ✅ SeatBooked event received:', notification);
            callback(notification);
        });
    }

    /**
     * Subscribe to seat released events
     */
    onSeatReleased(callback: (notification: SeatStatusChangedNotification) => void): void {
        const connection = this.getConnection();
        console.log('[SeatSignalRService] 📌 Registering handler: seatreleased (lowercase)');
        connection.on('seatreleased', (notification) => {
            console.log('[SeatSignalRService] 🔄 SeatReleased event received:', notification);
            callback(notification);
        });
    }

    /**
     * Subscribe to viewer count updates
     */
    onViewerCountUpdated(callback: (notification: ViewerCountNotification) => void): void {
        const connection = this.getConnection();
        console.log('[SeatSignalRService] 📌 Registering handler: viewercountupdated (lowercase)');
        connection.on('viewercountupdated', (notification) => {
            console.log('[SeatSignalRService] 👥 ViewerCountUpdated event received:', notification);
            callback(notification);
        });
    }

    /**
     * Unsubscribe from all events (cleanup)
     */
    offAll(): void {
        const connection = this.getConnection();
        connection.off('seatlocked');
        connection.off('seatunlocked');
        connection.off('seatbooked');
        connection.off('seatreleased');
        connection.off('viewercountupdated');
    }

    /**
     * Get current connection state
     */
    getConnectionState(): signalR.HubConnectionState | null {
        return signalRManager.getConnectionState(this.hubUrl);
    }

    /**
     * Check if connected
     */
    isConnected(): boolean {
        return signalRManager.isConnected(this.hubUrl);
    }
}
