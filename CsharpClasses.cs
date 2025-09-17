 public class Booking : BaseEntity, IAggregateRoot
 {
     public Guid? CustomerId { get; private set; } 
     public Guid ShowtimeId { get; private set; } 
     public DateTime BookingTime { get; private set; }
     public int TotalTickets { get; private set; }
     public decimal TotalAmount { get; private set; }
     public BookingStatus Status { get; private set; } 

     private readonly List<BookingTicket> _bookingTickets = new();
     public IReadOnlyCollection<BookingTicket> BookingTickets => _bookingTickets.AsReadOnly();

     private readonly List<Payment> _payments = new();
     public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
-----------------------------------
public class BookingTicket: BaseEntity
{
    public Guid SeatId { get; private set; } 
    public Guid BookingId { get; private set; }
    public decimal TicketPrice { get; private set; }
    public Seat? Seat { get; private set; }
---------------------------------
public class Payment : BaseEntity
{
    public Guid BookingId { get; private set; }
    public string? PaymentMethod { get; private set; } // e.g., cash, card, ewallet, bank_transfer
    public string? PaymentProvider { get; private set; } // e.g., Visa, Momo, ZaloPay
    public decimal Amount { get; private set; }
    public string? Currency { get; private set; }
    public string? TransactionId { get; private set; }
    public string? ReferenceCode { get; private set; }
    public DateTime PaymentTime { get; private set; }
  ---------------------------------
public class Cinema : BaseEntity, IAggregateRoot
{
    public string CinemaName { get; private set; }
    public string Address { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Image { get; private set; }
    public string ManagerName { get; private set; }
    public CinemaStatus Status { get; private set; }

    private readonly List<Screen> _screens = new();
    public IReadOnlyCollection<Screen> Screens => _screens.AsReadOnly();
--------------------------------------
public class Screen : BaseEntity
{
    public Guid CinemaId { get; private set; }
    public string ScreenName { get; private set; }
    public ScreenType Type { get; private set; }
    public ScreenStatus Status { get; private set; }


    private readonly List<Seat> _seats = new();
    public IReadOnlyCollection<Seat> Seats => _seats.AsReadOnly();
---------------------------------
public class Seat : BaseEntity
{
    public Guid ScreenId { get; private set; }
    public Guid SeatTypeId { get; private set; } 
    public string RowName { get; private set; } 
    public int Number { get; private set; } 
    public bool IsActive { get; private set; }
    public bool IsBlocked { get; private set; }
-------------------------------------
 public class ConcessionSale : BaseEntity, IAggregateRoot
 {
     public Guid CinemaId { get; private set; }
     public Guid? BookingId { get; private set; }
     public Guid StaffId { get; private set; }
     public DateTime SaleDate { get; private set; }
     public decimal TotalAmount { get; private set; }
     public string PaymentMethod { get; private set; }


     private readonly List<ConcessionSaleItem> _items = new();
     public IReadOnlyCollection<ConcessionSaleItem> Items => _items.AsReadOnly();
--------------------------------------
public class ConcessionSaleItem : BaseEntity
{
    public Guid ConcessionSaleId { get; private set; }
    public Guid InventoryId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

}
--------------------------------------
public class Equipment : BaseEntity, IAggregateRoot
{
    public Guid CinemaId { get; private set; }
    public Guid? ScreenId { get; private set; } // Null nếu là thiết bị chung của rạp
    public string EquipmentType { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public string Status { get; private set; } // working, needs_repair, out_of_order

    private List<MaintenanceLog> _maintenanceLogs;
    public IReadOnlyCollection<MaintenanceLog> MaintenanceLogs => _maintenanceLogs.AsReadOnly();

    public Screen? Screen { get; private set; }
}
--------------------------------------
 public class MaintenanceLog : BaseEntity
 {
     public Guid EquipmentId { get; private set; }
     public DateTime MaintenanceDate { get; private set; }
     public decimal? Cost { get; private set; }
     public string IssuesFound { get; private set; }
     public bool IsCompleted { get; private set; }
 }
----------------------------------
public class InventoryItem : BaseEntity, IAggregateRoot
{
    public Guid CinemaId { get; private set; } // FK to Cinema Aggregate Root
    public string ItemName { get; private set; }
    public string? ItemCategory { get; private set; }
    public int CurrentStock { get; private set; }
    public int MinimumStock { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal CostPrice { get; private set; }
    public string SupplierInfo { get; private set; }
    public DateTime LastRestocked { get; private set; }
}
----------------------------------
public class Movie : BaseEntity, IAggregateRoot
{
    public string Title { get; private set; }
    public int DurationMinutes { get; private set; }
    public RatingStatus Rating { get; private set; }
    public string Trailer { get; private set; }
    public DateTime ReleaseDate { get; private set; }
    public string Description { get; private set; }
    public string PosterUrl { get; private set; }
    public MovieStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }


    private readonly List<MovieCopyright> _copyrights;
    public IReadOnlyCollection<MovieCopyright> Copyrights => _copyrights.AsReadOnly();

    private readonly List<MovieCertification> _certifications;
    public IReadOnlyCollection<MovieCertification> Certifications => _certifications.AsReadOnly();

    private readonly List<MovieCastCrew> _castCrew;
    public IReadOnlyCollection<MovieCastCrew> CastCrew => _castCrew.AsReadOnly();

    private readonly List<MovieGenre> _movieGenres;
    public IReadOnlyCollection<MovieGenre> MovieGenres => _movieGenres.AsReadOnly();
-----------------------------------------
 public class Genre :BaseEntity, IAggregateRoot
 {
     public string? GenreName { get; private set; } // e.g., Action, Comedy, Horror
     public string? Description { get; private set; }
     public bool IsActive { get; private set; }
 ---------------------------------------
public class PricingTier : BaseEntity, IAggregateRoot
{
    public string? TierName { get; private set; } // e.g., Standard, Peak, Holiday, Student
    public decimal Multiplier { get; private set; }
    public string? ValidDays { get; private set; }
    
---------------------------------------
 public class SeatType : BaseEntity, IAggregateRoot
 {
     public string TypeName { get; private set; } // e.g., Standard, VIP, Couple, Wheelchair
     public decimal PriceMultiplier { get; private set; }
     
---------------------------------------
public class TimeSlot : BaseEntity, IAggregateRoot
{
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public string DayType { get; private set; } // e.g., weekday, weekend, holiday
    public bool IsActive { get; private set; }
   ----------------------------------------
public class Showtime : BaseEntity, IAggregateRoot
{
    public Guid CinemaId { get; private set; }
    public Guid MovieId { get; private set; } 
    public Guid ScreenId { get; private set; }
    public Guid SlotId { get; private set; }
    public Guid PricingTierId { get; private set; } 
    public DateTime ShowDate { get; private set; }
    public DateTime ActualStartTime { get; private set; }
    public DateTime ActualEndTime { get; private set; }
    public ShowtimeStatus Status { get; private set; } 


    private readonly List<ShowtimePricing> _showtimePricings = new();
    public IReadOnlyCollection<ShowtimePricing> ShowtimePricings => _showtimePricings.AsReadOnly();
-------------------------------------------
public class ShowtimePricing : BaseEntity
{
    public Guid ShowtimeId { get; private set; }
    public Guid SeatTypeId { get; private set; } 
    public decimal BasePrice { get; private set; }
    public decimal FinalPrice { get; private set; }
 ----------------------------------------
    public class Shift : BaseEntity
    {
        public Guid StaffId { get; private set; }
        public Guid CinemaId { get; private set; } 
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public DateTime ShiftDate { get; private set; }

    }
}
     
---------------------------------------------------------
public class Staff : BaseEntity, IAggregateRoot
{
    public Guid CinemaId { get; private set; } 
    public string? FullName { get; private set; }
    public string? Position { get; private set; }
    public string? Department { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public DateTime HireDate { get; private set; }
    public decimal Salary { get; private set; }
    public StaffStatus Status { get; private set; } // e.g., active, on_leave, terminated

    private readonly List<Shift> _shifts = new();
    public IReadOnlyCollection<Shift> Shifts => _shifts.AsReadOnly();

}