using Application.Interfaces.Integrations;
using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Application.Specifications.InventorySpec;
using Application.Specifications.StaffSpec;
using Domain.Constants;
using Domain.Entities.BookingAggregate;
using Domain.Entities.ConcessionAggregate;
using Domain.Entities.InventoryAggregate;
using Domain.Entities.StaffAggregate;
using Infrastructure.Hubs;
using Infrastructure.Hubs.Constants;
using Infrastructure.Identity;
using Infrastructure.Identity.Constants;
using Infrastructure.Redis.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Shared.Common.Base;
using Shared.Common.Paging;
using Shared.Models.DataModels.InventoryDtos;
using Shared.Models.DataModels.ShowtimeDtos;
using Shared.Models.DataModels.StaffDtos;


namespace Infrastructure.Data.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IRepository<InventoryItem> _inventoryRepository;
        private readonly IRepository<Staff> _staffRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IHubContext<SeatHub> _seatHubContext;
        private readonly IConcessionSaleRepository _concessionSaleRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public InventoryService(IRepository<InventoryItem> repository, 
            IUnitOfWork unitOfWork, ICacheService cacheService, 
            IHubContext<SeatHub> seatHubContext, 
            IConcessionSaleRepository concessionSaleRepository,
            IRepository<Staff> repository1,
            UserManager<ApplicationUser> userManager)
        {
            _inventoryRepository = repository;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _seatHubContext = seatHubContext;
            _concessionSaleRepository = concessionSaleRepository;
            _staffRepository = repository1;
            _userManager = userManager;
        }

        public async Task<BaseResponse<IEnumerable<InventoryResponse>>> AddInventoryAsync(IEnumerable<InventoryRequest> requests)
        {
            try
            {
                var listItems = requests.Select(e => new InventoryItem(e.CinemaId, e.ItemName, e.ItemCategory, e.CurrentStock, e.MinimumStock, e.UnitPrice, e.UnitPrice, e.ImageUrl));
                await _inventoryRepository.AddRangeAsync(listItems);
                var response = listItems.Select(e => new InventoryResponse
                {
                    Id = e.Id,
                    CurrentStock = e.CurrentStock,
                    Image = e.ImageUrl,
                    ItemCategory = e.ItemCategory,
                    ItemName = e.ItemName,
                    UnitPrice = e.UnitPrice,
                });
                return BaseResponse<IEnumerable<InventoryResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<InventoryResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<string>> AddStaffToCinema(EmployeeCreateRequest request)
        {
            try
            {
                if (await _userManager.FindByEmailAsync(request.Email) != null)
                {
                    return BaseResponse<string>.Failure(Error.Conflict("Email already in use"));
                }
                var user = new ApplicationUser
                {
                    UserName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                }; 
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BaseResponse<string>.Failure(Error.BadRequest(errors));
                }
                // Assign role
                var roles = new List<string> { RoleConstant.User, RoleConstant.Employee };
                var roleResult = await _userManager.AddToRolesAsync(user, roles);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    return BaseResponse<string>.Failure(Error.BadRequest(errors));
                }
                var staffByEmailSpec = new StaffByEmailSpecification(request.Email);
                var existingStaff = await _staffRepository.FirstOrDefaultAsync(staffByEmailSpec);
                var newStaff = new Staff(request.CinemaId, request.FullName, request.Position, request.Department, request.PhoneNumber, request.Email, request.Address, request.HireDate, request.Salary);
                await _staffRepository.AddAsync(newStaff);
                return BaseResponse<string>.Success("Staff added successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<string>> ConfirmPaymentConcessionAsync(CartRequest request, Guid cinemaId)
        {
            try
            {
                await _unitOfWork.BeginTractionAsync();

                var concessionSales = request.Concessions.ToList();
                var ticketSales = request.Tickets?.SelectedSeats.ToList();

                // Both ticket and concession
                if (ticketSales?.Count > 0 && concessionSales.Count > 0)
                {
                    var totalTickets = ticketSales.Count();
                    var totalTicketMoney = ticketSales.Sum(e => e.Price);
                    var bookingTickets = ticketSales.Select(e => new BookingTicket(e.SeatId, e.Price)).ToList();
                    var payment = new Payment(totalTicketMoney);
                    var newBooking = new Booking(null, request.Tickets.ShowTimeId, totalTickets, totalTicketMoney, bookingTickets, payment);
                    newBooking.CreateId();
                    newBooking.MarkAsCompleted();

                    // Concession
                    var totalConcessionMoney = concessionSales.Sum(e => e.Quantity * e.Price);
                    var totalMoney = totalTicketMoney + totalConcessionMoney;
                    var concessionSale = new ConcessionSale(cinemaId, newBooking.Id, request.StaffId, totalMoney, request.PaymentMethod);
                    var concessionItems = concessionSales.Select(e => new ConcessionSaleItem(e.ItemId, e.Quantity, e.Price)).ToList();
                    concessionSale.AddItems(concessionItems);

                    // Update inventory for each concession item
                    var inventoryIds = concessionSales.Select(e => e.ItemId).Distinct().ToList();
                    var inventorySpec = new ListInventoriesSpecification(inventoryIds);
                    var inventories = await _unitOfWork.InventoryItems.ListAsync(inventorySpec);

                    var inventoryDict = inventories.ToDictionary(i => i.Id);

                    foreach (var c in concessionSales)
                    {
                        if (inventoryDict.TryGetValue(c.ItemId, out var inventory))
                        {
                            inventory.DecreaseStock(c.Quantity);
                        }
                    }
                    // Set seat as booked in cache
                    var seatListCache = await _cacheService.GetAsync<ShowtimeSeatingPlanResponse>(CacheKey.SeatingPlan(request.Tickets.ShowTimeId));
                    if (seatListCache != null)
                    {
                        seatListCache.Seats.ForEach(s =>
                        {
                            if (ticketSales.Any(ts => ts.SeatId == s.Id))
                            {
                                s.Status = Domain.Entities.CinemaAggreagte.Enum.SeatStatus.Booked;
                                s.LastUpdated = DateTime.UtcNow;
                            }
                        });
                        await _cacheService.UpdateAsync(CacheKey.SeatingPlan(request.Tickets.ShowTimeId), seatListCache);
                        await _seatHubContext.Clients.Group(request.Tickets.ShowTimeId.ToString()).SendAsync(SignalMethodConstants.OnSeatsBooked, ticketSales.Select(ts => ts.SeatId).ToList());
                    }

                    await _unitOfWork.Bookings.AddAsync(newBooking);
                    await _unitOfWork.ConcessionSales.AddAsync(concessionSale);
                    await _unitOfWork.InventoryItems.UpdateRangeAsync(inventories);

                }
                // Only ticket
                else if (ticketSales?.Count > 0)
                {
                    var totalTickets = ticketSales.Count();
                    var totalTicketMoney = ticketSales.Sum(e => e.Price);
                    var bookingTickets = ticketSales.Select(e => new BookingTicket(e.SeatId, e.Price)).ToList();
                    var payment = new Payment(totalTicketMoney);
                    var newBooking = new Booking(null, request.Tickets.ShowTimeId, totalTickets, totalTicketMoney, bookingTickets, payment);
                    newBooking.CreateId();
                    newBooking.MarkAsCompleted();

                    // Concession
                    var concessionSale = new ConcessionSale(cinemaId, newBooking.Id, Guid.Parse("93ADD7DE-5B81-40D9-971D-F47FB68F8429"), totalTicketMoney, request.PaymentMethod);

                    // Set seat as booked in cache                   
                    var seatListCache = await _cacheService.GetAsync<ShowtimeSeatingPlanResponse>(CacheKey.SeatingPlan(request.Tickets.ShowTimeId));
                    if (seatListCache != null)
                    {
                        seatListCache.Seats.ForEach(s =>
                        {
                            if (ticketSales.Any(ts => ts.SeatId == s.Id))
                            {
                                s.Status = Domain.Entities.CinemaAggreagte.Enum.SeatStatus.Booked;
                                s.LastUpdated = DateTime.UtcNow;
                            }
                        });
                        await _cacheService.UpdateAsync(CacheKey.SeatingPlan(request.Tickets.ShowTimeId), seatListCache);
                        await _seatHubContext.Clients.Group(request.Tickets.ShowTimeId.ToString()).SendAsync(SignalMethodConstants.OnSeatsBooked, ticketSales.Select(ts => ts.SeatId).ToList());
                    }

                    await _unitOfWork.Bookings.AddAsync(newBooking);
                    await _unitOfWork.ConcessionSales.AddAsync(concessionSale);
                }
                // Only concession
                else if (concessionSales.Count > 0)
                {
                    var totalConcessionMoney = concessionSales.Sum(e => e.Quantity * e.Price);
                    var concessionSale = new ConcessionSale(cinemaId, null, Guid.Parse("93ADD7DE-5B81-40D9-971D-F47FB68F8429"), totalConcessionMoney, request.PaymentMethod);
                    var concessionItems = concessionSales.Select(e => new ConcessionSaleItem(e.ItemId, e.Quantity, e.Price)).ToList();
                    concessionSale.AddItems(concessionItems);

                    // Update inventory for each concession item
                    var inventoryIds = concessionSales.Select(e => e.ItemId).Distinct().ToList();
                    var inventorySpec = new ListInventoriesSpecification(inventoryIds);
                    var inventories = await _unitOfWork.InventoryItems.ListAsync(inventorySpec);

                    var inventoryDict = inventories.ToDictionary(i => i.Id);

                    foreach (var c in concessionSales)
                    {
                        if (inventoryDict.TryGetValue(c.ItemId, out var inventory))
                        {
                            inventory.DecreaseStock(c.Quantity);
                        }
                    }

                    await _unitOfWork.ConcessionSales.AddAsync(concessionSale);
                    await _unitOfWork.InventoryItems.UpdateRangeAsync(inventories);
                }
                else
                {
                    return BaseResponse<string>.Failure(Error.BadRequest("No items in cart"));
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return BaseResponse<string>.Success("Transaction successful");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<BaseResponse<IEnumerable<ConcessionRevenueResponse>>> GetConcessionRevenueReportAsync(Guid cinemaId)
        {
            try
            {
                var result = await _concessionSaleRepository.GetConcessionRevenueReportAsync(cinemaId);
                return BaseResponse<IEnumerable<ConcessionRevenueResponse>>.Success(result);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<ConcessionRevenueResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<PaginatedList<ConcessionSaleHistoryResponse>>> GetConcessionSaleHistoryAsync(Guid cinemaId, ConcessionSaleQueryParameter query)
        {
            try
            {
                var result = await _concessionSaleRepository.GetConcessionSaleHistory(cinemaId, query);
                return BaseResponse<PaginatedList<ConcessionSaleHistoryResponse>>.Success(result);
            }
            catch (Exception ex)
            {
                return BaseResponse<PaginatedList<ConcessionSaleHistoryResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<InventoryResponse>>> GetConcessionsAsync(Guid cinemaId)
        {
            try
            {
                var results = new GetConcessionByCinemaIdSpecification(cinemaId);
                return BaseResponse<IEnumerable<InventoryResponse>>.Success(await _inventoryRepository.ListAsync(results));
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<InventoryResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<StaffInfoResponse>>> GetStaffInfoAsync(Guid cinemaId)
        {
            try
            {
                var staffInfo = new StaffWithShiftsSpecification(cinemaId);
                var result = await _staffRepository.ListAsync(staffInfo);
                if (result == null || !result.Any())
                {
                    return BaseResponse<IEnumerable<StaffInfoResponse>>.Failure(Error.NotFound("No staff found for the given cinema ID."));
                }
                return BaseResponse<IEnumerable<StaffInfoResponse>>.Success(result);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<StaffInfoResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<StaffReponse>> GetStaffOnTimeAsync(string email)
        {
            try
            {
                var staffByEmailSpec = new StaffByEmailSpecification(email);
                var staff = await _staffRepository.FirstOrDefaultAsync(staffByEmailSpec);
                if (staff == null)
                {
                    return BaseResponse<StaffReponse>.Failure(Error.NotFound("Staff not found"));
                }
                var resposne = new StaffReponse
                {
                    Id = staff.Id,
                    CinemaId = staff.CinemaId,
                };
                return BaseResponse<StaffReponse>.Success(resposne);
            }
            catch (Exception ex)
            {
                return BaseResponse<StaffReponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }
    }
}
