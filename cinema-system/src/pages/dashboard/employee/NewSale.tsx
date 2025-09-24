import React, { useState, useEffect, useMemo } from 'react';
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




const EmployeeNewSale: React.FC = () => {
    const cinemaId = useMemo(() => localStorage.getItem('cinemaId'), []);
    const staffId = useMemo(() => localStorage.getItem('staffId'), []);
    if (!cinemaId || !staffId) {
        alert('No cinema assigned. Please contact admin.');
        return <div className="p-6">No cinema assigned. Please contact admin.</div>;
    }
    const [activeTab, setActiveTab] = useState<'combo' | 'concession'>('combo');
    const { connection, isConnected } = useSignalR();
    const [cart, setCart] = useState<CartItem>({ staffId: staffId || '00000000-0000-0000-0000-000000000000', concessions: [], paymentMethod: 'cash', tickets: { showtimeId: '00000000-0000-0000-0000-000000000000', actualStartTime: '', actualEndTime: '', movieTitle: '', selectedSeats: [] } });
    const [showSeatSelection, setShowSeatSelection] = useState(false);
    const [selectedSeats, setSelectedSeats] = useState<SelectedSeat[]>([]);
    const [paymentMethod, setPaymentMethod] = useState<'cash' | 'card'>('cash');

    const today = new Date();
    const endDate = new Date(today);
    endDate.setDate(today.getDate() + 6);
    const dates: Date[] = [];
    let currentDate = new Date(today);
    while (currentDate <= endDate) {
        dates.push(new Date(currentDate));
        currentDate.setDate(currentDate.getDate() + 1);
    }

    const [selectedDate, setSelectedDate] = useState<string>(today.toISOString().split('T')[0]);

    const dispatch = useAppDispatch();
    const { moviesListFeature } = useAppSelector(state => state.movie);
    const { items } = useAppSelector(state => state.inventory);
    const { showtimeInfo, pricings, seats } = useAppSelector(state => state.showtime);

    useEffect(() => {
        dispatch(getInventory(cinemaId));
        dispatch(getMoviesListFeature({ cinemaId: cinemaId, showDate: selectedDate }));
    }, []);
    // Simulate fetching seat data when clicking a showtime
    const fetchSeatData = async (showtimeId: string) => {

        if (isConnected && connection) {
            try {
                await connection.invoke('JoinShowtimeGroup', showtimeId);
            } catch (error) {
                console.error('Failed to join group:', error);
            }
        }
        await dispatch(getShowtimeSeatingPlan(showtimeId))

        const data: SeatsSelectedResponse = {
            showtimeInfo: showtimeInfo,
            pricings: pricings,
            seats: seats,
        }
        console.log(data);
        setShowSeatSelection(true);
    };

    const handleDateChange = (date: string) => {
        if (date === selectedDate) return;
        setSelectedDate(date);
        console.log(date);
        dispatch(getMoviesListFeature({ cinemaId: cinemaId, showDate: date }));
    };

    const handleSelectSeat = (seat: Seat) => {
        if (seat.status !== 'Available') return;

        const pricing = pricings.find(p => p.seatTypeId === seat.seatTypeId);
        if (!pricing) return;

        const existingIndex = selectedSeats.findIndex(s => s.seatId === seat.id);
        let newSelected;
        if (existingIndex >= 0) {
            // Toggle: remove if already selected
            newSelected = selectedSeats.filter(s => s.seatId !== seat.id);
        } else {
            // Add if not selected
            newSelected = [...selectedSeats, { seatId: seat.id, price: pricing.finalPrice }];
        }
        setSelectedSeats(newSelected);
    };

    const addTicketToCart = async () => {
        if (selectedSeats.length === 0) return;

        setCart(prve => {
            return { ...prve, tickets: { showtimeId: showtimeInfo.id, actualStartTime: showtimeInfo.actualStartTime, actualEndTime: showtimeInfo.actualEndTime, movieTitle: showtimeInfo.movieTitle, selectedSeats: selectedSeats } };
        });

        // ivoke update seat status
        if (isConnected && connection) {
            try {
                await connection.invoke("SeatsReserved", showtimeInfo.id, selectedSeats.map(s => s.seatId));
            }
            catch (err) {
                console.log(err);
            }
        }
        setShowSeatSelection(false);
        setSelectedSeats([]);
    };

    const addConcessionToCart = (item: InventoryItem) => {
        const existingItem = cart.concessions.find(cartItem => cartItem.itemId === item.id);

        if (existingItem) {
            //check quantity before update
            if (existingItem.quantity >= item.currentStock) return;
            setCart(prev => {
                return {
                    ...prev, concessions: prev.concessions.map(cartItem =>
                        cartItem.itemId === item.id
                            ? { ...cartItem, quantity: cartItem.quantity + 1 }
                            : cartItem
                    )
                };
            });
        } else {
            setCart(prev => ({
                ...prev,
                concessions: [...prev.concessions, { itemId: item.id, itemName: item.itemName, price: item.unitPrice, quantity: 1 }]
            }));
        }
    };
    const handleUpdateItem = (itemId: string, action: 'increase' | 'decrease' | 'remove') => {
        setCart(prev => ({
            ...prev,
            concessions: prev.concessions
                .map(item => {
                    if (item.itemId === itemId) {
                        let newQuantity = item.quantity;
                        if (action === 'increase') {
                            newQuantity = item.quantity + 1;
                        } else if (action === 'decrease') {
                            newQuantity = item.quantity > 1 ? item.quantity - 1 : item.quantity;
                        } else if (action === 'remove') {
                            newQuantity = 0;
                        }
                        return { ...item, quantity: newQuantity };
                    }
                    return item;
                })
                .filter(item => item.quantity > 0)
        }));
    };
    const handleRemoveTicket = () => {
        try {
            if (connection && isConnected) {
                connection.invoke('SeatsReleased', cart.tickets.showtimeId, cart.tickets.selectedSeats.map(s => s.seatId));
                setCart(prve => {
                    return { ...prve, tickets: { showtimeId: '00000000-0000-0000-0000-000000000000', actualStartTime: '', actualEndTime: '', movieTitle: '', selectedSeats: [] } };
                });
            }
        }
        catch (error) {
            console.error('Remove ticket error:', error);
        }
    }

    const handleClearCart = () => {
        // Release seats if any
        if (cart.tickets.selectedSeats.length > 0) {
            handleRemoveTicket();
        }
        setCart({ staffId: staffId || '00000000-0000-0000-0000-000000000000', concessions: [], paymentMethod: 'cash', tickets: { showtimeId: '00000000-0000-0000-0000-000000000000', actualStartTime: '', actualEndTime: '', movieTitle: '', selectedSeats: [] } });
    }



    const getTotalAmount = () => {
        let total = 0;
        cart.tickets.selectedSeats.forEach(seat => {
            total += seat.price;
        });
        cart.concessions.forEach(item => {
            total += item.price * item.quantity;
        });
        return total;
    };


    const formatCurrency = (amount: number) => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    };

    const getDateString = (dateStr: string) => {
        const date = new Date(dateStr);
        return date.toLocaleDateString('vi-VN', {
            day: 'numeric',
            month: 'numeric',
            year: 'numeric'
        });
    };

    const getSeatColor = (seat: Seat, isSelected: boolean) => {
        if (isSelected) return 'bg-blue-500 text-white border-blue-500';

        // Base colors for different seat types
        const getBaseColor = (status: string) => {
            switch (status.toLowerCase()) {
                case 'available':
                    return seat.seatTypeName.toLowerCase() === 'vip'
                        ? 'bg-purple-50 hover:bg-purple-100 text-purple-800'
                        : seat.seatTypeName.toLowerCase() === 'couple'
                            ? 'bg-pink-50 hover:bg-pink-100 text-pink-800'
                            : 'bg-green-50 hover:bg-green-100 text-green-800';
                case 'reserved':
                    return 'bg-yellow-100 text-yellow-800 cursor-not-allowed';
                case 'booked':
                    return 'bg-red-100 text-red-800 cursor-not-allowed';
                case 'unavailable':
                    return 'bg-gray-100 text-gray-500 cursor-not-allowed';
                default:
                    return 'bg-gray-100 text-gray-500 cursor-not-allowed';
            }
        };

        return getBaseColor(seat.status);
    };

    const confirmPayment = () => {
        // call api to confirm payment
        try {
            dispatch(confirmConcessionPurchase({ cinemaId: cinemaId, cartItem: cart }));
            alert('Payment confirmed!');
            // Reset cart or handle success
            setCart({ staffId: staffId || '00000000-0000-0000-0000-000000000000', concessions: [], paymentMethod: 'cash', tickets: { showtimeId: '00000000-0000-0000-0000-000000000000', actualStartTime: '', actualEndTime: '', movieTitle: '', selectedSeats: [] } });
            setPaymentMethod('cash');
        }
        catch (err) {
            console.error(err);
        }

    };

    const formatVN = (d: Date) =>
        d.toLocaleDateString('vi-VN', { day: 'numeric', month: 'numeric' });

    useEffect(() => {
        setCart({ staffId: staffId || '00000000-0000-0000-0000-000000000000', concessions: [], paymentMethod: 'cash', tickets: { showtimeId: '00000000-0000-0000-0000-000000000000', actualStartTime: '', actualEndTime: '', movieTitle: '', selectedSeats: [] } });
    }, [activeTab]);

    return (
        <div className="flex h-screen bg-gray-50">
            {/* Main Content */}
            <div className="flex-1 flex flex-col">
                <TabNavigation activeTab={activeTab} onTabChange={setActiveTab} />

                {/* Content */}
                <div className="flex-1 p-6">
                    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 h-full">
                        {/* Products Section */}
                        <div className="lg:col-span-2 bg-white rounded-lg shadow border border-gray-200 p-6">
                            {activeTab === 'combo' ? (
                                <div>
                                    <h3 className="text-lg font-semibold mb-4 flex items-center">
                                        <Film className="w-5 h-5 mr-2" />
                                        Lịch chiếu từ {formatVN(today)} đến {formatVN(endDate)}
                                    </h3>

                                    <DateSelector
                                        dates={dates}
                                        selectedDate={selectedDate}
                                        onDateChange={handleDateChange}
                                        formatDate={getDateString}
                                    />

                                    {/* Movies for selected date */}
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

                                    {/* Concession Items */}
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