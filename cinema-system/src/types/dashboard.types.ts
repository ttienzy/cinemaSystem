import { AlertTriangle, BarChart3, Building, Calendar, Film, Home, Monitor, Package, Settings, ShoppingCart, Tags, UserCheck, Users, Wrench } from "lucide-react";
import type { SelectedSeat } from "./showtime.type";

interface MenuItem {
    id: string;
    title: string;
    icon?: React.ComponentType<{ className?: string }>;
    content?: string;
    children?: MenuItem[];
}
interface CartItem {
    staffId: string;
    tickets: TicketCartItem;
    concessions: ConcessionCartItem[];
    paymentMethod: 'cash' | 'card';
}
interface TicketCartItem {
    showtimeId?: string;
    actualStartTime: string;
    actualEndTime: string;
    movieTitle: string;
    selectedSeats: SelectedSeat[];
}
interface ConcessionCartItem {
    itemId: string;
    itemName: string;
    price: number;
    quantity: number;
}
export const menuItems: Record<string, MenuItem[]> = {
    employee: [
        {
            id: 'dashboard',
            title: 'Dashboard',
            icon: Home,
            content: 'Employee Dashboard - Tổng quan công việc hàng ngày'
        },
        {
            id: 'concession-sales',
            title: 'Bán hàng tại quầy',
            icon: ShoppingCart,
            children: [
                {
                    id: 'new-sale',
                    title: 'Tạo đơn hàng mới',
                    content: 'Tạo đơn bán đồ ăn/nước uống mới cho khách hàng'
                },
                {
                    id: 'sales-history',
                    title: 'Lịch sử bán hàng',
                    content: 'Xem lịch sử các đơn hàng đã bán trong ca'
                }
            ]
        },
        {
            id: 'checkin',
            title: 'Check-in khách hàng',
            icon: UserCheck,
            content: 'Check-in khách hàng khi đến xem phim, xác nhận vé'
        },
        {
            id: 'schedule',
            title: 'Lịch làm việc',
            icon: Calendar,
            content: 'Xem lịch làm việc cá nhân, ca trực được phân'
        },
        {
            id: 'equipment-report',
            title: 'Báo cáo sự cố',
            icon: AlertTriangle,
            content: 'Báo cáo sự cố thiết bị, vấn đề kỹ thuật cơ bản'
        }
    ],
    manager: [
        {
            id: 'dashboard',
            title: 'Dashboard',
            icon: Home,
            content: 'Manager Dashboard - Tổng quan quản lý rạp chiếu'
        },
        {
            id: 'showtimes',
            title: 'Quản lý suất chiếu',
            icon: Film,
            children: [
                {
                    id: 'create-showtime',
                    title: 'Tạo suất chiếu mới',
                    content: 'Tạo lịch chiếu phim mới cho các phòng'
                },
                {
                    id: 'manage-showtimes',
                    title: 'Quản lý lịch chiếu',
                    content: 'Chỉnh sửa, hủy các suất chiếu đã lên lịch'
                },
                {
                    id: 'showtime-pricing',
                    title: 'Định giá suất chiếu',
                    content: 'Thiết lập giá vé cho từng loại ghế, suất chiếu'
                }
            ]
        },
        {
            id: 'staff-management',
            title: 'Quản lý nhân viên',
            icon: Users,
            children: [
                {
                    id: 'staff-list',
                    title: 'Danh sách nhân viên',
                    content: 'Xem và quản lý thông tin nhân viên trong rạp'
                },
                {
                    id: 'shift-assignment',
                    title: 'Phân ca làm việc',
                    content: 'Phân công ca trực cho nhân viên theo lịch'
                },
                {
                    id: 'staff-performance',
                    title: 'Đánh giá hiệu suất',
                    content: 'Theo dõi và đánh giá hiệu suất làm việc'
                }
            ]
        },
        {
            id: 'inventory',
            title: 'Quản lý kho hàng',
            icon: Package,
            children: [
                {
                    id: 'inventory-list',
                    title: 'Danh sách hàng hóa',
                    content: 'Quản lý danh sách đồ ăn, nước uống trong kho'
                },
                {
                    id: 'stock-control',
                    title: 'Kiểm soát tồn kho',
                    content: 'Theo dõi số lượng tồn, cảnh báo hết hàng'
                },
                {
                    id: 'restock',
                    title: 'Nhập hàng',
                    content: 'Ghi nhận việc nhập hàng mới từ nhà cung cấp'
                }
            ]
        },
        {
            id: 'equipment',
            title: 'Thiết bị & Bảo trì',
            icon: Wrench,
            children: [
                {
                    id: 'equipment-list',
                    title: 'Danh sách thiết bị',
                    content: 'Quản lý thiết bị chiếu, âm thanh, máy móc'
                },
                {
                    id: 'maintenance-schedule',
                    title: 'Lịch bảo trì',
                    content: 'Lập lịch và theo dõi việc bảo trì thiết bị'
                },
                {
                    id: 'maintenance-history',
                    title: 'Lịch sử bảo trì',
                    content: 'Xem lịch sử các lần bảo trì, sửa chữa'
                }
            ]
        },
        {
            id: 'reports',
            title: 'Báo cáo doanh thu',
            icon: BarChart3,
            children: [
                {
                    id: 'daily-revenue',
                    title: 'Doanh thu hàng ngày',
                    content: 'Báo cáo doanh thu vé và đồ ăn theo ngày'
                },
                {
                    id: 'monthly-report',
                    title: 'Báo cáo tháng',
                    content: 'Tổng hợp doanh thu và hiệu suất theo tháng'
                },
                {
                    id: 'movie-performance',
                    title: 'Hiệu suất phim',
                    content: 'Phân tích doanh thu theo từng bộ phim'
                }
            ]
        }
    ],
    admin: [
        {
            id: 'dashboard',
            title: 'Dashboard',
            icon: Home,
            content: 'Admin Dashboard - Tổng quan toàn hệ thống rạp chiếu'
        },
        {
            id: 'cinema-management',
            title: 'Quản lý rạp',
            icon: Building,
            children: [
                {
                    id: 'cinema-list',
                    title: 'Danh sách rạp',
                    content: 'Quản lý thông tin các rạp chiếu trong hệ thống'
                },
                {
                    id: 'cinema-create',
                    title: 'Tạo rạp mới',
                    content: 'Thêm rạp chiếu mới vào hệ thống'
                },
                {
                    id: 'cinema-settings',
                    title: 'Cấu hình rạp',
                    content: 'Thiết lập thông tin, trạng thái hoạt động của rạp'
                }
            ]
        },
        {
            id: 'screen-seat-management',
            title: 'Phòng chiếu & Ghế',
            icon: Monitor,
            children: [
                {
                    id: 'screen-management',
                    title: 'Quản lý phòng chiếu',
                    content: 'Tạo và quản lý các phòng chiếu trong rạp'
                },
                {
                    id: 'seat-layout',
                    title: 'Bố trí ghế ngồi',
                    content: 'Thiết kế layout ghế cho từng phòng chiếu'
                },
                {
                    id: 'seat-types',
                    title: 'Loại ghế',
                    content: 'Quản lý các loại ghế: Standard, VIP, Couple...'
                }
            ]
        },
        {
            id: 'movie-management',
            title: 'Quản lý phim',
            icon: Film,
            children: [
                {
                    id: 'movie-list',
                    title: 'Danh sách phim',
                    content: 'Quản lý thông tin các bộ phim trong hệ thống'
                },
                {
                    id: 'movie-create',
                    title: 'Thêm phim mới',
                    content: 'Thêm bộ phim mới với đầy đủ thông tin'
                },
                {
                    id: 'movie-genres',
                    title: 'Thể loại phim',
                    content: 'Quản lý danh mục thể loại: Hành động, Kinh dị...'
                },
                {
                    id: 'movie-cast-crew',
                    title: 'Diễn viên & Đạo diễn',
                    content: 'Quản lý thông tin cast và crew của phim'
                }
            ]
        },
        {
            id: 'pricing-system',
            title: 'Hệ thống định giá',
            icon: Tags,
            children: [
                {
                    id: 'pricing-tiers',
                    title: 'Bậc giá',
                    content: 'Cấu hình các bậc giá: Standard, Peak, Holiday...'
                },
                {
                    id: 'seat-pricing',
                    title: 'Giá theo loại ghế',
                    content: 'Thiết lập hệ số giá cho từng loại ghế'
                },
                {
                    id: 'time-slots',
                    title: 'Khung giờ chiếu',
                    content: 'Quản lý các khung thời gian chiếu phim'
                }
            ]
        },
        {
            id: 'system-reports',
            title: 'Báo cáo hệ thống',
            icon: BarChart3,
            children: [
                {
                    id: 'overall-revenue',
                    title: 'Doanh thu tổng thể',
                    content: 'Báo cáo doanh thu toàn hệ thống rạp'
                },
                {
                    id: 'cinema-performance',
                    title: 'Hiệu suất các rạp',
                    content: 'So sánh hiệu suất kinh doanh giữa các rạp'
                },
                {
                    id: 'system-analytics',
                    title: 'Phân tích hệ thống',
                    content: 'Phân tích xu hướng, thống kê sử dụng hệ thống'
                }
            ]
        },
        {
            id: 'system-config',
            title: 'Cấu hình hệ thống',
            icon: Settings,
            children: [
                {
                    id: 'general-settings',
                    title: 'Cài đặt chung',
                    content: 'Cấu hình các thông số cơ bản của hệ thống'
                },
                {
                    id: 'user-roles',
                    title: 'Phân quyền người dùng',
                    content: 'Quản lý vai trò và quyền hạn người dùng'
                },
                {
                    id: 'system-backup',
                    title: 'Sao lưu dữ liệu',
                    content: 'Quản lý việc backup và restore dữ liệu'
                }
            ]
        }
    ]
};

export type { MenuItem, CartItem, TicketCartItem, ConcessionCartItem };
