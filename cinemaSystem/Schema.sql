-- =====================================================
-- OLTP DATABASE - CINEMA MANAGEMENT SYSTEM (IMPROVED)
-- =====================================================

-- 1. BẢNG CUSTOMERS - Thông tin khách hàng
-- Đây là bảng QUAN TRỌNG bị thiếu trong schema gốc!
CREATE TABLE [Customers] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NULL,
    [Phone] nvarchar(20) NULL,
    [DateOfBirth] date NULL,
    [Gender] nvarchar(10) NULL, -- Male/Female/Other
    [Address] nvarchar(255) NULL,
    [MembershipTier] nvarchar(50) NULL, -- Bronze/Silver/Gold/Platinum
    [TotalSpent] decimal(18,2) NOT NULL DEFAULT 0,
    [TotalVisits] int NOT NULL DEFAULT 0,
    [RegistrationDate] datetime2 NOT NULL,
    [LastVisitDate] datetime2 NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);
GO

-- 2. BẢNG CINEMAS - Thông tin rạp chiếu phim
CREATE TABLE [Cinemas] (
    [Id] uniqueidentifier NOT NULL,
    [CinemaName] nvarchar(100) NOT NULL,
    [Address] nvarchar(255) NOT NULL,
    [City] nvarchar(100) NOT NULL, -- Thêm để phân tích theo địa lý
    [District] nvarchar(100) NULL,
    [Phone] nvarchar(20) NULL,
    [Email] nvarchar(100) NULL,
    [Image] nvarchar(100) NULL,
    [ManagerName] nvarchar(100) NOT NULL,
    [OpeningDate] datetime2 NULL, -- Thêm ngày khai trương
    [TotalScreens] int NOT NULL DEFAULT 0, -- Số phòng chiếu
    [TotalSeats] int NOT NULL DEFAULT 0, -- Tổng số ghế
    [Status] nvarchar(50) NOT NULL, -- Active/Inactive/UnderMaintenance
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Cinemas] PRIMARY KEY ([Id])
);
GO

-- 3. BẢNG GENRES - Thể loại phim
CREATE TABLE [Genres] (
    [Id] uniqueidentifier NOT NULL,
    [GenreName] nvarchar(50) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Genres] PRIMARY KEY ([Id])
);
GO

-- 4. BẢNG MOVIES - Thông tin phim
CREATE TABLE [Movies] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [OriginalTitle] nvarchar(200) NULL, -- Tên gốc
    [DurationMinutes] int NOT NULL,
    [Rating] nvarchar(10) NOT NULL DEFAULT N'P', -- P/C13/C16/C18
    [Country] nvarchar(100) NULL, -- Quốc gia sản xuất
    [Language] nvarchar(50) NULL, -- Ngôn ngữ
    [Subtitle] nvarchar(50) NULL, -- Phụ đề
    [Director] nvarchar(150) NULL, -- Đạo diễn chính
    [Trailer] nvarchar(500) NULL,
    [ReleaseDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL, -- Ngày kết thúc chiếu
    [Description] nvarchar(2000) NOT NULL,
    [PosterUrl] nvarchar(500) NOT NULL,
    [BackdropUrl] nvarchar(500) NULL, -- Ảnh nền
    [IMDbRating] decimal(3,1) NULL, -- Điểm IMDb
    [Status] nvarchar(50) NOT NULL, -- ComingSoon/NowShowing/Ended
    [IsHot] bit NOT NULL DEFAULT 0, -- Phim hot
    [TotalRevenue] decimal(18,2) NULL, -- Doanh thu tổng
    [TotalTicketsSold] int NULL, -- Tổng vé bán
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Movies] PRIMARY KEY ([Id])
);
GO

-- 5. BẢNG MOVIE_GENRES - Phim và thể loại (Many-to-Many)
CREATE TABLE [MovieGenres] (
    [Id] uniqueidentifier NOT NULL,
    [MovieId] uniqueidentifier NOT NULL,
    [GenreId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_MovieGenres] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MovieGenres_Movies] FOREIGN KEY ([MovieId]) 
        REFERENCES [Movies] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MovieGenres_Genres] FOREIGN KEY ([GenreId]) 
        REFERENCES [Genres] ([Id]) ON DELETE CASCADE
);
GO

-- 6. BẢNG MOVIE_CAST_CREWS - Diễn viên và đoàn làm phim
CREATE TABLE [MovieCastCrews] (
    [Id] uniqueidentifier NOT NULL,
    [MovieId] uniqueidentifier NOT NULL,
    [PersonName] nvarchar(150) NOT NULL,
    [RoleType] nvarchar(50) NOT NULL, -- Actor/Director/Producer/Writer
    [CharacterName] nvarchar(150) NULL, -- Tên nhân vật (nếu là diễn viên)
    [DisplayOrder] int NULL, -- Thứ tự hiển thị
    CONSTRAINT [PK_MovieCastCrews] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MovieCastCrews_Movies] FOREIGN KEY ([MovieId]) 
        REFERENCES [Movies] ([Id]) ON DELETE CASCADE
);
GO

-- 7. BẢNG PRICING_TIERS - Khung giá theo thời gian
CREATE TABLE [PricingTiers] (
    [Id] uniqueidentifier NOT NULL,
    [TierName] nvarchar(100) NOT NULL, -- EarlyBird/Standard/PrimeTime/Weekend
    [Description] nvarchar(255) NULL,
    [Multiplier] decimal(5,2) NOT NULL, -- Hệ số nhân giá (0.8, 1.0, 1.2, 1.5)
    [ValidDays] nvarchar(100) NULL, -- Mon-Thu hoặc Fri-Sun
    [StartTime] time NULL, -- Giờ bắt đầu áp dụng
    [EndTime] time NULL, -- Giờ kết thúc
    [IsActive] bit NOT NULL DEFAULT 1,
    CONSTRAINT [PK_PricingTiers] PRIMARY KEY ([Id])
);
GO

-- 8. BẢNG SEAT_TYPES - Loại ghế
CREATE TABLE [SeatTypes] (
    [Id] uniqueidentifier NOT NULL,
    [TypeName] nvarchar(50) NOT NULL, -- Standard/VIP/Couple/Deluxe
    [Description] nvarchar(255) NULL,
    [BasePrice] decimal(18,2) NOT NULL, -- Giá cơ bản
    [PriceMultiplier] decimal(5,2) NOT NULL, -- Hệ số nhân (1.0, 1.5, 2.0)
    [Color] nvarchar(20) NULL, -- Màu hiển thị trên sơ đồ ghế
    CONSTRAINT [PK_SeatTypes] PRIMARY KEY ([Id])
);
GO

-- 9. BẢNG SCREENS - Phòng chiếu
CREATE TABLE [Screens] (
    [Id] uniqueidentifier NOT NULL,
    [CinemaId] uniqueidentifier NOT NULL,
    [ScreenName] nvarchar(50) NOT NULL, -- Screen 1, Screen 2
    [ScreenNumber] int NOT NULL, -- Số thứ tự phòng
    [Type] nvarchar(50) NOT NULL, -- 2D/3D/4DX/IMAX
    [TotalSeats] int NOT NULL DEFAULT 0,
    [RowCount] int NOT NULL DEFAULT 0, -- Số hàng ghế
    [SeatsPerRow] int NOT NULL DEFAULT 0, -- Số ghế mỗi hàng
    [Status] nvarchar(50) NOT NULL, -- Active/Inactive/UnderMaintenance
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Screens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Screens_Cinemas] FOREIGN KEY ([CinemaId]) 
        REFERENCES [Cinemas] ([Id]) ON DELETE CASCADE
);
GO

-- 10. BẢNG SEATS - Ghế ngồi
CREATE TABLE [Seats] (
    [Id] uniqueidentifier NOT NULL,
    [ScreenId] uniqueidentifier NOT NULL,
    [SeatTypeId] uniqueidentifier NOT NULL,
    [RowName] nvarchar(5) NOT NULL, -- A, B, C...
    [Number] int NOT NULL, -- 1, 2, 3...
    [SeatCode] nvarchar(10) NOT NULL, -- A1, A2, B1... (để hiển thị)
    [IsActive] bit NOT NULL DEFAULT 1,
    [IsBlocked] bit NOT NULL DEFAULT 0, -- Ghế hỏng/bảo trì
    CONSTRAINT [PK_Seats] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Seats_Screens] FOREIGN KEY ([ScreenId]) 
        REFERENCES [Screens] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Seats_SeatTypes] FOREIGN KEY ([SeatTypeId]) 
        REFERENCES [SeatTypes] ([Id]) ON DELETE NO ACTION
);
GO

-- 11. BẢNG SHOWTIMES - Suất chiếu
CREATE TABLE [Showtimes] (
    [Id] uniqueidentifier NOT NULL,
    [CinemaId] uniqueidentifier NOT NULL,
    [MovieId] uniqueidentifier NOT NULL,
    [ScreenId] uniqueidentifier NOT NULL,
    [PricingTierId] uniqueidentifier NOT NULL,
    [ShowDate] date NOT NULL, -- Ngày chiếu
    [StartTime] time NOT NULL, -- Giờ bắt đầu
    [EndTime] time NOT NULL, -- Giờ kết thúc
    [ActualStartTime] datetime2 NULL, -- Giờ chiếu thực tế (có thể trễ)
    [ActualEndTime] datetime2 NULL,
    [TotalSeats] int NOT NULL,
    [AvailableSeats] int NOT NULL,
    [BookedSeats] int NOT NULL DEFAULT 0,
    [Status] nvarchar(50) NOT NULL, -- Scheduled/OnGoing/Completed/Cancelled
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Showtimes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Showtimes_Cinemas] FOREIGN KEY ([CinemaId]) 
        REFERENCES [Cinemas] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Showtimes_Movies] FOREIGN KEY ([MovieId]) 
        REFERENCES [Movies] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Showtimes_Screens] FOREIGN KEY ([ScreenId]) 
        REFERENCES [Screens] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Showtimes_PricingTiers] FOREIGN KEY ([PricingTierId]) 
        REFERENCES [PricingTiers] ([Id]) ON DELETE NO ACTION
);
GO

-- 12. BẢNG SHOWTIME_PRICINGS - Giá vé theo suất chiếu và loại ghế
CREATE TABLE [ShowtimePricings] (
    [Id] uniqueidentifier NOT NULL,
    [ShowtimeId] uniqueidentifier NOT NULL,
    [SeatTypeId] uniqueidentifier NOT NULL,
    [BasePrice] decimal(18,2) NOT NULL, -- Giá gốc loại ghế
    [PricingMultiplier] decimal(5,2) NOT NULL, -- Hệ số từ PricingTier
    [FinalPrice] decimal(18,2) NOT NULL, -- Giá cuối = BasePrice * Multiplier
    CONSTRAINT [PK_ShowtimePricings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShowtimePricings_Showtimes] FOREIGN KEY ([ShowtimeId]) 
        REFERENCES [Showtimes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ShowtimePricings_SeatTypes] FOREIGN KEY ([SeatTypeId]) 
        REFERENCES [SeatTypes] ([Id]) ON DELETE NO ACTION
);
GO

-- 13. BẢNG BOOKINGS - Đặt vé
CREATE TABLE [Bookings] (
    [Id] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL, -- BẮT BUỘC phải có khách hàng
    [ShowtimeId] uniqueidentifier NOT NULL,
    [BookingCode] nvarchar(20) NOT NULL, -- Mã đặt vé (BK20241109001)
    [BookingTime] datetime2 NOT NULL DEFAULT GETDATE(),
    [TotalTickets] int NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [DiscountAmount] decimal(18,2) NULL DEFAULT 0, -- Giảm giá
    [FinalAmount] decimal(18,2) NOT NULL, -- Số tiền thực trả
    [Status] nvarchar(50) NOT NULL, -- Pending/Confirmed/Cancelled/Expired
    [PaymentStatus] nvarchar(50) NOT NULL, -- Unpaid/Paid/Refunded
    [IsCheckedIn] bit NOT NULL DEFAULT 0, -- Đã check-in chưa
    [CheckInTime] datetime2 NULL,
    [BookingChannel] nvarchar(50) NULL, -- Website/Mobile/Counter
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Bookings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Bookings_Customers] FOREIGN KEY ([CustomerId]) 
        REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Bookings_Showtimes] FOREIGN KEY ([ShowtimeId]) 
        REFERENCES [Showtimes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [UQ_BookingCode] UNIQUE ([BookingCode])
);
GO

-- 14. BẢNG BOOKING_TICKETS - Chi tiết vé trong booking
CREATE TABLE [BookingTickets] (
    [Id] uniqueidentifier NOT NULL,
    [BookingId] uniqueidentifier NOT NULL,
    [SeatId] uniqueidentifier NOT NULL,
    [TicketPrice] decimal(18,2) NOT NULL,
    [TicketCode] nvarchar(30) NOT NULL, -- Mã vé riêng (TK20241109001)
    [QRCode] nvarchar(255) NULL, -- QR code để check-in
    CONSTRAINT [PK_BookingTickets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BookingTickets_Bookings] FOREIGN KEY ([BookingId]) 
        REFERENCES [Bookings] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BookingTickets_Seats] FOREIGN KEY ([SeatId]) 
        REFERENCES [Seats] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [UQ_TicketCode] UNIQUE ([TicketCode])
);
GO

-- 15. BẢNG PAYMENTS - Thanh toán
CREATE TABLE [Payments] (
    [Id] uniqueidentifier NOT NULL,
    [BookingId] uniqueidentifier NOT NULL,
    [PaymentMethod] nvarchar(50) NOT NULL, -- Cash/CreditCard/MoMo/ZaloPay/VNPay
    [PaymentProvider] nvarchar(50) NULL, -- Visa/Mastercard/MoMo...
    [Amount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(10) NOT NULL DEFAULT 'VND',
    [TransactionId] nvarchar(100) NULL, -- ID từ payment gateway
    [ReferenceCode] nvarchar(100) NULL,
    [PaymentStatus] nvarchar(50) NOT NULL, -- Success/Failed/Pending/Refunded
    [PaymentTime] datetime2 NOT NULL DEFAULT GETDATE(),
    [RefundAmount] decimal(18,2) NULL,
    [RefundTime] datetime2 NULL,
    [Notes] nvarchar(500) NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_Bookings] FOREIGN KEY ([BookingId]) 
        REFERENCES [Bookings] ([Id]) ON DELETE CASCADE
);
GO

-- 16. BẢNG INVENTORY_ITEMS - Hàng hóa bán kèm (bắp rang bơ, nước...)
CREATE TABLE [InventoryItems] (
    [Id] uniqueidentifier NOT NULL,
    [CinemaId] uniqueidentifier NOT NULL,
    [ItemName] nvarchar(150) NOT NULL,
    [ItemCategory] nvarchar(100) NOT NULL, -- Food/Beverage/Combo/Merchandise
    [Description] nvarchar(500) NULL,
    [CurrentStock] int NOT NULL DEFAULT 0,
    [MinimumStock] int NOT NULL DEFAULT 0,
    [UnitPrice] decimal(18,2) NOT NULL, -- Giá bán
    [CostPrice] decimal(18,2) NOT NULL, -- Giá vốn
    [ImageUrl] nvarchar(255) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_InventoryItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InventoryItems_Cinemas] FOREIGN KEY ([CinemaId]) 
        REFERENCES [Cinemas] ([Id]) ON DELETE NO ACTION
);
GO

-- 17. BẢNG STAFFS - Nhân viên
CREATE TABLE [Staffs] (
    [Id] uniqueidentifier NOT NULL,
    [CinemaId] uniqueidentifier NOT NULL,
    [EmployeeCode] nvarchar(20) NOT NULL, -- Mã nhân viên
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NULL,
    [Phone] nvarchar(20) NULL,
    [DateOfBirth] date NULL,
    [Gender] nvarchar(10) NULL,
    [Address] nvarchar(255) NULL,
    [Position] nvarchar(100) NOT NULL, -- Manager/Cashier/Usher/Technician
    [Department] nvarchar(100) NULL, -- Sales/Operations/Technical
    [HireDate] datetime2 NOT NULL,
    [Salary] decimal(18,2) NOT NULL,
    [Status] nvarchar(50) NOT NULL, -- Active/OnLeave/Resigned
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Staffs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Staffs_Cinemas] FOREIGN KEY ([CinemaId]) 
        REFERENCES [Cinemas] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [UQ_EmployeeCode] UNIQUE ([EmployeeCode])
);
GO

-- 18. BẢNG CONCESSION_SALES - Bán hàng kèm
CREATE TABLE [ConcessionSales] (
    [Id] uniqueidentifier NOT NULL,
    [CinemaId] uniqueidentifier NOT NULL,
    [BookingId] uniqueidentifier NULL, -- Có thể mua không liên kết với booking
    [CustomerId] uniqueidentifier NULL, -- Khách hàng
    [StaffId] uniqueidentifier NOT NULL, -- Nhân viên bán
    [SaleCode] nvarchar(20) NOT NULL, -- Mã hóa đơn
    [SaleDate] datetime2 NOT NULL DEFAULT GETDATE(),
    [TotalAmount] decimal(18,2) NOT NULL,
    [DiscountAmount] decimal(18,2) NULL DEFAULT 0,
    [FinalAmount] decimal(18,2) NOT NULL,
    [PaymentMethod] nvarchar(50) NOT NULL,
    [PaymentStatus] nvarchar(50) NOT NULL, -- Paid/Refunded
    CONSTRAINT [PK_ConcessionSales] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConcessionSales_Cinemas] FOREIGN KEY ([CinemaId]) 
        REFERENCES [Cinemas] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ConcessionSales_Bookings] FOREIGN KEY ([BookingId]) 
        REFERENCES [Bookings] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_ConcessionSales_Customers] FOREIGN KEY ([CustomerId]) 
        REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ConcessionSales_Staffs] FOREIGN KEY ([StaffId]) 
        REFERENCES [Staffs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [UQ_SaleCode] UNIQUE ([SaleCode])
);
GO

-- 19. BẢNG CONCESSION_SALE_ITEMS - Chi tiết hàng bán
CREATE TABLE [ConcessionSaleItems] (
    [Id] uniqueidentifier NOT NULL,
    [ConcessionSaleId] uniqueidentifier NOT NULL,
    [InventoryId] uniqueidentifier NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [TotalPrice] decimal(18,2) NOT NULL, -- Quantity * UnitPrice
    CONSTRAINT [PK_ConcessionSaleItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConcessionSaleItems_ConcessionSales] FOREIGN KEY ([ConcessionSaleId]) 
        REFERENCES [ConcessionSales] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ConcessionSaleItems_InventoryItems] FOREIGN KEY ([InventoryId]) 
        REFERENCES [InventoryItems] ([Id]) ON DELETE NO ACTION
);
GO

-- 20. BẢNG PROMOTIONS - Khuyến mãi
CREATE TABLE [Promotions] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL, -- Mã khuyến mãi
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [DiscountType] nvarchar(20) NOT NULL, -- Percentage/FixedAmount
    [DiscountValue] decimal(18,2) NOT NULL,
    [MinimumSpend] decimal(18,2) NULL, -- Chi tiêu tối thiểu
    [MaximumDiscount] decimal(18,2) NULL, -- Giảm tối đa
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [UsageLimit] int NULL, -- Số lần sử dụng tối đa
    [UsedCount] int NOT NULL DEFAULT 0,
    [ApplicableFor] nvarchar(50) NOT NULL, -- Tickets/Concessions/All
    [Status] nvarchar(50) NOT NULL, -- Active/Expired/Disabled
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Promotions] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_PromotionCode] UNIQUE ([Code])
);
GO

-- 21. BẢNG CUSTOMER_PROMOTIONS - Khuyến mãi đã sử dụng
CREATE TABLE [CustomerPromotions] (
    [Id] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [PromotionId] uniqueidentifier NOT NULL,
    [BookingId] uniqueidentifier NULL,
    [ConcessionSaleId] uniqueidentifier NULL,
    [UsedDate] datetime2 NOT NULL DEFAULT GETDATE(),
    [DiscountAmount] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_CustomerPromotions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerPromotions_Customers] FOREIGN KEY ([CustomerId]) 
        REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CustomerPromotions_Promotions] FOREIGN KEY ([PromotionId]) 
        REFERENCES [Promotions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CustomerPromotions_Bookings] FOREIGN KEY ([BookingId]) 
        REFERENCES [Bookings] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CustomerPromotions_ConcessionSales] FOREIGN KEY ([ConcessionSaleId]) 
        REFERENCES [ConcessionSales] ([Id]) ON DELETE NO ACTION
);
GO

-- =====================================================
-- CREATE INDEXES FOR PERFORMANCE
-- =====================================================

-- Customers
CREATE INDEX [IX_Customers_Email] ON [Customers] ([Email]);
CREATE INDEX [IX_Customers_Phone] ON [Customers] ([Phone]);
CREATE INDEX [IX_Customers_MembershipTier] ON [Customers] ([MembershipTier]);

-- Cinemas
CREATE INDEX [IX_Cinemas_City] ON [Cinemas] ([City]);
CREATE INDEX [IX_Cinemas_Status] ON [Cinemas] ([Status]);

-- Movies
CREATE INDEX [IX_Movies_Status] ON [Movies] ([Status]);
CREATE INDEX [IX_Movies_ReleaseDate] ON [Movies] ([ReleaseDate]);
CREATE INDEX [IX_Movies_IsHot] ON [Movies] ([IsHot]);

-- MovieGenres
CREATE INDEX [IX_MovieGenres_MovieId] ON [MovieGenres] ([MovieId]);
CREATE INDEX [IX_MovieGenres_GenreId] ON [MovieGenres] ([GenreId]);

-- Screens
CREATE INDEX [IX_Screens_CinemaId] ON [Screens] ([CinemaId]);

-- Seats
CREATE INDEX [IX_Seats_ScreenId] ON [Seats] ([ScreenId]);
CREATE INDEX [IX_Seats_SeatTypeId] ON [Seats] ([SeatTypeId]);

-- Showtimes
CREATE INDEX [IX_Showtimes_CinemaId] ON [Showtimes] ([CinemaId]);
CREATE INDEX [IX_Showtimes_MovieId] ON [Showtimes] ([MovieId]);
CREATE INDEX [IX_Showtimes_ScreenId] ON [Showtimes] ([ScreenId]);
CREATE INDEX [IX_Showtimes_ShowDate] ON [Showtimes] ([ShowDate]);
CREATE INDEX [IX_Showtimes_Status] ON [Showtimes] ([Status]);

-- ShowtimePricings
CREATE INDEX [IX_ShowtimePricings_ShowtimeId] ON [ShowtimePricings] ([ShowtimeId]);
CREATE INDEX [IX_ShowtimePricings_SeatTypeId] ON [ShowtimePricings] ([SeatTypeId]);

-- Bookings
CREATE INDEX [IX_Bookings_CustomerId] ON [Bookings] ([CustomerId]);
CREATE INDEX [IX_Bookings_ShowtimeId] ON [Bookings] ([ShowtimeId]);
CREATE INDEX [IX_Bookings_BookingCode] ON [Bookings] ([BookingCode]);
CREATE INDEX [IX_Bookings_BookingTime] ON [Bookings] ([BookingTime]);
CREATE INDEX [IX_Bookings_Status] ON [Bookings] ([Status]);
CREATE INDEX [IX_Bookings_PaymentStatus] ON [Bookings] ([PaymentStatus]);

-- BookingTickets
CREATE INDEX [IX_BookingTickets_BookingId] ON [BookingTickets] ([BookingId]);
CREATE INDEX [IX_BookingTickets_SeatId] ON [BookingTickets] ([SeatId]);

-- Payments
CREATE INDEX [IX_Payments_BookingId] ON [Payments] ([BookingId]);
CREATE INDEX [IX_Payments_PaymentMethod] ON [Payments] ([PaymentMethod]);
CREATE INDEX [IX_Payments_PaymentTime] ON [Payments] ([PaymentTime]);

-- InventoryItems
CREATE INDEX [IX_InventoryItems_CinemaId] ON [InventoryItems] ([CinemaId]);
CREATE INDEX [IX_InventoryItems_ItemCategory] ON [InventoryItems] ([ItemCategory]);

-- Staffs
CREATE INDEX [IX_Staffs_CinemaId] ON [Staffs] ([CinemaId]);
CREATE INDEX [IX_Staffs_EmployeeCode] ON [Staffs] ([EmployeeCode]);
CREATE INDEX [IX_Staffs_Position] ON [Staffs] ([Position]);

-- ConcessionSales
CREATE INDEX [IX_ConcessionSales_CinemaId] ON [ConcessionSales] ([CinemaId]);
CREATE INDEX [IX_ConcessionSales_BookingId] ON [ConcessionSales] ([BookingId]);
CREATE INDEX [IX_ConcessionSales_CustomerId] ON [ConcessionSales] ([CustomerId]);
CREATE INDEX [IX_ConcessionSales_StaffId] ON [ConcessionSales] ([StaffId]);
CREATE INDEX [IX_ConcessionSales_SaleDate] ON [ConcessionSales] ([SaleDate]);

-- ConcessionSaleItems
CREATE INDEX [IX_ConcessionSaleItems_ConcessionSaleId] ON [ConcessionSaleItems] ([ConcessionSaleId]);
CREATE INDEX [IX_ConcessionSaleItems_InventoryId] ON [ConcessionSaleItems] ([InventoryId]);

-- Promotions
CREATE INDEX [IX_Promotions_Code] ON [Promotions] ([Code]);
CREATE INDEX [IX_Promotions_Status] ON [Promotions] ([Status]);
CREATE INDEX [IX_Promotions_StartDate_EndDate] ON [Promotions] ([StartDate], [EndDate]);

-- CustomerPromotions
CREATE INDEX [IX_CustomerPromotions_CustomerId] ON [CustomerPromotions] ([CustomerId]);
CREATE INDEX [IX_CustomerPromotions_PromotionId] ON [CustomerPromotions] ([PromotionId]);

GO