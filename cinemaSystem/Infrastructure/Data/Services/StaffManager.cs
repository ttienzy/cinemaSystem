using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Application.Specifications.StaffSpec;
using Domain.Entities.StaffAggregate;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Shared.Common.Base;
using Shared.Models.DataModels.InventoryDtos;
using Shared.Models.DataModels.StaffDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Services
{
    public class StaffManager : IStaffManager
    {
        private readonly IRepository<Staff> _staffRepository;
        private readonly IRepository<Shift> _shiftRepository;
        private readonly IRepository<WorkSchedule> _workScheduleRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public StaffManager(IRepository<Staff> staffRepository, 
            UserManager<ApplicationUser> userManager, 
            IRepository<Shift> shiftRepo,
            IRepository<WorkSchedule> workRepo) 
        {
            _staffRepository = staffRepository;
            _userManager = userManager;
            _shiftRepository = shiftRepo;
            _workScheduleRepository = workRepo;
        }

        public async Task<BaseResponse<string>> AddShiftsToCinemaAsync(IEnumerable<ShiftRequest> requests)
        {
            try
            {
                IEnumerable<Shift> shifts = requests.Select(e => new Shift(e.CinemaId, e.Name, e.StartTime, e.EndTime));
                await _shiftRepository.AddRangeAsync(shifts);
                return BaseResponse<string>.Success("Add successful");
            }
            catch (Exception ex)
            {
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<object>> AddShiftToEmployeeAsync(TakeAttendanceOEmpRequest request)
        {
            try
            {
                var shift = await _shiftRepository.GetByIdAsync(request.ShiftId);
                if (shift == null) 
                    return BaseResponse<object>.Failure(Error.NotFound("Shift not found"));
                WorkSchedule workSchedule = new WorkSchedule(request.StaffId, request.ShiftId, request.ShiftDate);
                await _workScheduleRepository.AddAsync(workSchedule);

                var response = new
                {
                    StaffId = request.StaffId,
                    ShiftId = request.ShiftId,
                    StartTime = shift.DefaultStartTime,
                    EndTime = shift.DefaultEndTime,
                    ShiftDate = request.ShiftDate
                };

                return BaseResponse<object>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<string>> AddStaffToCinemaAsync(EmployeeCreateRequest request)
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
                var roleResult = await _userManager.AddToRolesAsync(user, request.Roles);
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

        public async Task<BaseResponse<IEnumerable<ShiftInfoResponse>>> GetShiftsInfoAsync(Guid cinemaId)
        {
            try
            {
                var shiftSpec = new ShiftsByCinemaSpecification(cinemaId);
                var shifts = await _shiftRepository.ListAsync(shiftSpec);
                return BaseResponse<IEnumerable<ShiftInfoResponse>>.Success(shifts);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<ShiftInfoResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<GetShiftsToCinemaResponse>>> GetShiftsMnAsync(Guid cinemaId)
        {
            try
            {
                var shiftsSpec = new GetShiftByCinemaIdSpecifcation(cinemaId);
                var shifts = await _shiftRepository.ListAsync(shiftsSpec);

                var resposne = shifts.Select(e => new GetShiftsToCinemaResponse
                {
                    ShiftId = e.Id,
                    StartTime = e.DefaultStartTime,
                    EndTime = e.DefaultEndTime,
                    Name = e.Name
                });
                return BaseResponse<IEnumerable<GetShiftsToCinemaResponse>>.Success(resposne);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<GetShiftsToCinemaResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<StaffInfoResponse>>> GetStaffInfoAsync(Guid cinemaId, DateTime ShiftDate)
        {
            try
            {
                var staffInfo = new StaffWithShiftsSpecification(cinemaId, ShiftDate);
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

        public async Task<BaseResponse<IEnumerable<GetStaffToCinemaResponse>>> GetStaffToCinemaAsync(Guid cinemaId)
        {
            try
            {
                var staffsSpec= new StaffsByCinemaIdSpecification(cinemaId);
                var staffs = await _staffRepository.ListAsync(staffsSpec);
                var response = staffs.Select(e => new GetStaffToCinemaResponse
                {
                    Id = e.Id,
                    Address = e.Address,
                    Department = e.Department,
                    Email = e.Email,
                    FullName = e.FullName,
                    HireDate = e.HireDate,
                    Phone = e.Phone,
                    Position = e.Position,
                    Salary = e.Salary,
                    Status = e.Status,

                });
                return BaseResponse<IEnumerable<GetStaffToCinemaResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<GetStaffToCinemaResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }
    }
}
