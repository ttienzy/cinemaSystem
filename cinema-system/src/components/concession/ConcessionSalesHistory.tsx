import React, { useState, useEffect, useMemo } from 'react';
import { Search, Calendar, Package, Ticket, Clock, Filter } from 'lucide-react';
import { useAppDispatch, useAppSelector } from '../../hooks/redux';
import { getConcessionSalesHistory } from '../../store/slices/inventorySlice';
import type { ConcessionSaleQueryParameters } from '../../types/inventory.types';

const ConcessionSalesHistory: React.FC = () => {
    const dispatch = useAppDispatch();
    const { concessionHistory, loading, error } = useAppSelector(state => state.inventory);
    const cinemaId = useMemo(() => localStorage.getItem('cinemaId'), []);

    if (!cinemaId) {
        alert('No cinema assigned. Please contact admin.');
        return <div className="p-6">No cinema assigned. Please contact admin.</div>;
    }

    // Get today's date in YYYY-MM-DD format
    const getTodayDate = () => {
        return new Date().toISOString().split('T')[0];
    };

    // Get 30 days ago date in YYYY-MM-DD format
    const getFromDate = () => {
        const date = new Date();
        date.setDate(date.getDate() - 30);
        return date.toISOString().split('T')[0];
    };

    const [queryParams, setQueryParams] = useState<ConcessionSaleQueryParameters>({
        fromDate: getFromDate(),
        toDate: getTodayDate(),
        paymentMethod: '',
        pageIndex: 1,
        pageSize: 5
    });

    // Load data when component mounts or when query params change
    useEffect(() => {
        fetchSalesHistory();
    }, [dispatch, cinemaId, queryParams.pageIndex]);

    const fetchSalesHistory = () => {
        dispatch(getConcessionSalesHistory({ cinemaId, queryParams }));
    };
    console.log('Concession History:', concessionHistory);
    const formatCurrency = (amount: number) => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount).replace('$', ''); // Remove any $ symbol if present, though VND uses ₫
    };

    const formatDateTime = (dateString: string) => {
        return new Date(dateString).toLocaleString('vi-VN', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const handleSearch = () => {
        // Reset to first page when searching
        const updatedQuery = { ...queryParams, pageIndex: 1 };
        setQueryParams(updatedQuery);
        dispatch(getConcessionSalesHistory({ cinemaId, queryParams: updatedQuery }));
    };

    const handleFromDateChange = (date: string) => {
        setQueryParams(prev => ({ ...prev, fromDate: date, pageIndex: 1 }));
    };

    const handleToDateChange = (date: string) => {
        setQueryParams(prev => ({ ...prev, toDate: date, pageIndex: 1 }));
    };

    const handlePaymentMethodChange = (method: string) => {
        setQueryParams(prev => ({ ...prev, paymentMethod: method, pageIndex: 1 }));
    };

    const handlePageChange = (page: number) => {
        setQueryParams(prev => ({ ...prev, pageIndex: page }));
    };

    const handlePrevPage = () => {
        if (concessionHistory.pagination.hasPreviousPage) {
            handlePageChange(Math.max(1, queryParams.pageIndex - 1));
        }
    };

    const handleNextPage = () => {
        if (concessionHistory.pagination.hasNextPage) {
            handlePageChange(queryParams.pageIndex + 1);
        }
    };

    const resetFilters = () => {
        const resetQuery = {
            fromDate: getFromDate(),
            toDate: getTodayDate(),
            paymentMethod: '',
            pageIndex: 1,
            pageSize: 10
        };
        setQueryParams(resetQuery);
        dispatch(getConcessionSalesHistory({ cinemaId, queryParams: resetQuery }));
    };

    return (
        <div className="p-6 max-w-7xl mx-auto bg-white">

            {/* Search and Filter Section */}
            <div className="bg-gray-50 p-4 rounded-lg mb-6">
                <div className="grid grid-cols-1 md:grid-cols-6 gap-4">
                    {/* From Date */}
                    <div className="relative">
                        <Calendar className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
                        <input
                            title="Từ ngày"
                            type="date"
                            value={queryParams.fromDate}
                            onChange={(e) => handleFromDateChange(e.target.value)}
                            className="pl-10 w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="Từ ngày"
                        />
                        <label className="absolute -top-2 left-2 bg-gray-50 px-1 text-xs text-gray-500">
                            Từ ngày
                        </label>
                    </div>

                    {/* To Date */}
                    <div className="relative">
                        <Calendar className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
                        <input
                            title="Đến ngày"
                            type="date"
                            value={queryParams.toDate}
                            onChange={(e) => handleToDateChange(e.target.value)}
                            className="pl-10 w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="Đến ngày"
                        />
                        <label className="absolute -top-2 left-2 bg-gray-50 px-1 text-xs text-gray-500">
                            Đến ngày
                        </label>
                    </div>

                    {/* Payment Method Filter */}
                    <div className="relative">
                        <Filter className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
                        <select
                            title="Chọn phương thức thanh toán"
                            value={queryParams.paymentMethod}
                            onChange={(e) => handlePaymentMethodChange(e.target.value)}
                            className="pl-10 w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                        >
                            <option value="">Tất cả phương thức</option>
                            <option value="cash">Tiền mặt</option>
                            <option value="card">Chuyển khoản</option>
                            <option value="vnpay">VNPay</option>
                            <option value="momo">MoMo</option>
                        </select>
                    </div>

                    {/* Search Button */}
                    <button
                        onClick={handleSearch}
                        disabled={loading}
                        className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 disabled:opacity-50 flex items-center justify-center gap-2 transition-colors"
                    >
                        <Search className="h-4 w-4" />
                        {loading ? 'Đang tìm...' : 'Tìm kiếm'}
                    </button>

                    {/* Reset Button */}
                    <button
                        onClick={resetFilters}
                        disabled={loading}
                        className="bg-gray-500 text-white px-4 py-2 rounded-md hover:bg-gray-600 disabled:opacity-50 flex items-center justify-center gap-2 transition-colors"
                    >
                        <Filter className="h-4 w-4" />
                        Đặt lại
                    </button>

                    {/* Total Count */}
                    <div className="text-sm text-gray-600 flex items-center col-span-full md:col-span-1">
                        <Package className="h-4 w-4 mr-2" />
                        Tổng: {concessionHistory?.pagination?.count || 0} giao dịch
                    </div>
                </div>
            </div>

            {/* Date Range Display */}
            <div className="mb-4 text-sm text-gray-600 flex items-center gap-4">
                <Clock className="h-4 w-4" />
                <span>
                    Khoảng thời gian: {queryParams.fromDate ? new Date(queryParams.fromDate).toLocaleDateString('vi-VN') : 'Không xác định'}
                    {' đến '}
                    {queryParams.toDate ? new Date(queryParams.toDate).toLocaleDateString('vi-VN') : 'Không xác định'}
                </span>
            </div>

            {/* Sales List */}
            {loading ? (
                <div className="text-center py-12">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                    <p className="mt-4 text-gray-600">Đang tải dữ liệu...</p>
                </div>
            ) : error ? (
                <div className="text-center py-12">
                    <div className="text-red-500 mb-4">⚠️ Có lỗi xảy ra khi tải dữ liệu</div>
                    <button
                        onClick={fetchSalesHistory}
                        className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700"
                    >
                        Thử lại
                    </button>
                </div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="min-w-full bg-white border border-gray-200 rounded-lg shadow-sm">
                        <thead>
                            <tr className="bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                <th className="px-6 py-3">Đơn hàng</th>
                                <th className="px-6 py-3">Thời gian</th>
                                <th className="px-6 py-3">Sản phẩm</th>
                                <th className="px-6 py-3">Vé</th>
                                <th className="px-6 py-3">Tổng tiền</th>
                                <th className="px-6 py-3">Phương thức</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-200">
                            {concessionHistory?.data?.length > 0 ? (
                                concessionHistory.data.map((sale, index) => (
                                    <tr key={index} className="hover:bg-gray-50">
                                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                                            #{((queryParams.pageIndex - 1) * queryParams.pageSize) + index + 1}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            {formatDateTime(sale.saleDate)}
                                        </td>
                                        <td className="px-6 py-4 text-sm text-gray-500">
                                            <div className="space-y-1">
                                                {sale.items?.map((item, itemIndex) => (
                                                    <div key={itemIndex} className="flex justify-between">
                                                        <span>{item.itemName} (SL: {item.quantity})</span>
                                                        <span>{formatCurrency(item.unitPrice * item.quantity)}</span>
                                                    </div>
                                                ))}
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 text-sm text-gray-500">
                                            {sale.ticketSales ? (
                                                <div className="flex items-center gap-2">
                                                    <Ticket className="h-4 w-4 text-blue-600" />
                                                    <span>Số vé: {sale.ticketSales.totalTickets}</span>
                                                    <span className={`px-2 py-1 rounded-full text-xs ${sale.ticketSales.status === 'Confirmed' ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'}`}>
                                                        {sale.ticketSales.status}
                                                    </span>
                                                </div>
                                            ) : (
                                                '-'
                                            )}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                                            {formatCurrency(sale.totalAmount)}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            <span className="px-2 py-1 rounded-full text-xs bg-blue-100 text-blue-800">
                                                {sale.paymentMethod}
                                            </span>
                                        </td>
                                    </tr>
                                ))
                            ) : (
                                <tr>
                                    <td colSpan={6} className="px-6 py-12 text-center text-gray-500">
                                        Không có dữ liệu trong khoảng thời gian đã chọn
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            )}

            {/* Pagination */}
            {concessionHistory?.pagination?.totalPages > 1 && (
                <div className="mt-6 flex items-center justify-between">
                    <div className="text-sm text-gray-600">
                        Hiển thị trang {concessionHistory.pagination.pageIndex} trong tổng số {concessionHistory.pagination.totalPages} trang
                        ({concessionHistory.pagination.count} giao dịch)
                    </div>

                    <div className="flex items-center gap-2">
                        <button
                            onClick={handlePrevPage}
                            disabled={!concessionHistory.pagination.hasPreviousPage || loading}
                            className="px-3 py-2 text-sm border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 transition-colors"
                        >
                            Trước
                        </button>

                        <span className="px-3 py-2 text-sm text-gray-600">
                            {queryParams.pageIndex} / {concessionHistory.pagination.totalPages}
                        </span>

                        <button
                            onClick={handleNextPage}
                            disabled={!concessionHistory.pagination.hasNextPage || loading}
                            className="px-3 py-2 text-sm border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 transition-colors"
                        >
                            Sau
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default ConcessionSalesHistory;