import React, { useState } from 'react';
import { Calendar, TrendingUp, Ticket, ShoppingBag, DollarSign, BarChart3, PieChart, Activity } from 'lucide-react';
import { BarChart, Bar, LineChart, Line, PieChart as RechartsPieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';

interface MonthlyRevenueData {
    month: string;
    ticketRevenue: number;
    ticketCount: number;
    averageTicketPrice: number;
    concessionRevenue: number;
    concessionCount: number;
    averageConcessionPrice: number;
    totalRevenue: number;
    concessionRatioPercent: number;
}

const MOCK_DATA: MonthlyRevenueData[] = [
    {
        month: "2025-09",
        ticketRevenue: 560000,
        ticketCount: 8,
        averageTicketPrice: 70000,
        concessionRevenue: 1491000,
        concessionCount: 33,
        averageConcessionPrice: 45181.818181,
        totalRevenue: 2051000,
        concessionRatioPercent: 72.7
    },
    {
        month: "2025-10",
        ticketRevenue: 70000,
        ticketCount: 1,
        averageTicketPrice: 70000,
        concessionRevenue: 100000,
        concessionCount: 2,
        averageConcessionPrice: 50000,
        totalRevenue: 170000,
        concessionRatioPercent: 58.82
    },
    {
        month: "2025-08",
        ticketRevenue: 1200000,
        ticketCount: 15,
        averageTicketPrice: 80000,
        concessionRevenue: 890000,
        concessionCount: 18,
        averageConcessionPrice: 49444.44,
        totalRevenue: 2090000,
        concessionRatioPercent: 42.58
    },
    {
        month: "2025-07",
        ticketRevenue: 1850000,
        ticketCount: 25,
        averageTicketPrice: 74000,
        concessionRevenue: 1250000,
        concessionCount: 28,
        averageConcessionPrice: 44642.86,
        totalRevenue: 3100000,
        concessionRatioPercent: 40.32
    }
];

const COLORS = ['#3b82f6', '#f97316', '#10b981', '#8b5cf6', '#ef4444', '#06b6d4'];

const ManagerMonthlyReport: React.FC = () => {
    const [cinemaldPath] = useState("779E27FD-DCF4-4F63-9B55-08DDE64DEEI");
    const [cinemaldQuery] = useState("779E27FD-DCF4-4F63-9B55-08DDE64DEEI");
    const [startDate, setStartDate] = useState("2025-09-01");
    const [endDate, setEndDate] = useState("2025-09-30");
    const [revenueData] = useState<MonthlyRevenueData[]>(MOCK_DATA);

    const formatCurrency = (amount: number) => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    };

    const formatMonth = (monthString: string) => {
        const [year, month] = monthString.split('-');
        return `Tháng ${month}/${year}`;
    };

    const formatShortCurrency = (value: number) => {
        if (value >= 1000000) {
            return `${(value / 1000000).toFixed(1)}M`;
        }
        if (value >= 1000) {
            return `${(value / 1000).toFixed(0)}K`;
        }
        return value.toString();
    };

    const calculateTotals = () => {
        return revenueData.reduce((acc, item) => ({
            totalRevenue: acc.totalRevenue + item.totalRevenue,
            ticketRevenue: acc.ticketRevenue + item.ticketRevenue,
            concessionRevenue: acc.concessionRevenue + item.concessionRevenue,
            ticketCount: acc.ticketCount + item.ticketCount,
            concessionCount: acc.concessionCount + item.concessionCount
        }), {
            totalRevenue: 0,
            ticketRevenue: 0,
            concessionRevenue: 0,
            ticketCount: 0,
            concessionCount: 0
        });
    };

    const totals = calculateTotals();
    const sortedData = [...revenueData].sort((a, b) => a.month.localeCompare(b.month));

    // Prepare chart data
    const chartData = sortedData.map(item => ({
        name: formatMonth(item.month),
        'Doanh thu vé': item.ticketRevenue,
        'Doanh thu đồ ăn': item.concessionRevenue,
        'Tổng doanh thu': item.totalRevenue
    }));

    const pieData = [
        { name: 'Doanh thu vé', value: totals.ticketRevenue },
        { name: 'Doanh thu đồ ăn', value: totals.concessionRevenue }
    ];

    const growthData = sortedData.map((item, index) => {
        if (index === 0) return { name: formatMonth(item.month), growth: 0 };
        const prevRevenue = sortedData[index - 1].totalRevenue;
        const growth = ((item.totalRevenue - prevRevenue) / prevRevenue) * 100;
        return { name: formatMonth(item.month), growth: parseFloat(growth.toFixed(2)) };
    });

    const avgTicketPriceData = sortedData.map(item => ({
        name: formatMonth(item.month),
        'Giá vé TB': item.averageTicketPrice,
        'Giá đồ ăn TB': item.averageConcessionPrice
    }));

    return (
        <div className="p-6 bg-gray-50 min-h-screen">
            {/* Header */}
            <div className="mb-6">
                <h1 className="text-2xl font-bold text-gray-900 mb-2">Báo cáo doanh thu theo tháng</h1>
                <p className="text-gray-600">Tổng hợp và phân tích doanh thu, hiệu suất kinh doanh theo tháng</p>
            </div>

            {/* Filters */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Cinema ID (Path) <span className="text-red-500">*</span>
                        </label>
                        <input
                            type="text"
                            value={cinemaldPath}
                            readOnly
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-gray-50 text-gray-600"
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Cinema ID (Query)
                        </label>
                        <input
                            type="text"
                            value={cinemaldQuery}
                            readOnly
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-gray-50 text-gray-600"
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Ngày bắt đầu
                        </label>
                        <input
                            type="date"
                            value={startDate}
                            onChange={(e) => setStartDate(e.target.value)}
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Ngày kết thúc
                        </label>
                        <input
                            type="date"
                            value={endDate}
                            onChange={(e) => setEndDate(e.target.value)}
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                        />
                    </div>
                </div>
            </div>

            {/* Summary Cards */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
                <div className="bg-gradient-to-br from-green-500 to-green-600 rounded-lg shadow-lg p-6 text-white">
                    <div className="flex items-center justify-between mb-2">
                        <span className="text-sm font-medium opacity-90">Tổng doanh thu</span>
                        <DollarSign className="w-6 h-6 opacity-80" />
                    </div>
                    <p className="text-3xl font-bold mb-1">{formatShortCurrency(totals.totalRevenue)}</p>
                    <p className="text-xs opacity-75">{formatCurrency(totals.totalRevenue)}</p>
                </div>

                <div className="bg-gradient-to-br from-blue-500 to-blue-600 rounded-lg shadow-lg p-6 text-white">
                    <div className="flex items-center justify-between mb-2">
                        <span className="text-sm font-medium opacity-90">Doanh thu vé</span>
                        <Ticket className="w-6 h-6 opacity-80" />
                    </div>
                    <p className="text-3xl font-bold mb-1">{formatShortCurrency(totals.ticketRevenue)}</p>
                    <p className="text-xs opacity-75">{totals.ticketCount} vé bán ra</p>
                </div>

                <div className="bg-gradient-to-br from-orange-500 to-orange-600 rounded-lg shadow-lg p-6 text-white">
                    <div className="flex items-center justify-between mb-2">
                        <span className="text-sm font-medium opacity-90">Doanh thu đồ ăn</span>
                        <ShoppingBag className="w-6 h-6 opacity-80" />
                    </div>
                    <p className="text-3xl font-bold mb-1">{formatShortCurrency(totals.concessionRevenue)}</p>
                    <p className="text-xs opacity-75">{totals.concessionCount} sản phẩm</p>
                </div>

                <div className="bg-gradient-to-br from-purple-500 to-purple-600 rounded-lg shadow-lg p-6 text-white">
                    <div className="flex items-center justify-between mb-2">
                        <span className="text-sm font-medium opacity-90">Tỷ lệ đồ ăn TB</span>
                        <TrendingUp className="w-6 h-6 opacity-80" />
                    </div>
                    <p className="text-3xl font-bold mb-1">
                        {totals.totalRevenue > 0
                            ? ((totals.concessionRevenue / totals.totalRevenue) * 100).toFixed(1)
                            : 0}%
                    </p>
                    <p className="text-xs opacity-75">Trung bình các tháng</p>
                </div>
            </div>

            {/* Charts Section */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
                {/* Revenue Comparison Chart */}
                <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div className="flex items-center mb-4">
                        <BarChart3 className="w-5 h-5 text-blue-600 mr-2" />
                        <h3 className="text-lg font-semibold text-gray-900">So sánh doanh thu theo tháng</h3>
                    </div>
                    <ResponsiveContainer width="100%" height={300}>
                        <BarChart data={chartData}>
                            <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                            <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                            <YAxis tick={{ fontSize: 12 }} tickFormatter={formatShortCurrency} />
                            <Tooltip
                                formatter={(value: number) => formatCurrency(value)}
                                contentStyle={{ borderRadius: '8px', border: '1px solid #e5e7eb' }}
                            />
                            <Legend />
                            <Bar dataKey="Doanh thu vé" fill="#3b82f6" radius={[8, 8, 0, 0]} />
                            <Bar dataKey="Doanh thu đồ ăn" fill="#f97316" radius={[8, 8, 0, 0]} />
                        </BarChart>
                    </ResponsiveContainer>
                </div>

                {/* Revenue Distribution Pie Chart */}
                <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div className="flex items-center mb-4">
                        <PieChart className="w-5 h-5 text-purple-600 mr-2" />
                        <h3 className="text-lg font-semibold text-gray-900">Phân bổ doanh thu tổng thể</h3>
                    </div>
                    <ResponsiveContainer width="100%" height={300}>
                        <RechartsPieChart>
                            <Pie
                                data={pieData}
                                cx="50%"
                                cy="50%"
                                labelLine={false}
                                label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(1)}%`}
                                outerRadius={100}
                                fill="#8884d8"
                                dataKey="value"
                            >
                                {pieData.map((entry, index) => (
                                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                                ))}
                            </Pie>
                            <Tooltip formatter={(value: number) => formatCurrency(value)} />
                        </RechartsPieChart>
                    </ResponsiveContainer>
                </div>

                {/* Growth Rate Chart */}
                <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div className="flex items-center mb-4">
                        <Activity className="w-5 h-5 text-green-600 mr-2" />
                        <h3 className="text-lg font-semibold text-gray-900">Tốc độ tăng trưởng (%)</h3>
                    </div>
                    <ResponsiveContainer width="100%" height={300}>
                        <LineChart data={growthData}>
                            <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                            <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                            <YAxis tick={{ fontSize: 12 }} />
                            <Tooltip
                                formatter={(value: number) => `${value}%`}
                                contentStyle={{ borderRadius: '8px', border: '1px solid #e5e7eb' }}
                            />
                            <Legend />
                            <Line
                                type="monotone"
                                dataKey="growth"
                                stroke="#10b981"
                                strokeWidth={3}
                                dot={{ fill: '#10b981', r: 6 }}
                                name="Tăng trưởng"
                            />
                        </LineChart>
                    </ResponsiveContainer>
                </div>

                {/* Average Price Comparison */}
                <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div className="flex items-center mb-4">
                        <TrendingUp className="w-5 h-5 text-orange-600 mr-2" />
                        <h3 className="text-lg font-semibold text-gray-900">Giá trung bình</h3>
                    </div>
                    <ResponsiveContainer width="100%" height={300}>
                        <LineChart data={avgTicketPriceData}>
                            <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                            <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                            <YAxis tick={{ fontSize: 12 }} tickFormatter={formatShortCurrency} />
                            <Tooltip
                                formatter={(value: number) => formatCurrency(value)}
                                contentStyle={{ borderRadius: '8px', border: '1px solid #e5e7eb' }}
                            />
                            <Legend />
                            <Line
                                type="monotone"
                                dataKey="Giá vé TB"
                                stroke="#3b82f6"
                                strokeWidth={2}
                                dot={{ fill: '#3b82f6', r: 5 }}
                            />
                            <Line
                                type="monotone"
                                dataKey="Giá đồ ăn TB"
                                stroke="#f97316"
                                strokeWidth={2}
                                dot={{ fill: '#f97316', r: 5 }}
                            />
                        </LineChart>
                    </ResponsiveContainer>
                </div>
            </div>

            {/* Data Table */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
                <div className="px-6 py-4 bg-gray-50 border-b border-gray-200">
                    <h3 className="text-lg font-semibold text-gray-900">Chi tiết doanh thu theo tháng</h3>
                </div>
                <div className="overflow-x-auto">
                    <table className="w-full">
                        <thead className="bg-gray-50 border-b border-gray-200">
                            <tr>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Tháng
                                </th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Doanh thu vé
                                </th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Số vé
                                </th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Giá TB/Vé
                                </th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Doanh thu đồ ăn
                                </th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Số món
                                </th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Giá TB/Món
                                </th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Tổng DT
                                </th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Tỷ lệ đồ ăn
                                </th>
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            {sortedData.map((row, index) => (
                                <tr key={index} className="hover:bg-gray-50 transition-colors">
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <div className="flex items-center">
                                            <Calendar className="w-4 h-4 text-gray-400 mr-2" />
                                            <span className="text-sm font-medium text-gray-900">
                                                {formatMonth(row.month)}
                                            </span>
                                        </div>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-gray-900 font-medium">
                                        {formatCurrency(row.ticketRevenue)}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-gray-600">
                                        {row.ticketCount}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-gray-600">
                                        {formatCurrency(row.averageTicketPrice)}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-gray-900 font-medium">
                                        {formatCurrency(row.concessionRevenue)}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-gray-600">
                                        {row.concessionCount}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-gray-600">
                                        {formatCurrency(row.averageConcessionPrice)}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-bold text-green-600">
                                        {formatCurrency(row.totalRevenue)}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-right">
                                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${row.concessionRatioPercent > 50
                                                ? 'bg-green-100 text-green-800'
                                                : row.concessionRatioPercent > 30
                                                    ? 'bg-yellow-100 text-yellow-800'
                                                    : 'bg-red-100 text-red-800'
                                            }`}>
                                            {row.concessionRatioPercent.toFixed(2)}%
                                        </span>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                        <tfoot className="bg-gray-100 border-t-2 border-gray-300">
                            <tr>
                                <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-gray-900">
                                    TỔNG CỘNG
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-bold text-gray-900">
                                    {formatCurrency(totals.ticketRevenue)}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-bold text-gray-900">
                                    {totals.ticketCount}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-gray-600">
                                    {totals.ticketCount > 0
                                        ? formatCurrency(totals.ticketRevenue / totals.ticketCount)
                                        : formatCurrency(0)
                                    }
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-bold text-gray-900">
                                    {formatCurrency(totals.concessionRevenue)}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-bold text-gray-900">
                                    {totals.concessionCount}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-gray-600">
                                    {totals.concessionCount > 0
                                        ? formatCurrency(totals.concessionRevenue / totals.concessionCount)
                                        : formatCurrency(0)
                                    }
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-bold text-green-700">
                                    {formatCurrency(totals.totalRevenue)}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-bold text-gray-900">
                                    {totals.totalRevenue > 0
                                        ? ((totals.concessionRevenue / totals.totalRevenue) * 100).toFixed(2)
                                        : 0}%
                                </td>
                            </tr>
                        </tfoot>
                    </table>
                </div>
            </div>
        </div>
    );
};

export default ManagerMonthlyReport;