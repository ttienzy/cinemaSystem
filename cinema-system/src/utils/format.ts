const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
};

const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString('vi-VN', {
        year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit'
    });
};
const formatDateTimeLocal = (date: Date): string => {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
};
const getStatusInfo = (status: string) => {
    switch (status) {
        case 'Pending': return { text: 'Pending Payment', color: 'bg-yellow-100 text-yellow-800' };
        case 'Completed': return { text: 'Completed', color: 'bg-green-100 text-green-800' };
        case 'Cancelled': return { text: 'Cancelled', color: 'bg-red-100 text-red-800' };
        default: return { text: 'Unknown', color: 'bg-gray-100 text-gray-800' };
    }
};

export { formatCurrency, formatDateTime, getStatusInfo, formatDateTimeLocal };