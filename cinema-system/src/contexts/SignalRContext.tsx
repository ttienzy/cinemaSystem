// src/context/SignalRContext.tsx
import { createContext, useContext, useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import { HubConnectionBuilder, LogLevel, HubConnection } from '@microsoft/signalr';
import type { SeatReserved, SeatsSelectedResponse } from '../types/showtime.type';
import { updateSeatStatus } from '../store/slices/showtimeSlice';
import { useAppDispatch } from '../hooks/redux';

interface SignalRContextType {
    connection: HubConnection | null;
    isConnected: boolean;
    connectionStatus: string;
    isCompleted: boolean;
    setIsCompleted: React.Dispatch<React.SetStateAction<boolean>>;
    //---------------
    joinShowtimeGroup: (stId: string) => void;
    leaveShowtimeGroup: (stId: string) => void;
    seatReserved: (seatReserved: SeatReserved) => void;
}

const SignalRContext = createContext<SignalRContextType | undefined>(undefined);

export const SignalRProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [connection, setConnection] = useState<HubConnection | null>(null);
    const [isConnected, setIsConnected] = useState<boolean>(false);
    const [connectionStatus, setConnectionStatus] = useState<string>('Disconnected');
    const [isCompleted, setIsCompleted] = useState<boolean>(false);

    const dispatch = useAppDispatch();

    // Khởi tạo connection
    useEffect(() => {
        const newConnection = new HubConnectionBuilder()
            .withUrl(import.meta.env.SIGNALR_HUB_URL || `https://localhost:7227/seatHub`, {
                // Cách 1: Sử dụng accessTokenFactory (Recommended)
                accessTokenFactory: () => {
                    const token = localStorage.getItem('accessToken');
                    console.log('Token being sent:', token); // Debug log
                    return token || '';
                },

                // Cách 2: Hoặc sử dụng headers (Alternative)
                // headers: {
                //     "Authorization": `Bearer ${localStorage.getItem('accessToken') || ''}`
                // }
            })
            .configureLogging(LogLevel.Information)
            .withAutomaticReconnect([0, 2000, 10000, 30000])
            .build();

        setConnection(newConnection);
    }, []);

    // Kết nối và lắng nghe events
    useEffect(() => {
        if (connection) {
            setConnectionStatus('Connecting...');

            connection.start()
                .then(() => {
                    console.log('SignalR Connected!');
                    setIsConnected(true);
                    setConnectionStatus('Connected');

                    // Lắng nghe events từ server
                    connection.on('ConnectToShowtime', (response: SeatsSelectedResponse) => {
                        console.log(response);
                    })

                    connection.on('DisconnectFromShowtime', (notification: string) => {
                        console.log(notification);
                    })

                    connection.on("OnSeatsReserved", (reservedSeatIds: string[]) => {
                        console.log("Received reservation for seats: ", reservedSeatIds);
                        dispatch(updateSeatStatus({ seatIds: reservedSeatIds, status: 'Reserved' }));
                    });

                    connection.on("OnSeatsReleased", (releasedSeatIds: string[]) => {
                        console.log("Seats released: ", releasedSeatIds);
                        dispatch(updateSeatStatus({ seatIds: releasedSeatIds, status: 'Available' }));
                    });

                    connection.on("OnSeatsBooked", (soldSeatIds: string[]) => {
                        console.log("Seats booked: ", soldSeatIds);
                        dispatch(updateSeatStatus({ seatIds: soldSeatIds, status: 'Booked' }));
                    });

                    connection.on("OnPaymentSuccessful", (soldSeatIds: string[]) => {
                        setIsCompleted(true);
                        console.log("Seats booked: ", soldSeatIds);
                        dispatch(updateSeatStatus({ seatIds: soldSeatIds, status: 'Booked' }));
                    })

                    connection.on("OnPaymentCanceled", (soldSeatIds: string[]) => {
                        console.log("Seats canceled: ", soldSeatIds);
                        dispatch(updateSeatStatus({ seatIds: soldSeatIds, status: 'Available' }));
                    })

                    // Handle reconnect/close
                    connection.onreconnecting(() => {
                        setIsConnected(false);
                        setConnectionStatus('Reconnecting...');
                    });

                    connection.onreconnected(() => {
                        setIsConnected(true);
                        setConnectionStatus('Connected');
                    });

                    connection.onclose(() => {
                        setIsConnected(false);
                        setConnectionStatus('Disconnected');
                    });
                })
                .catch((e) => {
                    console.error('Connection failed: ', e);
                    setConnectionStatus('Connection Failed');
                });

            // Cleanup khi unmount
            return () => {
                if (connection) {
                    connection.stop();
                }
            };
        }
    }, [connection, dispatch]);

    // Các functions invoke Hub methods
    const joinShowtimeGroup = (showtimeId: string) => {
        if (connection && isConnected) {
            connection.invoke('JoinShowtimeGroup', showtimeId).catch((e) => console.error('Join group error:', e));
        }
    }

    const leaveShowtimeGroup = (showtimeId: string) => {
        if (connection && isConnected) {
            connection.invoke('LeaveShowtimeGroup', showtimeId).catch((e) => console.error('Leave group error:', e));
        }
    }

    const seatReserved = (seatReserved: SeatReserved) => {
        if (connection && isConnected) {
            connection.invoke('SeatsReserved', seatReserved.showtimeId, seatReserved.seatIds)
                .catch((e) => console.error('Seat reservation error:', e));
        }
    }

    return (
        <SignalRContext.Provider
            value={{
                connection,
                isConnected,
                connectionStatus,
                isCompleted,
                setIsCompleted,
                joinShowtimeGroup,
                leaveShowtimeGroup,
                seatReserved
            }}
        >
            {children}
        </SignalRContext.Provider>
    );
};

export const useSignalR = (): SignalRContextType => {
    const context = useContext(SignalRContext);
    if (!context) {
        throw new Error('useSignalR must be used within a SignalRProvider');
    }
    return context;
};