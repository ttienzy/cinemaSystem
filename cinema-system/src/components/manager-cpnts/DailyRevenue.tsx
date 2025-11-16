// src/pages/dashboard/manager/DailyRevenue.tsx
import React, { useState } from 'react';
import { Calendar, TrendingUp, Ticket, ShoppingBag, DollarSign, Package } from 'lucide-react';

interface DailyRevenueData {
    date: string;
    ticketRevenue: number;
    ticketCount: number;
    averageTicketPrice: number;
    concessionRevenue: number;
    concessionCount: number;
    averageConcessionPrice: number;
    totalRevenue: number;
    concessionRatioPercent: number;
}

const MOCK_DATA: DailyRevenueData[] = [
    {
        date: "2025-09-20T00:00:00",
        ticketRevenue: 560000,
        ticketCount: 8,
        averageTicketPrice: 70000,
        concessionRevenue: 676000,
        concessionCount: 16,
        averageConcessionPrice: 42250,
        totalRevenue: 1236000,
        concessionRatioPercent: 54.69
    },
    {
        date: "2025-09-22T00:00:00",
        ticketRevenue: 0,
        ticketCount: 0,
        averageTicketPrice: 0,
        concessionRevenue: 765000,
        concessionCount: 15,
        averageConcessionPrice: 51000,
        totalRevenue: 765000,
        concessionRatioPercent: 100
    },
    {
        date: "2025-09-24T00:00:00",
        ticketRevenue: 0,
        ticketCount: 0,
        averageTicketPrice: 0,
        concessionRevenue: 50000,
        concessionCount: 2,
        averageConcessionPrice: 25000,
        totalRevenue: 50000,
        concessionRatioPercent: 100
    },
    {
        date: "2025-09-21T00:00:00",
        ticketRevenue: 1200000,
        ticketCount: 15,
        averageTicketPrice: 80000,
        concessionRevenue: 890000,
        concessionCount: 20,
        averageConcessionPrice: 44500,
        totalRevenue: 2090000,
        concessionRatioPercent: 42.58
    },
    {
        date: "2025-09-23T00:00:00",
        ticketRevenue: 850000,
        ticketCount: 12,
        averageTicketPrice: 70833,
        concessionRevenue: 420000,
        concessionCount: 10,
        averageConcessionPrice: 42000,
        totalRevenue: 1270000,
        concessionRatioPercent: 33.07
    }
];

const ManagerDailyRevenue: React.FC = () => {
    const [cinemaldPath] = useState("779E27FD-DCF4-4F63-9B55-08DDE64DEEI");
    const [cinemaldQuery] = useState("779E27FD-DCF4-4F63-9B55-08DDE64DEEI");
    const [startDate, setStartDate] = useState("2025-09-01");
    const [endDate, setEndDate] = useState("2025-09-30");
    const [revenueData] = useState<DailyRevenueData[]>(MOCK_DATA);

    const formatCurrency = (amount: number) => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
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
    const sortedData = [...revenueData].sort((a, b) =>
        new Date(a.date).getTime() - new Date(b.date).getTime()
    );

    return (
        <div className="p-6 bg-gray-50 min-h-screen">
            {/* Header */}
            <div className="mb-6">
                <h1 className="text-2xl font-bold text-gray-900 mb-2">Báo cáo doanh thu theo ngày</h1>
                <p className="text-gray-600">Theo dõi doanh thu vé và đồ ăn hàng ngày</p>
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
                <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div className="flex items-center justify-between mb-2">
                        <span className="text-sm font-medium text-gray-600">Tổng doanh thu</span>
                        <DollarSign className="w-5 h-5 text-green-600" />
                    </div>
                    <p className="text-2xl font-bold text-gray-900">{formatCurrency(totals.totalRevenue)}</p>
                </div>

                <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div className="flex items-center justify-between mb-2">
                        <span className="text-sm font-medium text-gray-600">Doanh thu vé</span>
                        <Ticket className="w-5 h-5 text-blue-600" />
                    </div>
                    <p className="text-2xl font-bold text-gray-900">{formatCurrency(totals.ticketRevenue)}</p>
                    <p className="text-xs text-gray-500 mt-1">{totals.ticketCount} vé</p>
                </div>

                <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div className="flex items-center justify-between mb-2">
                        <span className="text-sm font-medium text-gray-600">Doanh thu đồ ăn</span>
                        <ShoppingBag className="w-5 h-5 text-orange-600" />
                    </div>
                    <p className="text-2xl font-bold text-gray-900">{formatCurrency(totals.concessionRevenue)}</p>
                    <p className="text-xs text-gray-500 mt-1">{totals.concessionCount} sản phẩm</p>
                </div>

                <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <div className="flex items-center justify-between mb-2">
                        <span className="text-sm font-medium text-gray-600">Tỷ lệ đồ ăn TB</span>
                        <TrendingUp className="w-5 h-5 text-purple-600" />
                    </div>
                    <p className="text-2xl font-bold text-gray-900">
                        {totals.totalRevenue > 0
                            ? ((totals.concessionRevenue / totals.totalRevenue) * 100).toFixed(2)
                            : 0}%
                    </p>
                </div>
            </div>

            {/* Data Table */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full">
                        <thead className="bg-gray-50 border-b border-gray-200">
                            <tr>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Ngày
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
                                                {formatDate(row.date)}
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

export default ManagerDailyRevenue;