import React, { useEffect, useState } from 'react';
import { TrendingUp, Users, Package, Film, Calendar, ChevronDown, DollarSign, Activity, Clock } from 'lucide-react';
import { BarChart, Bar, LineChart, Line, AreaChart, Area, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, RadarChart, PolarGrid, PolarAngleAxis, PolarRadiusAxis, Radar } from 'recharts';
import { useAppDispatch } from '../../../hooks/redux';
import { getStaffWorkingInfo } from '../../../store/slices/staffSlice';
import { getEmailFromToken } from '../../../utils/decodeTokenAndGetUser';

// Types
interface TicketsData {
    totalTickets: number;
    totalTicketsAmount: number;
    transactions: number;
}

interface ConcessionsData {
    itemsSold: number;
    concessionRevenue: number;
}

interface RevenueData {
    tickets: TicketsData;
    concessions: ConcessionsData;
}

interface ShowtimeData {
    movie: string;
    actualStartTime: string;
    screenName: string;
    totalSeats: number;
    soldSeats: number;
    occupancyRate: number;
}

interface InventoryItem {
    itemName: string;
    currentStock: number;
    minimumStock: number;
    stockStatus: string;
}

interface StaffData {
    totalStaffWorkingToday: number;
    checkedIn: number;
    late: number;
}

type DateRangeType = 'today' | 'week' | 'month' | 'year';

// RevenueOverview.tsx
const RevenueOverview: React.FC<{ data: RevenueData }> = ({ data }) => {
    const [dateRange, setDateRange] = useState<DateRangeType>('today');
    const [isDropdownOpen, setIsDropdownOpen] = useState(false);

    const totalRevenue = data.tickets.totalTicketsAmount + data.concessions.concessionRevenue;

    const formatCurrency = (amount: number): string => {
        return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
    };

    const dateRangeLabels: Record<DateRangeType, string> = {
        today: 'Hôm nay',
        week: '7 ngày qua',
        month: 'Tháng này',
        year: 'Năm nay'
    };

    // Mock trend data for line chart
    const trendData = [
        { time: '00:00', revenue: 0 },
        { time: '06:00', revenue: 120000 },
        { time: '09:00', revenue: 280000 },
        { time: '12:00', revenue: 450000 },
        { time: '15:00', revenue: 680000 },
        { time: '18:00', revenue: 950000 },
        { time: '21:00', revenue: 1236000 },
    ];

    const handleDateRangeChange = (range: DateRangeType) => {
        setDateRange(range);
        setIsDropdownOpen(false);
        // TODO: Call API with new date range
        console.log('Date range changed to:', range);
    };

    return (
        <div className="bg-white rounded-xl shadow-sm border border-blue-100 p-6">
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                        <DollarSign className="text-blue-600" size={20} />
                    </div>
                    <h2 className="text-lg font-semibold text-gray-800">Doanh thu</h2>
                </div>

                <div className="relative">
                    <button
                        onClick={() => setIsDropdownOpen(!isDropdownOpen)}
                        className="flex items-center gap-2 px-4 py-2 bg-blue-50 text-blue-700 rounded-lg hover:bg-blue-100 transition text-sm font-medium"
                    >
                        <Calendar size={16} />
                        {dateRangeLabels[dateRange]}
                        <ChevronDown size={16} />
                    </button>

                    {isDropdownOpen && (
                        <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-10">
                            {(Object.keys(dateRangeLabels) as DateRangeType[]).map((range) => (
                                <button
                                    key={range}
                                    onClick={() => handleDateRangeChange(range)}
                                    className={`w-full text-left px-4 py-2 text-sm hover:bg-blue-50 transition ${dateRange === range ? 'bg-blue-50 text-blue-700 font-medium' : 'text-gray-700'
                                        }`}
                                >
                                    {dateRangeLabels[range]}
                                </button>
                            ))}
                        </div>
                    )}
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
                <div className="lg:col-span-2 space-y-4">
                    <div className="bg-gradient-to-br from-blue-50 to-blue-100 rounded-xl p-5 border border-blue-200">
                        <div className="flex items-center justify-between mb-2">
                            <div className="text-sm text-blue-700 font-medium">Tổng doanh thu</div>
                            <div className="flex items-center gap-1 text-xs text-green-600 font-medium">
                                <TrendingUp size={14} />
                                +12.5%
                            </div>
                        </div>
                        <div className="text-3xl font-bold text-blue-900">{formatCurrency(totalRevenue)}</div>
                        <div className="text-xs text-blue-600 mt-1">So với kỳ trước</div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="border border-blue-100 rounded-xl p-4 hover:shadow-md transition">
                            <div className="flex items-center justify-between mb-2">
                                <div className="text-sm text-gray-600">Doanh thu vé</div>
                                <div className="text-xs bg-blue-100 text-blue-700 px-2 py-1 rounded-full font-medium">
                                    {((data.tickets.totalTicketsAmount / totalRevenue) * 100).toFixed(1)}%
                                </div>
                            </div>
                            <div className="text-2xl font-bold text-gray-900 mb-1">{formatCurrency(data.tickets.totalTicketsAmount)}</div>
                            <div className="flex items-center justify-between text-xs text-gray-500">
                                <span>{data.tickets.totalTickets} vé</span>
                                <span>{data.tickets.transactions} giao dịch</span>
                            </div>
                        </div>

                        <div className="border border-blue-100 rounded-xl p-4 hover:shadow-md transition">
                            <div className="flex items-center justify-between mb-2">
                                <div className="text-sm text-gray-600">Doanh thu F&B</div>
                                <div className="text-xs bg-blue-100 text-blue-700 px-2 py-1 rounded-full font-medium">
                                    {((data.concessions.concessionRevenue / totalRevenue) * 100).toFixed(1)}%
                                </div>
                            </div>
                            <div className="text-2xl font-bold text-gray-900 mb-1">{formatCurrency(data.concessions.concessionRevenue)}</div>
                            <div className="text-xs text-gray-500">{data.concessions.itemsSold} món đã bán</div>
                        </div>
                    </div>
                </div>

                <div className="bg-gradient-to-br from-gray-50 to-gray-100 rounded-xl p-5 border border-gray-200">
                    <div className="text-sm text-gray-600 mb-4 font-medium">Xu hướng trong ngày</div>
                    <ResponsiveContainer width="100%" height={200}>
                        <AreaChart data={trendData}>
                            <defs>
                                <linearGradient id="revenueGradient" x1="0" y1="0" x2="0" y2="1">
                                    <stop offset="5%" stopColor="#3B82F6" stopOpacity={0.3} />
                                    <stop offset="95%" stopColor="#3B82F6" stopOpacity={0} />
                                </linearGradient>
                            </defs>
                            <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                            <XAxis dataKey="time" tick={{ fontSize: 11 }} stroke="#9CA3AF" />
                            <YAxis tick={{ fontSize: 11 }} stroke="#9CA3AF" />
                            <Tooltip
                                formatter={(value: number) => formatCurrency(value)}
                                contentStyle={{ backgroundColor: '#fff', border: '1px solid #E5E7EB', borderRadius: '8px' }}
                            />
                            <Area type="monotone" dataKey="revenue" stroke="#3B82F6" strokeWidth={2} fill="url(#revenueGradient)" />
                        </AreaChart>
                    </ResponsiveContainer>
                </div>
            </div>
        </div>
    );
};

// ShowtimePerformance.tsx
const ShowtimePerformance: React.FC<{ data: ShowtimeData[] }> = ({ data }) => {
    const avgOccupancy = data.length > 0 ? (data.reduce((sum, show) => sum + show.occupancyRate, 0) / data.length).toFixed(1) : '0';

    // Group by movie and calculate stats
    const movieStats = data.reduce((acc: Record<string, any>, show) => {
        if (!acc[show.movie]) {
            acc[show.movie] = { movie: show.movie, totalSeats: 0, soldSeats: 0, showCount: 0 };
        }
        acc[show.movie].totalSeats += show.totalSeats;
        acc[show.movie].soldSeats += show.soldSeats;
        acc[show.movie].showCount += 1;
        return acc;
    }, {});

    const chartData = Object.values(movieStats).map((stat: any) => ({
        movie: stat.movie.length > 12 ? stat.movie.substring(0, 12) + '...' : stat.movie,
        'Lấp đầy': parseFloat(((stat.soldSeats / stat.totalSeats) * 100).toFixed(1)),
        'Suất chiếu': stat.showCount
    }));

    // Time slot analysis
    const timeSlotData = [
        { time: 'Sáng', shows: 2, occupancy: 15 },
        { time: 'Trưa', shows: 3, occupancy: 35 },
        { time: 'Chiều', shows: 4, occupancy: 45 },
        { time: 'Tối', shows: 5, occupancy: 65 }
    ];

    return (
        <div className="bg-white rounded-xl shadow-sm border border-blue-100 p-6">
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                        <Film className="text-blue-600" size={20} />
                    </div>
                    <h2 className="text-lg font-semibold text-gray-800">Hiệu suất suất chiếu</h2>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
                <div className="bg-gradient-to-br from-blue-50 to-blue-100 rounded-xl p-5 border border-blue-200">
                    <div className="text-sm text-blue-700 font-medium mb-2">Tỷ lệ lấp đầy TB</div>
                    <div className="text-3xl font-bold text-blue-900 mb-2">{avgOccupancy}%</div>
                    <div className="w-full bg-blue-200 rounded-full h-2">
                        <div className="bg-blue-600 h-2 rounded-full transition-all" style={{ width: `${avgOccupancy}%` }} />
                    </div>
                </div>

                <div className="bg-gradient-to-br from-green-50 to-green-100 rounded-xl p-5 border border-green-200">
                    <div className="text-sm text-green-700 font-medium mb-2">Tổng suất chiếu</div>
                    <div className="text-3xl font-bold text-green-900">{data.length}</div>
                    <div className="text-xs text-green-600 mt-1">Đang hoạt động</div>
                </div>

                <div className="bg-gradient-to-br from-purple-50 to-purple-100 rounded-xl p-5 border border-purple-200">
                    <div className="text-sm text-purple-700 font-medium mb-2">Ghế đã bán</div>
                    <div className="text-3xl font-bold text-purple-900">{data.reduce((sum, s) => sum + s.soldSeats, 0)}</div>
                    <div className="text-xs text-purple-600 mt-1">/ {data.reduce((sum, s) => sum + s.totalSeats, 0)} tổng ghế</div>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
                <div>
                    <div className="text-sm font-medium text-gray-700 mb-3">Hiệu suất theo phim</div>
                    <ResponsiveContainer width="100%" height={220}>
                        <BarChart data={chartData}>
                            <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                            <XAxis dataKey="movie" tick={{ fontSize: 11 }} stroke="#6B7280" />
                            <YAxis stroke="#6B7280" />
                            <Tooltip
                                contentStyle={{ backgroundColor: '#fff', border: '1px solid #E5E7EB', borderRadius: '8px' }}
                            />
                            <Bar dataKey="Lấp đầy" fill="#3B82F6" radius={[8, 8, 0, 0]} />
                        </BarChart>
                    </ResponsiveContainer>
                </div>

                <div>
                    <div className="text-sm font-medium text-gray-700 mb-3">Phân bổ theo khung giờ</div>
                    <ResponsiveContainer width="100%" height={220}>
                        <LineChart data={timeSlotData}>
                            <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                            <XAxis dataKey="time" stroke="#6B7280" />
                            <YAxis stroke="#6B7280" />
                            <Tooltip
                                contentStyle={{ backgroundColor: '#fff', border: '1px solid #E5E7EB', borderRadius: '8px' }}
                            />
                            <Legend />
                            <Line type="monotone" dataKey="occupancy" stroke="#3B82F6" strokeWidth={2} name="Lấp đầy %" />
                            <Line type="monotone" dataKey="shows" stroke="#10B981" strokeWidth={2} name="Số suất" />
                        </LineChart>
                    </ResponsiveContainer>
                </div>
            </div>

            <div className="space-y-2 max-h-72 overflow-y-auto">
                <div className="text-sm font-medium text-gray-700 mb-3">Chi tiết suất chiếu gần đây</div>
                {data.slice(0, 8).map((show, idx) => (
                    <div key={idx} className="border border-blue-100 rounded-lg p-4 hover:shadow-md transition hover:border-blue-300">
                        <div className="flex justify-between items-start mb-3">
                            <div>
                                <div className="font-semibold text-gray-900 text-sm mb-1">{show.movie}</div>
                                <div className="text-xs text-gray-500">
                                    {new Date(show.actualStartTime).toLocaleString('vi-VN', {
                                        day: '2-digit',
                                        month: '2-digit',
                                        hour: '2-digit',
                                        minute: '2-digit'
                                    })}
                                </div>
                            </div>
                            <div className="flex items-center gap-2">
                                <span className="text-xs bg-blue-100 text-blue-700 px-2 py-1 rounded font-medium">{show.screenName}</span>
                                <span className={`text-xs px-2 py-1 rounded font-medium ${show.occupancyRate >= 70 ? 'bg-green-100 text-green-700' :
                                        show.occupancyRate >= 40 ? 'bg-yellow-100 text-yellow-700' :
                                            'bg-red-100 text-red-700'
                                    }`}>
                                    {show.occupancyRate}%
                                </span>
                            </div>
                        </div>
                        <div className="flex items-center gap-3">
                            <div className="flex-1 bg-gray-200 rounded-full h-2">
                                <div
                                    className={`h-2 rounded-full transition-all ${show.occupancyRate >= 70 ? 'bg-green-500' :
                                            show.occupancyRate >= 40 ? 'bg-yellow-500' :
                                                'bg-red-500'
                                        }`}
                                    style={{ width: `${show.occupancyRate}%` }}
                                />
                            </div>
                            <span className="text-xs text-gray-600 font-medium whitespace-nowrap">
                                {show.soldSeats}/{show.totalSeats}
                            </span>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};

// InventoryStatus.tsx
const InventoryStatus: React.FC<{ data: InventoryItem[] }> = ({ data }) => {
    const lowStockItems = data.filter(item => item.currentStock < item.minimumStock * 1.5);
    const criticalItems = data.filter(item => item.currentStock < item.minimumStock);

    const chartData = data.map(item => ({
        name: item.itemName.length > 15 ? item.itemName.substring(0, 15) + '...' : item.itemName,
        'Tồn kho': item.currentStock,
        'Tồn TB': item.minimumStock * 2
    }));

    // Stock health radar
    const radarData = data.slice(0, 5).map(item => ({
        product: item.itemName.split(' ')[0],
        health: Math.min((item.currentStock / (item.minimumStock * 3)) * 100, 100)
    }));

    return (
        <div className="bg-white rounded-xl shadow-sm border border-blue-100 p-6">
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                        <Package className="text-blue-600" size={20} />
                    </div>
                    <h2 className="text-lg font-semibold text-gray-800">Quản lý tồn kho</h2>
                </div>
            </div>

            <div className="grid grid-cols-3 gap-4 mb-6">
                <div className="bg-gradient-to-br from-blue-50 to-blue-100 rounded-xl p-4 border border-blue-200">
                    <div className="text-sm text-blue-700 font-medium mb-1">Tổng sản phẩm</div>
                    <div className="text-2xl font-bold text-blue-900">{data.length}</div>
                </div>

                <div className="bg-gradient-to-br from-amber-50 to-amber-100 rounded-xl p-4 border border-amber-200">
                    <div className="text-sm text-amber-700 font-medium mb-1">Sắp hết</div>
                    <div className="text-2xl font-bold text-amber-900">{lowStockItems.length}</div>
                </div>

                <div className="bg-gradient-to-br from-red-50 to-red-100 rounded-xl p-4 border border-red-200">
                    <div className="text-sm text-red-700 font-medium mb-1">Báo động</div>
                    <div className="text-2xl font-bold text-red-900">{criticalItems.length}</div>
                </div>
            </div>

            {lowStockItems.length > 0 && (
                <div className="mb-6 bg-amber-50 border-l-4 border-amber-500 rounded-lg p-4">
                    <div className="flex items-center gap-2">
                        <Activity className="text-amber-600" size={18} />
                        <div className="text-sm text-amber-900 font-medium">
                            Cảnh báo: {lowStockItems.length} sản phẩm cần nhập thêm hàng
                        </div>
                    </div>
                </div>
            )}

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
                <div>
                    <div className="text-sm font-medium text-gray-700 mb-3">So sánh tồn kho</div>
                    <ResponsiveContainer width="100%" height={250}>
                        <BarChart data={chartData} layout="horizontal">
                            <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                            <XAxis type="number" stroke="#6B7280" />
                            <YAxis dataKey="name" type="category" width={100} tick={{ fontSize: 11 }} stroke="#6B7280" />
                            <Tooltip
                                contentStyle={{ backgroundColor: '#fff', border: '1px solid #E5E7EB', borderRadius: '8px' }}
                            />
                            <Legend />
                            <Bar dataKey="Tồn kho" fill="#3B82F6" radius={[0, 4, 4, 0]} />
                            <Bar dataKey="Tồn TB" fill="#93C5FD" radius={[0, 4, 4, 0]} />
                        </BarChart>
                    </ResponsiveContainer>
                </div>

                <div>
                    <div className="text-sm font-medium text-gray-700 mb-3">Sức khỏe tồn kho</div>
                    <ResponsiveContainer width="100%" height={250}>
                        <RadarChart data={radarData}>
                            <PolarGrid stroke="#E5E7EB" />
                            <PolarAngleAxis dataKey="product" tick={{ fontSize: 11 }} />
                            <PolarRadiusAxis angle={90} domain={[0, 100]} />
                            <Radar name="Health %" dataKey="health" stroke="#3B82F6" fill="#3B82F6" fillOpacity={0.5} />
                            <Tooltip />
                        </RadarChart>
                    </ResponsiveContainer>
                </div>
            </div>

            <div className="space-y-3 max-h-64 overflow-y-auto">
                <div className="text-sm font-medium text-gray-700 mb-3">Chi tiết sản phẩm</div>
                {data.map((item, idx) => {
                    const stockPercentage = (item.currentStock / (item.minimumStock * 3)) * 100;
                    const isLow = item.currentStock < item.minimumStock * 1.5;
                    const isCritical = item.currentStock < item.minimumStock;

                    return (
                        <div key={idx} className={`border rounded-lg p-4 transition hover:shadow-md ${isCritical ? 'border-red-200 bg-red-50' :
                                isLow ? 'border-amber-200 bg-amber-50' :
                                    'border-blue-100'
                            }`}>
                            <div className="flex justify-between items-center mb-3">
                                <div className="font-semibold text-gray-900 text-sm">{item.itemName}</div>
                                <div className="flex items-center gap-2">
                                    <span className={`text-xs px-3 py-1 rounded-full font-medium ${isCritical ? 'bg-red-200 text-red-800' :
                                            isLow ? 'bg-amber-200 text-amber-800' :
                                                'bg-green-100 text-green-700'
                                        }`}>
                                        {item.currentStock} còn lại
                                    </span>
                                </div>
                            </div>
                            <div className="flex items-center gap-3">
                                <div className="flex-1">
                                    <div className="flex justify-between text-xs text-gray-600 mb-1">
                                        <span>Tối thiểu: {item.minimumStock}</span>
                                        <span className="font-medium">{stockPercentage.toFixed(0)}%</span>
                                    </div>
                                    <div className="w-full bg-gray-200 rounded-full h-2">
                                        <div
                                            className={`h-2 rounded-full transition-all ${isCritical ? 'bg-red-500' :
                                                    isLow ? 'bg-amber-500' :
                                                        'bg-green-500'
                                                }`}
                                            style={{ width: `${Math.min(stockPercentage, 100)}%` }}
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
};

// StaffAttendance.tsx
const StaffAttendance: React.FC<{ data: StaffData }> = ({ data }) => {
    const onTimeRate = data.totalStaffWorkingToday > 0
        ? ((data.checkedIn - data.late) / data.totalStaffWorkingToday * 100).toFixed(1)
        : '0';

    const onTime = data.checkedIn - data.late;
    const notCheckedIn = data.totalStaffWorkingToday - data.checkedIn;

    const chartData = [
        { name: 'Đúng giờ', value: onTime, color: '#3B82F6' },
        { name: 'Đi muộn', value: data.late, color: '#F59E0B' },
        { name: 'Chưa check-in', value: notCheckedIn, color: '#EF4444' }
    ].filter(d => d.value > 0);

    // Weekly attendance trend
    const weeklyData = [
        { day: 'T2', onTime: 10, late: 2 },
        { day: 'T3', onTime: 11, late: 1 },
        { day: 'T4', onTime: 12, late: 0 },
        { day: 'T5', onTime: 10, late: 2 },
        { day: 'T6', onTime: 11, late: 1 },
        { day: 'T7', onTime: 12, late: 0 },
        { day: 'CN', onTime: onTime, late: data.late }
    ];

    return (
        <div className="bg-white rounded-xl shadow-sm border border-blue-100 p-6">
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                        <Users className="text-blue-600" size={20} />
                    </div>
                    <h2 className="text-lg font-semibold text-gray-800">Chấm công nhân viên</h2>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-4 gap-4 mb-6">
                <div className="bg-gradient-to-br from-blue-50 to-blue-100 rounded-xl p-4 border border-blue-200">
                    <div className="text-sm text-blue-700 font-medium mb-1">Tổng NV</div>
                    <div className="text-3xl font-bold text-blue-900">{data.totalStaffWorkingToday}</div>
                </div>

                <div className="bg-gradient-to-br from-green-50 to-green-100 rounded-xl p-4 border border-green-200">
                    <div className="text-sm text-green-700 font-medium mb-1">Đúng giờ</div>
                    <div className="text-3xl font-bold text-green-900">{onTime}</div>
                </div>

                <div className="bg-gradient-to-br from-amber-50 to-amber-100 rounded-xl p-4 border border-amber-200">
                    <div className="text-sm text-amber-700 font-medium mb-1">Đi muộn</div>
                    <div className="text-3xl font-bold text-amber-900">{data.late}</div>
                </div>

                <div className="bg-gradient-to-br from-purple-50 to-purple-100 rounded-xl p-4 border border-purple-200">
                    <div className="text-sm text-purple-700 font-medium mb-1">Tỷ lệ đúng giờ</div>
                    <div className="text-3xl font-bold text-purple-900">{onTimeRate}%</div>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <div>
                    <div className="text-sm font-medium text-gray-700 mb-3">Phân bổ hôm nay</div>
                    <div className="flex items-center justify-center">
                        <ResponsiveContainer width="100%" height={220}>
                            <PieChart>
                                <Pie
                                    data={chartData}
                                    cx="50%"
                                    cy="50%"
                                    labelLine={false}
                                    label={({ name, value }: { name: string; value: number }) => `${name}: ${value}`}
                                    outerRadius={80}
                                    fill="#8884d8"
                                    dataKey="value"
                                >
                                    {chartData.map((entry, index) => (
                                        <Cell key={`cell-${index}`} fill={entry.color} />
                                    ))}
                                </Pie>
                                <Tooltip />
                            </PieChart>
                        </ResponsiveContainer>
                    </div>
                </div>

                <div>
                    <div className="text-sm font-medium text-gray-700 mb-3">Xu hướng tuần này</div>
                    <ResponsiveContainer width="100%" height={220}>
                        <BarChart data={weeklyData}>
                            <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                            <XAxis dataKey="day" stroke="#6B7280" />
                            <YAxis stroke="#6B7280" />
                            <Tooltip
                                contentStyle={{ backgroundColor: '#fff', border: '1px solid #E5E7EB', borderRadius: '8px' }}
                            />
                            <Legend />
                            <Bar dataKey="onTime" fill="#3B82F6" radius={[4, 4, 0, 0]} name="Đúng giờ" />
                            <Bar dataKey="late" fill="#F59E0B" radius={[4, 4, 0, 0]} name="Đi muộn" />
                        </BarChart>
                    </ResponsiveContainer>
                </div>
            </div>

            <div className="mt-6 p-4 bg-gradient-to-r from-blue-50 to-blue-100 rounded-xl border border-blue-200">
                <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <Clock className="text-blue-600" size={20} />
                        <div>
                            <div className="text-sm font-medium text-blue-900">Hiệu suất chấm công</div>
                            <div className="text-xs text-blue-700 mt-1">
                                {onTime} / {data.totalStaffWorkingToday} nhân viên đã check-in đúng giờ
                            </div>
                        </div>
                    </div>
                    <div className="w-24">
                        <div className="w-full bg-blue-200 rounded-full h-3">
                            <div
                                className="bg-blue-600 h-3 rounded-full transition-all"
                                style={{ width: `${onTimeRate}%` }}
                            />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

// Main Dashboard Component
const ManagerDashboard: React.FC = () => {
    const dispatch = useAppDispatch();

    useEffect(() => {
        dispatch(getStaffWorkingInfo(getEmailFromToken()));
    }, [dispatch]);

    // Mock data - replace with actual API calls
    const revenueData: RevenueData = {
        tickets: { totalTickets: 8, totalTicketsAmount: 560000, transactions: 4 },
        concessions: { itemsSold: 16, concessionRevenue: 676000 }
    };

    const showtimeData: ShowtimeData[] = [
        { movie: "Hidden Truth", actualStartTime: "2025-11-13T13:20:00", screenName: "P01", totalSeats: 98, soldSeats: 0, occupancyRate: 0 },
        { movie: "Hidden Truth", actualStartTime: "2025-11-12T22:03:00", screenName: "P01", totalSeats: 98, soldSeats: 0, occupancyRate: 0 },
        { movie: "Mưa Đỏ", actualStartTime: "2025-11-12T13:30:00", screenName: "P01", totalSeats: 98, soldSeats: 12, occupancyRate: 12.24 },
        { movie: "Hidden Truth", actualStartTime: "2025-11-11T22:00:00", screenName: "P01", totalSeats: 98, soldSeats: 45, occupancyRate: 45.92 },
        { movie: "Cyber Storm", actualStartTime: "2025-10-19T20:00:00", screenName: "P01", totalSeats: 98, soldSeats: 25, occupancyRate: 25.51 },
        { movie: "Cyber Storm", actualStartTime: "2025-08-30T18:00:00", screenName: "P01", totalSeats: 98, soldSeats: 78, occupancyRate: 79.59 },
        { movie: "Mưa Đỏ", actualStartTime: "2025-08-30T15:00:00", screenName: "P02", totalSeats: 120, soldSeats: 95, occupancyRate: 79.17 },
        { movie: "Hidden Truth", actualStartTime: "2025-08-29T20:00:00", screenName: "P01", totalSeats: 98, soldSeats: 68, occupancyRate: 69.39 }
    ];

    const inventoryData: InventoryItem[] = [
        { itemName: "Popcorn (Large)", currentStock: 107, minimumStock: 20, stockStatus: "Còn hàng" },
        { itemName: "Coca Cola 330ml", currentStock: 193, minimumStock: 50, stockStatus: "Còn hàng" },
        { itemName: "Pepsi 500ml", currentStock: 180, minimumStock: 40, stockStatus: "Còn hàng" },
        { itemName: "Nachos with Cheese", currentStock: 90, minimumStock: 15, stockStatus: "Còn hàng" },
        { itemName: "Hotdog", currentStock: 66, minimumStock: 10, stockStatus: "Còn hàng" },
        { itemName: "Combo Popcorn + Drink", currentStock: 18, minimumStock: 10, stockStatus: "Sắp hết" },
        { itemName: "Iced Tea 500ml", currentStock: 150, minimumStock: 30, stockStatus: "Còn hàng" }
    ];

    const staffData: StaffData = { totalStaffWorkingToday: 12, checkedIn: 10, late: 2 };

    return (
        <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50">
            <div className="bg-white border-b border-blue-100 px-6 py-5 shadow-sm">
                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">Dashboard Quản Lý Rạp</h1>
                        <p className="text-sm text-gray-600 mt-1">Tổng quan hoạt động và phân tích chi tiết</p>
                    </div>
                    <div className="flex items-center gap-3 text-sm text-gray-700 bg-blue-50 px-4 py-2 rounded-lg border border-blue-200">
                        <Calendar size={18} className="text-blue-600" />
                        <span className="font-medium">
                            {new Date().toLocaleDateString('vi-VN', {
                                weekday: 'long',
                                year: 'numeric',
                                month: 'long',
                                day: 'numeric'
                            })}
                        </span>
                    </div>
                </div>
            </div>

            <div className="p-6 space-y-6">
                <div className="grid grid-cols-1 gap-6">
                    <RevenueOverview data={revenueData} />
                </div>

                <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
                    <ShowtimePerformance data={showtimeData} />
                    <InventoryStatus data={inventoryData} />
                </div>

                <div className="grid grid-cols-1 gap-6">
                    <StaffAttendance data={staffData} />
                </div>
            </div>
        </div>
    );
};

export default ManagerDashboard