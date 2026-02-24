import React, { useState, useEffect, useMemo, useCallback } from 'react';
import type { Seat, SeatsSelectedResponse, SelectedSeat } from '../../../types/showtime.type';
import TabNavigation from '../../../components/concession/TabNavigation';
import DateSelector from '../../../components/concession/DateSelector';
import MovieCard from '../../../components/concession/MovieCard';
import ConcessionGrid from '../../../components/concession/ConcessionGrid';
import StandaloneConcessionGrid from '../../../components/concession/StandaloneConcessionGrid';
import Cart from '../../../components/concession/Cart';
import SeatSelection from '../../../components/concession/SeatSelection';
import type { CartItem } from '../../../types/dashboard.types';
import type { InventoryItem } from '../../../types/inventory.types';
import { useAppDispatch, useAppSelector } from '../../../hooks/redux';
import { confirmConcessionPurchase, getInventory } from '../../../store/slices/inventorySlice';
import { getMoviesListFeature } from '../../../store/slices/movieSlice';
import { Coffee, Film } from 'lucide-react';
import { getShowtimeSeatingPlan } from '../../../store/slices/showtimeSlice';
import { useSignalR } from '../../../contexts/SignalRContext';

// Utility function để format date theo local timezone (VN)
const getLocalDateString = (date: Date): string => {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
};

// Utility function để tạo Date array
const generateDateRange = (startDate: Date, days: number): Date[] => {
    return Array.from({ length: days }, (_, i) => {
        const date = new Date(startDate);
        date.setDate(startDate.getDate() + i);
        return date;
    });
};

const EmployeeNewSale: React.FC = () => {
    const dispatch = useAppDispatch();
    const { connection, isConnected } = useSignalR();

    // Redux selectors
    const { moviesListFeature } = useAppSelector(state => state.movie);
    const { items } = useAppSelector(state => state.inventory);
    const { showtimeInfo, pricings, seats } = useAppSelector(state => state.showtime);

    // Memoized values
    const cinemaId = useMemo(() => localStorage.getItem('cinemaId'), []);
    const staffId = useMemo(() => localStorage.getItem('staffId'), []);

    const today = useMemo(() => new Date(), []);
    const dates = useMemo(() => generateDateRange(today, 7), [today]);

    const initialCart = useMemo((): CartItem => ({
        staffId: staffId || '00000000-0000-0000-0000-000000000000',
        concessions: [],
        paymentMethod: 'cash',
        tickets: {
            showtimeId: '00000000-0000-0000-0000-000000000000',
            actualStartTime: '',
            actualEndTime: '',
            movieTitle: '',
            selectedSeats: []
        }
    }), [staffId]);

    // State
    const [activeTab, setActiveTab] = useState<'combo' | 'concession'>('combo');
    const [selectedDate, setSelectedDate] = useState<string>(() => getLocalDateString(today));
    const [cart, setCart] = useState<CartItem>(initialCart);
    const [showSeatSelection, setShowSeatSelection] = useState(false);
    const [selectedSeats, setSelectedSeats] = useState<SelectedSeat[]>([]);
    const [paymentMethod, setPaymentMethod] = useState<'cash' | 'card'>('cash');

    // Validation check
    if (!cinemaId || !staffId) {
        return <div className="p-6">No cinema assigned. Please contact admin.</div>;
    }

    // Initial data fetch
    useEffect(() => {
        dispatch(getInventory(cinemaId));
        dispatch(getMoviesListFeature({ cinemaId, showDate: selectedDate }));
    }, [dispatch, cinemaId, selectedDate]);

    // Reset cart when tab changes
    useEffect(() => {
        setCart(initialCart);
    }, [activeTab, initialCart]);

    // Date change handler
    const handleDateChange = useCallback((date: string) => {
        if (date === selectedDate) return;
        setSelectedDate(date);
        dispatch(getMoviesListFeature({ cinemaId, showDate: date }));
    }, [selectedDate, dispatch, cinemaId]);

    // Seat selection handlers
    const fetchSeatData = useCallback(async (showtimeId: string) => {
        if (isConnected && connection) {
            try {
                await connection.invoke('JoinShowtimeGroup', showtimeId);
            } catch (error) {
                console.error('Failed to join group:', error);
            }
        }
        await dispatch(getShowtimeSeatingPlan(showtimeId));
        setShowSeatSelection(true);
    }, [isConnected, connection, dispatch]);

    const handleSelectSeat = useCallback((seat: Seat) => {
        if (seat.status !== 'Available') return;

        const pricing = pricings.find(p => p.seatTypeId === seat.seatTypeId);
        if (!pricing) return;

        setSelectedSeats(prev => {
            const existingIndex = prev.findIndex(s => s.seatId === seat.id);
            if (existingIndex >= 0) {
                return prev.filter(s => s.seatId !== seat.id);
            }
            return [...prev, { seatId: seat.id, price: pricing.finalPrice }];
        });
    }, [pricings]);

    const addTicketToCart = useCallback(async () => {
        if (selectedSeats.length === 0) return;

        setCart(prev => ({
            ...prev,
            tickets: {
                showtimeId: showtimeInfo.id,
                actualStartTime: showtimeInfo.actualStartTime,
                actualEndTime: showtimeInfo.actualEndTime,
                movieTitle: showtimeInfo.movieTitle,
                selectedSeats
            }
        }));

        if (isConnected && connection) {
            try {
                await connection.invoke("SeatsReserved", showtimeInfo.id, selectedSeats.map(s => s.seatId));
            } catch (err) {
                console.error('Seat reservation error:', err);
            }
        }

        setShowSeatSelection(false);
        setSelectedSeats([]);
    }, [selectedSeats, showtimeInfo, isConnected, connection]);

    // Concession handlers
    const addConcessionToCart = useCallback((item: InventoryItem) => {
        setCart(prev => {
            const existingItem = prev.concessions.find(cartItem => cartItem.itemId === item.id);

            if (existingItem) {
                if (existingItem.quantity >= item.currentStock) return prev;

                return {
                    ...prev,
                    concessions: prev.concessions.map(cartItem =>
                        cartItem.itemId === item.id
                            ? { ...cartItem, quantity: cartItem.quantity + 1 }
                            : cartItem
                    )
                };
            }

            return {
                ...prev,
                concessions: [
                    ...prev.concessions,
                    { itemId: item.id, itemName: item.itemName, price: item.unitPrice, quantity: 1 }
                ]
            };
        });
    }, []);

    const handleUpdateItem = useCallback((itemId: string, action: 'increase' | 'decrease' | 'remove') => {
        setCart(prev => ({
            ...prev,
            concessions: prev.concessions
                .map(item => {
                    if (item.itemId !== itemId) return item;

                    let newQuantity = item.quantity;
                    switch (action) {
                        case 'increase':
                            newQuantity = item.quantity + 1;
                            break;
                        case 'decrease':
                            newQuantity = Math.max(1, item.quantity - 1);
                            break;
                        case 'remove':
                            newQuantity = 0;
                            break;
                    }
                    return { ...item, quantity: newQuantity };
                })
                .filter(item => item.quantity > 0)
        }));
    }, []);

    const handleRemoveTicket = useCallback(async () => {
        try {
            if (connection && isConnected && cart.tickets.selectedSeats.length > 0) {
                await connection.invoke('SeatsReleased', cart.tickets.showtimeId, cart.tickets.selectedSeats.map(s => s.seatId));
            }
            setCart(prev => ({ ...prev, tickets: initialCart.tickets }));
        } catch (error) {
            console.error('Remove ticket error:', error);
        }
    }, [connection, isConnected, cart.tickets, initialCart.tickets]);

    const handleClearCart = useCallback(async () => {
        if (cart.tickets.selectedSeats.length > 0) {
            await handleRemoveTicket();
        }
        setCart(initialCart);
    }, [cart.tickets.selectedSeats.length, handleRemoveTicket, initialCart]);

    // Calculation functions
    const getTotalAmount = useCallback(() => {
        const ticketsTotal = cart.tickets.selectedSeats.reduce((sum, seat) => sum + seat.price, 0);
        const concessionsTotal = cart.concessions.reduce((sum, item) => sum + (item.price * item.quantity), 0);
        return ticketsTotal + concessionsTotal;
    }, [cart]);

    // Format functions
    const formatCurrency = useCallback((amount: number) => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    }, []);

    const getDateString = useCallback((dateStr: string) => {
        const [year, month, day] = dateStr.split('-').map(Number);
        const date = new Date(year, month - 1, day);
        return `${day}/${month}/${year}`;
    }, []);

    const formatVN = useCallback((d: Date) => {
        const day = d.getDate();
        const month = d.getMonth() + 1;
        return `${day}/${month}`;
    }, []);

    const getSeatColor = useCallback((seat: Seat, isSelected: boolean) => {
        if (isSelected) return 'bg-blue-500 text-white border-blue-500';

        const statusColorMap: Record<string, string> = {
            available: seat.seatTypeName.toLowerCase() === 'vip'
                ? 'bg-purple-50 hover:bg-purple-100 text-purple-800'
                : seat.seatTypeName.toLowerCase() === 'couple'
                    ? 'bg-pink-50 hover:bg-pink-100 text-pink-800'
                    : 'bg-green-50 hover:bg-green-100 text-green-800',
            reserved: 'bg-yellow-100 text-yellow-800 cursor-not-allowed',
            booked: 'bg-red-100 text-red-800 cursor-not-allowed',
            unavailable: 'bg-gray-100 text-gray-500 cursor-not-allowed'
        };

        return statusColorMap[seat.status.toLowerCase()] || 'bg-gray-100 text-gray-500 cursor-not-allowed';
    }, []);

    // Payment handler
    const confirmPayment = useCallback(async () => {
        try {
            await dispatch(confirmConcessionPurchase({ cinemaId, cartItem: cart }));
            alert('Payment confirmed!');
            setCart(initialCart);
            setPaymentMethod('cash');
        } catch (err) {
            console.error('Payment error:', err);
        }
    }, [dispatch, cinemaId, cart, initialCart]);

    return (
        <div className="flex h-screen bg-gray-50">
            <div className="flex-1 flex flex-col">
                <TabNavigation activeTab={activeTab} onTabChange={setActiveTab} />

                <div className="flex-1 p-6">
                    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 h-full">
                        {/* Products Section */}
                        <div className="lg:col-span-2 bg-white rounded-lg shadow border border-gray-200 p-6">
                            {activeTab === 'combo' ? (
                                <div>
                                    <h3 className="text-lg font-semibold mb-4 flex items-center">
                                        <Film className="w-5 h-5 mr-2" />
                                        Lịch chiếu từ {formatVN(dates[0])} đến {formatVN(dates[6])}
                                    </h3>

                                    <DateSelector
                                        dates={dates}
                                        selectedDate={selectedDate}
                                        onDateChange={handleDateChange}
                                        formatDate={getDateString}
                                    />

                                    <div className="space-y-6 max-h-[calc(100vh-400px)] overflow-y-auto">
                                        {moviesListFeature.map(movie => (
                                            <MovieCard
                                                key={movie.postUrl}
                                                movie={movie}
                                                onShowtimeClick={fetchSeatData}
                                            />
                                        ))}
                                        {moviesListFeature.length === 0 && (
                                            <p className="text-center text-gray-500">Không có phim chiếu trong ngày này</p>
                                        )}
                                    </div>

                                    <SeatSelection
                                        seatData={showSeatSelection ? { showtimeInfo, pricings, seats } : null}
                                        selectedSeats={selectedSeats}
                                        onSelectSeat={handleSelectSeat}
                                        onConfirm={addTicketToCart}
                                        formatCurrency={formatCurrency}
                                        getSeatColor={getSeatColor}
                                    />

                                    <div className="mt-8 pt-6 border-t border-gray-200">
                                        <h4 className="font-medium mb-3 flex items-center">
                                            <Coffee className="w-4 h-4 mr-2" />
                                            Thêm đồ ăn uống:
                                        </h4>
                                        <ConcessionGrid
                                            items={items}
                                            onAdd={addConcessionToCart}
                                            formatCurrency={formatCurrency}
                                        />
                                    </div>
                                </div>
                            ) : (
                                <div>
                                    <h3 className="text-lg font-semibold mb-4 flex items-center">
                                        <Coffee className="w-5 h-5 mr-2" />
                                        Đồ ăn & Nước uống
                                    </h3>

                                    <StandaloneConcessionGrid
                                        items={items}
                                        onAdd={addConcessionToCart}
                                        formatCurrency={formatCurrency}
                                    />
                                </div>
                            )}
                        </div>

                        {/* Cart Section */}
                        <Cart
                            cart={cart}
                            totalAmount={getTotalAmount()}
                            onClear={handleClearCart}
                            onConfirmPayment={confirmPayment}
                            paymentMethod={paymentMethod}
                            onPaymentMethodChange={setPaymentMethod}
                            formatCurrency={formatCurrency}
                            onUpdateItem={handleUpdateItem}
                            onRemoveTicket={handleRemoveTicket}
                        />
                    </div>
                </div>
            </div>
        </div>
    );
};

export default EmployeeNewSale;