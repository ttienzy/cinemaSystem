import * as signalR from '@microsoft/signalr';

/**
 * Singleton SignalR Connection Manager
 * Ensures only ONE connection instance exists across the entire application
 * Handles React StrictMode double-mounting gracefully
 */
class SignalRConnectionManager {
    private static instance: SignalRConnectionManager;
    private connections: Map<string, signalR.HubConnection> = new Map();
    private connectionPromises: Map<string, Promise<void>> = new Map();

    private constructor() {
        // Private constructor for singleton
    }

    public static getInstance(): SignalRConnectionManager {
        if (!SignalRConnectionManager.instance) {
            SignalRConnectionManager.instance = new SignalRConnectionManager();
        }
        return SignalRConnectionManager.instance;
    }

    /**
     * Get or create a connection for a specific hub
     * @param hubUrl - Full URL to the hub endpoint
     * @param getAccessToken - Function to get access token
     * @returns Existing or new HubConnection
     */
    public getConnection(
        hubUrl: string,
        getAccessToken: () => string | null
    ): signalR.HubConnection {
        // Check if connection already exists
        if (this.connections.has(hubUrl)) {
            const existingConnection = this.connections.get(hubUrl)!;
            console.log('[SignalRManager] ♻️ Reusing existing connection for:', hubUrl);
            return existingConnection;
        }

        console.log('[SignalRManager] 🆕 Creating NEW connection for:', hubUrl);

        // Create new connection
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => {
                    const token = getAccessToken();
                    if (!token) {
                        console.warn('[SignalRManager] ⚠️ No access token available');
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

        // Setup connection lifecycle handlers
        connection.onreconnecting((error) => {
            console.warn('[SignalRManager] 🔄 Connection lost. Reconnecting...', error);
        });

        connection.onreconnected((connectionId) => {
            console.log('[SignalRManager] ✅ Reconnected successfully. Connection ID:', connectionId);
        });

        connection.onclose((error) => {
            console.error('[SignalRManager] ❌ Connection closed', error);
            // Remove from cache when closed
            this.connections.delete(hubUrl);
            this.connectionPromises.delete(hubUrl);
        });

        // Store connection
        this.connections.set(hubUrl, connection);

        return connection;
    }

    /**
     * Start a connection (idempotent - safe to call multiple times)
     * @param hubUrl - Hub URL
     * @returns Promise that resolves when connection is established
     */
    public async startConnection(hubUrl: string): Promise<void> {
        const connection = this.connections.get(hubUrl);
        if (!connection) {
            throw new Error(`Connection not found for ${hubUrl}`);
        }

        // If already connected or connecting, return existing promise
        if (
            connection.state === signalR.HubConnectionState.Connected ||
            connection.state === signalR.HubConnectionState.Connecting
        ) {
            console.log('[SignalRManager] ⏭️ Connection already active, skipping start');
            return this.connectionPromises.get(hubUrl) || Promise.resolve();
        }

        // If already have a pending start promise, return it
        if (this.connectionPromises.has(hubUrl)) {
            console.log('[SignalRManager] ⏳ Start already in progress, waiting...');
            return this.connectionPromises.get(hubUrl)!;
        }

        // Start new connection
        console.log('[SignalRManager] 🚀 Starting connection...');
        const startPromise = connection.start().then(() => {
            console.log('[SignalRManager] ✅ Connection started successfully');
            this.connectionPromises.delete(hubUrl);
        }).catch((error) => {
            console.error('[SignalRManager] ❌ Failed to start connection:', error);
            this.connectionPromises.delete(hubUrl);
            throw error;
        });

        this.connectionPromises.set(hubUrl, startPromise);
        return startPromise;
    }

    /**
     * Stop a connection
     * @param hubUrl - Hub URL
     */
    public async stopConnection(hubUrl: string): Promise<void> {
        const connection = this.connections.get(hubUrl);
        if (!connection) {
            return;
        }

        try {
            await connection.stop();
            console.log('[SignalRManager] 🛑 Connection stopped');
        } catch (error) {
            console.error('[SignalRManager] ❌ Error stopping connection:', error);
        } finally {
            this.connections.delete(hubUrl);
            this.connectionPromises.delete(hubUrl);
        }
    }

    /**
     * Get connection state
     */
    public getConnectionState(hubUrl: string): signalR.HubConnectionState | null {
        const connection = this.connections.get(hubUrl);
        return connection?.state ?? null;
    }

    /**
     * Check if connection exists and is connected
     */
    public isConnected(hubUrl: string): boolean {
        const connection = this.connections.get(hubUrl);
        return connection?.state === signalR.HubConnectionState.Connected;
    }
}

// Export singleton instance
export const signalRManager = SignalRConnectionManager.getInstance();
