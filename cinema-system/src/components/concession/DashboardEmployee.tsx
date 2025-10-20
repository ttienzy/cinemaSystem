import React, { useEffect, useMemo } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import { ShoppingCart, DollarSign, ArrowUpRight, ArrowDownRight, TrendingUp } from 'lucide-react';
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { getInventory, getReportRevenueAndStock } from '../../store/slices/inventorySlice';
import { getStaffWorkingInfo } from '../../store/slices/staffSlice';
import type { InventoryItem } from '../../types/inventory.types';
import { getEmailFromToken } from '../../utils/decodeTokenAndGetUser';





interface SummaryCardProps {
    title: string;
    value: string;
    icon: React.ReactNode;
    trend: string;
    isPositive: boolean;
    loading?: boolean;
}

// --- Utility Functions ---
const formatVND = (value: number): string => {
    if (isNaN(value) || !isFinite(value)) return '0 ₫';
    return `${Math.round(value).toLocaleString('vi-VN')} ₫`;
};

const formatChartValue = (value: number): string => {
    if (value >= 1000000) return `${(value / 1000000).toFixed(1)}M`;
    if (value >= 1000) return `${(value / 1000).toFixed(1)}K`;
    return value.toString();
};

const calculateTrend = (current: number, previous: number): { percentage: number; isPositive: boolean } => {
    if (previous === 0) {
        // If previous was 0, any increase is positive, can't calculate percentage
        return { percentage: current > 0 ? 100 : 0, isPositive: current >= 0 };
    }
    const percentage = ((current - previous) / previous) * 100;
    return {
        percentage: Math.abs(percentage),
        isPositive: percentage >= 0,
    };
};


// --- Sub-components ---

const SkeletonCard: React.FC = () => (
    <div className="bg-white p-6 rounded-lg shadow-md animate-pulse">
        <div className="flex items-center justify-between mb-4">
            <div className="h-4 bg-gray-300 rounded w-24"></div>
            <div className="w-6 h-6 bg-gray-300 rounded"></div>
        </div>
        <div className="h-8 bg-gray-300 rounded w-32 mb-2"></div>
        <div className="h-4 bg-gray-300 rounded w-20"></div>
    </div>
);

const ErrorCard: React.FC<{ error: string }> = ({ error }) => (
    <div className="bg-red-50 border border-red-200 rounded-lg p-6">
        <h3 className="text-red-800 font-medium">Lỗi tải dữ liệu</h3>
        <p className="text-red-600 text-sm mt-1">{error}</p>
    </div>
);

const SummaryCard: React.FC<SummaryCardProps> = ({ title, value, icon, trend, isPositive, loading = false }) => {
    if (loading) return <SkeletonCard />;

    return (
        <div className="bg-white p-6 rounded-lg shadow-md hover:shadow-lg transition-shadow duration-200">
            <div className="flex items-center justify-between mb-4">
                <p className="text-sm font-medium text-gray-500 truncate">{title}</p>
                <div className="flex-shrink-0">{icon}</div>
            </div>
            <div>
                <h3 className="text-2xl lg:text-3xl font-bold text-gray-800 truncate" title={value}>
                    {value}
                </h3>
                <div className={`flex items-center text-sm mt-2 ${isPositive ? 'text-green-600' : 'text-red-600'}`}>
                    {isPositive ? <ArrowUpRight className="w-4 h-4 mr-1 flex-shrink-0" /> : <ArrowDownRight className="w-4 h-4 mr-1 flex-shrink-0" />}
                    <span className="truncate" title={trend}>{trend}</span>
                </div>
            </div>
        </div>
    );
};

const CustomTooltip: React.FC<any> = ({ active, payload, label }) => {
    if (!active || !payload || !payload.length) return null;

    return (
        <div className="bg-white p-3 rounded-lg shadow-lg border border-gray-200">
            <p className="font-semibold text-gray-700 mb-2">{`Ngày: ${label}`}</p>
            {payload.map((entry: any, index: number) => (
                <p key={index} style={{ color: entry.color }} className="text-sm">
                    {`${entry.name}: ${entry.name === 'Doanh thu' ? formatVND(entry.value) : entry.value.toLocaleString('vi-VN')}`}
                </p>
            ))}
        </div>
    );
};

// --- Main Dashboard Component ---

const DashboardEmployee: React.FC = () => {
    const dispatch = useAppDispatch();

    const { reportEmployee, items, loading, error } = useAppSelector(state => state.inventory);

    const cinemaId = useMemo(() => localStorage.getItem('cinemaId'), []);



    useEffect(() => {
        if (!cinemaId) {
            dispatch(getStaffWorkingInfo(getEmailFromToken()));
            return;
        }
        dispatch(getReportRevenueAndStock(cinemaId));
        dispatch(getInventory(cinemaId));
    }, [dispatch, cinemaId]);

    const dashboardMetrics = useMemo(() => {
        const defaultMetrics = {
            totalRevenue: 0,
            totalTransactions: 0,
            avgRevenuePerTransaction: 0,
            topStockItem: null as InventoryItem | null,
            revenueTrend: { text: 'Không có dữ liệu', isPositive: true },
            transactionTrend: { text: 'Không có dữ liệu', isPositive: true },
            avgRevenueTrend: { text: 'Không có dữ liệu', isPositive: true },
        };

        if (!reportEmployee || !items) return defaultMetrics;

        const totalRevenue = reportEmployee.reduce((acc, item) => acc + (item.totalRevenue || 0), 0);
        const totalTransactions = reportEmployee.reduce((acc, item) => acc + (item.totalTransactions || 0), 0);
        const avgRevenuePerTransaction = totalTransactions > 0 ? totalRevenue / totalTransactions : 0;
        const topStockItem = items.length > 0 ? items.reduce((prev, current) => (prev.currentStock > current.currentStock) ? prev : current) : null;

        // Trend calculation requires at least 2 days of data
        if (reportEmployee.length >= 2) {
            const recentReport = reportEmployee[reportEmployee.length - 1];
            const previousReport = reportEmployee[reportEmployee.length - 2];

            // Revenue Trend
            const revenueTrendCalc = calculateTrend(recentReport.totalRevenue, previousReport.totalRevenue);
            defaultMetrics.revenueTrend = {
                text: `${revenueTrendCalc.percentage.toFixed(1)}% so với ngày trước`,
                isPositive: revenueTrendCalc.isPositive,
            };

            // Transaction Trend
            const transactionTrendCalc = calculateTrend(recentReport.totalTransactions, previousReport.totalTransactions);
            defaultMetrics.transactionTrend = {
                text: `${transactionTrendCalc.percentage.toFixed(1)}% so với ngày trước`,
                isPositive: transactionTrendCalc.isPositive,
            };

            // Average Revenue Per Transaction Trend
            const recentAvg = recentReport.totalTransactions > 0 ? recentReport.totalRevenue / recentReport.totalTransactions : 0;
            const previousAvg = previousReport.totalTransactions > 0 ? previousReport.totalRevenue / previousReport.totalTransactions : 0;
            const avgRevenueTrendCalc = calculateTrend(recentAvg, previousAvg);
            defaultMetrics.avgRevenueTrend = {
                text: `${avgRevenueTrendCalc.percentage.toFixed(1)}% so với ngày trước`,
                isPositive: avgRevenueTrendCalc.isPositive,
            };
        }

        return { ...defaultMetrics, totalRevenue, totalTransactions, avgRevenuePerTransaction, topStockItem };
    }, [reportEmployee, items]);

    const chartData = useMemo(() =>
        reportEmployee?.map(item => ({
            ...item,
            saleDate: new Date(item.saleDate).toLocaleDateString('vi-VN', { month: 'short', day: 'numeric' })
        })) || []
        , [reportEmployee]);

    const sortedItems = useMemo(() =>
        [...(items || [])].sort((a, b) => b.currentStock - a.currentStock)
        , [items]);

    if (error) return <div className="bg-gray-100 min-h-screen p-8"><ErrorCard error={error} /></div>;
    if (!cinemaId) return <div className="bg-gray-100 min-h-screen p-8"><ErrorCard error="Không tìm thấy ID rạp chiếu. Vui lòng đăng nhập lại." /></div>;

    return (
        <div className="bg-gray-100 min-h-screen p-4 md:p-8 font-sans">
            <header className="mb-6 md:mb-8">
                <h1 className="text-2xl md:text-3xl font-bold text-gray-800">Dashboard</h1>
                <p className="text-gray-500 mt-1">Chào mừng trở lại, đây là báo cáo của bạn.</p>
            </header>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 md:gap-6 mb-6 md:mb-8">
                <SummaryCard
                    title="Tổng doanh thu"
                    value={formatVND(dashboardMetrics.totalRevenue)}
                    icon={<DollarSign className="w-6 h-6 text-green-500" />}
                    trend={dashboardMetrics.revenueTrend.text}
                    isPositive={dashboardMetrics.revenueTrend.isPositive}
                    loading={loading}
                />
                <SummaryCard
                    title="Tổng giao dịch"
                    value={dashboardMetrics.totalTransactions.toLocaleString('vi-VN')}
                    icon={<ShoppingCart className="w-6 h-6 text-blue-500" />}
                    trend={dashboardMetrics.transactionTrend.text}
                    isPositive={dashboardMetrics.transactionTrend.isPositive}
                    loading={loading}
                />
                <SummaryCard
                    title="Doanh thu TB/GD"
                    value={formatVND(dashboardMetrics.avgRevenuePerTransaction)}
                    icon={<TrendingUp className="w-6 h-6 text-yellow-500" />}
                    trend={dashboardMetrics.avgRevenueTrend.text}
                    isPositive={dashboardMetrics.avgRevenueTrend.isPositive}
                    loading={loading}
                />
                <SummaryCard
                    title="SP tồn kho nhiều nhất"
                    value={dashboardMetrics.topStockItem?.itemName || 'N/A'}
                    icon={<ShoppingCart className="w-6 h-6 text-purple-500" />}
                    trend={`${dashboardMetrics.topStockItem?.currentStock.toLocaleString('vi-VN') || 0} sản phẩm`}
                    isPositive={true}
                    loading={loading}
                />
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 md:gap-8">
                <div className="lg:col-span-2 bg-white p-4 md:p-6 rounded-lg shadow-md">
                    <h2 className="text-lg md:text-xl font-semibold text-gray-700 mb-4">Doanh thu và Giao dịch (10 ngày gần nhất)</h2>
                    {loading ? (
                        <div className="h-[300px] bg-gray-100 rounded animate-pulse flex items-center justify-center"><span className="text-gray-500">Đang tải biểu đồ...</span></div>
                    ) : (
                        <ResponsiveContainer width="100%" height={300}>
                            <LineChart data={chartData} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#e0e0e0" />
                                <XAxis dataKey="saleDate" tick={{ fontSize: 12 }} angle={-45} textAnchor="end" height={60} />
                                <YAxis yAxisId="left" tickFormatter={formatChartValue} tick={{ fontSize: 12 }} />
                                <YAxis yAxisId="right" orientation="right" tick={{ fontSize: 12 }} />
                                <Tooltip content={<CustomTooltip />} />
                                <Legend />
                                <Line yAxisId="left" type="monotone" dataKey="totalRevenue" name="Doanh thu" stroke="#8884d8" strokeWidth={2} activeDot={{ r: 6 }} />
                                <Line yAxisId="right" type="monotone" dataKey="totalTransactions" name="Giao dịch" stroke="#82ca9d" strokeWidth={2} activeDot={{ r: 6 }} />
                            </LineChart>
                        </ResponsiveContainer>
                    )}
                </div>

                <div className="bg-white p-4 md:p-6 rounded-lg shadow-md">
                    <h2 className="text-lg md:text-xl font-semibold text-gray-700 mb-4">Tình hình tồn kho</h2>
                    {loading ? (
                        <div className="space-y-3">{[...Array(5)].map((_, i) => (<div key={i} className="flex items-center space-x-3 animate-pulse"><div className="w-8 h-8 bg-gray-300 rounded-full"></div><div className="flex-1"><div className="h-4 bg-gray-300 rounded w-3/4"></div><div className="h-3 bg-gray-300 rounded w-1/2 mt-1"></div></div><div className="h-4 bg-gray-300 rounded w-16"></div></div>))}</div>
                    ) : (
                        <div className="overflow-y-auto max-h-[300px]">
                            <table className="w-full text-sm text-left text-gray-500">
                                <thead className="text-xs text-gray-700 uppercase bg-gray-50 sticky top-0 z-10">
                                    <tr>
                                        <th scope="col" className="px-3 py-3">Sản phẩm</th>
                                        <th scope="col" className="px-3 py-3 text-right">Tồn kho</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {sortedItems.map((item) => (
                                        <tr key={item.id} className="bg-white border-b hover:bg-gray-50 transition-colors">
                                            <td className="px-3 py-3">
                                                <div className="flex items-center"><img src={item.image} alt={item.itemName} className="w-8 h-8 rounded-full mr-3 object-cover flex-shrink-0" />
                                                    <div className="min-w-0">
                                                        <p className="font-medium text-gray-900 truncate" title={item.itemName}>{item.itemName}</p>
                                                        <p className="text-xs text-gray-400 truncate" title={item.itemCategory}>{item.itemCategory}</p>
                                                    </div>
                                                </div>
                                            </td>
                                            <td className="px-3 py-3 text-right font-medium">
                                                <span className={`${item.currentStock < 10 ? 'text-red-600' : 'text-gray-800'}`}>{item.currentStock.toLocaleString('vi-VN')}</span>
                                            </td>
                                        </tr>
                                    ))}
                                    {sortedItems.length === 0 && (<tr><td colSpan={2} className="px-3 py-8 text-center text-gray-500">Không có dữ liệu tồn kho</td></tr>)}
                                </tbody>
                            </table>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default DashboardEmployee;