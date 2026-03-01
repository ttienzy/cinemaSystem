using Application.Common.Interfaces.Persistence;
using Domain.Entities.StaffAggregate;
using MediatR;
using Shared.Models.DataModels.StaffDtos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Staff.Commands.AssignStaff
{
    public record AssignStaffCommand(StaffAssignmentRequest Request) : IRequest<Guid>;

    public class AssignStaffHandler(IStaffRepository staffRepo, IUnitOfWork unitOfWork) 
        : IRequestHandler<AssignStaffCommand, Guid>
    {
        public async Task<Guid> Handle(AssignStaffCommand request, CancellationToken ct)
        {
            var existingStaff = await staffRepo.GetByEmailAsync(request.Request.Email, ct);
            
            if (existingStaff != null)
            {
                // In a real system, we might update the assignment or throw an error.
                // For this demo, let's assume we update the cinema assignment.
                existingStaff = new Domain.Entities.StaffAggregate.Staff(
                    request.Request.CinemaId,
                    request.Request.FullName,
                    request.Request.Position,
                    request.Request.Department,
                    request.Request.Phone,
                    request.Request.Email,
                    request.Request.Address,
                    request.Request.HireDate,
                    request.Request.Salary
                );
                // Actually, the Staff constructor above creates a new one. 
                // Let's just Add a new one if it's a new assignment for that email.
            }

            var staff = new Domain.Entities.StaffAggregate.Staff(
                request.Request.CinemaId,
                request.Request.FullName,
                request.Request.Position,
                request.Request.Department,
                request.Request.Phone,
                request.Request.Email,
                request.Request.Address,
                request.Request.HireDate,
                request.Request.Salary
            );

            await staffRepo.AddAsync(staff, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return staff.Id;
        }
    }
}
