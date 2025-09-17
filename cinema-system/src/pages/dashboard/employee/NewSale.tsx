import React, { useState, useEffect, use } from 'react';
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
import { getInventory } from '../../../store/slices/inventorySlice';
import { getMoviesListFeature } from '../../../store/slices/movieSlice';
import { Coffee, Film } from 'lucide-react';
import { getShowtimeSeatingPlan } from '../../../store/slices/showtimeSlice';



const EmployeeNewSale: React.FC = () => {
    const [activeTab, setActiveTab] = useState<'combo' | 'concession'>('combo');
    const [cart, setCart] = useState<CartItem[]>([]);
    const [showSeatSelection, setShowSeatSelection] = useState(false);
    const [seatData, setSeatData] = useState<SeatsSelectedResponse | null>(null);
    const [selectedSeats, setSelectedSeats] = useState<SelectedSeat[]>([]);
    const [paymentMethod, setPaymentMethod] = useState<'cash' | 'card' | null>(null);

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
    useEffect(() => {
        dispatch(getInventory('779E27FD-DCF4-4F63-9B55-08DDE64DEED3'));
        dispatch(getMoviesListFeature({ cinemaId: '779E27FD-DCF4-4F63-9B55-08DDE64DEED3', showDate: selectedDate }));
    }, []);
    console.log(selectedDate);

    const dispatch = useAppDispatch();
    const { moviesListFeature } = useAppSelector(state => state.movie);
    const { items } = useAppSelector(state => state.inventory);
    const { showtimeInfo, pricings, seats } = useAppSelector(state => state.showtime);

    // Simulate fetching seat data when clicking a showtime
    const fetchSeatData = (showtimeId: string) => {

        dispatch(getShowtimeSeatingPlan(showtimeId))

        const data: SeatsSelectedResponse = {
            showtimeInfo: showtimeInfo,
            pricings: pricings,
            seats: seats,
        }
        setSeatData(data);
        setShowSeatSelection(true);
    };

    const handleDateChange = (date: string) => {
        if (date === selectedDate) return;
        setSelectedDate(date);
        console.log(date);
        dispatch(getMoviesListFeature({ cinemaId: '779E27FD-DCF4-4F63-9B55-08DDE64DEED3', showDate: date }));
    };

    const handleSelectSeat = (seat: Seat) => {
        if (seat.status !== 'available') return;
        const pricing = seatData?.pricings.find(p => p.seatTypeId === seat.seatTypeId);
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

    const addTicketToCart = () => {
        if (!seatData || selectedSeats.length === 0) return;

        // check duplication
        const existingItem = cart.find(cartItem => cartItem.id === seatData.showtimeInfo.id && cartItem.type === 'ticket');
        if (existingItem) return;

        const totalPrice = selectedSeats.reduce((sum, s) => sum + s.price, 0);
        const newItem: CartItem = {
            id: seatData.showtimeInfo.id,
            name: `${seatData.showtimeInfo.movieTitle}  (${seatData.showtimeInfo.actualStartTime} - ${seatData.showtimeInfo.actualEndTime})`,
            price: totalPrice,
            quantity: selectedSeats.length, // Tickets are per showtime, quantity 1 but with multiple seats
            type: 'ticket',
            selectedSeats: selectedSeats
        };
        setCart([...cart, newItem]);
        setShowSeatSelection(false);
        setSelectedSeats([]);
    };

    const addConcessionToCart = (item: InventoryItem) => {
        const existingItem = cart.find(cartItem => cartItem.id === item.id && cartItem.type === 'concession');

        if (existingItem) {
            //check quantity before update
            if (existingItem.quantity >= item.currentStock) return;
            setCart(cart.map(cartItem =>
                cartItem.id === item.id && cartItem.type === 'concession'
                    ? { ...cartItem, quantity: cartItem.quantity + 1 }
                    : cartItem
            ));
        } else {
            const newItem: CartItem = {
                id: item.id,
                name: item.itemName,
                price: item.unitPrice,
                quantity: 1,
                type: 'concession'
            };
            setCart([...cart, newItem]);
        }
    };

    const getTotalAmount = () => {
        return cart.reduce((total, item) => total + (item.price * item.quantity), 0);
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
        if (isSelected) return 'bg-blue-500 text-white';
        switch (seat.status) {
            case 'available':
                return 'bg-green-100 hover:bg-green-200';
            case 'reserved':
                return 'bg-yellow-100 cursor-not-allowed';
            case 'booked':
                return 'bg-red-100 cursor-not-allowed';
            case 'unavailable':
                return 'bg-gray-100 cursor-not-allowed';
            default:
                return 'bg-gray-100 cursor-not-allowed';
        }
    };

    const confirmPayment = () => {
        // Output: for each ticket item, log showtimeId and selectedSeats
        cart.forEach(item => {
            if (item.type === 'ticket' && item.selectedSeats) {
                console.log({
                    showtimeId: item.id,
                    selectedSeats: item.selectedSeats
                });
            }
        });
        // Output: for each concession item, log itemId and quantity
        cart.forEach(item => {
            if (item.type === 'concession') {
                console.log({
                    itemId: item.id,
                    quantity: item.quantity
                });
            }
        });
        // Reset cart or handle success
        setCart([]);
        setPaymentMethod(null);
        alert('Thanh toán thành công! Thông tin chi tiết đã được in ra console.');
    };

    const formatVN = (d: Date) =>
        d.toLocaleDateString('vi-VN', { day: 'numeric', month: 'numeric' });

    useEffect(() => {
        setCart([]);
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
                                        seatData={showSeatSelection ? seatData : null}
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
                            onClear={() => setCart([])}
                            onConfirmPayment={confirmPayment}
                            paymentMethod={paymentMethod}
                            onPaymentMethodChange={setPaymentMethod}
                            formatCurrency={formatCurrency}
                        />
                    </div>
                </div>
            </div>
        </div>
    );
};

export default EmployeeNewSale;