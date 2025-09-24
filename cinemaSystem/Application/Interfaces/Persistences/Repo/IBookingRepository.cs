using Shared.Models.DataModels.BookingDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences.Repo
{
    public interface IBookingRepository
    {
        Task<IEnumerable<PurchaseResponse>> PurchaseHistoryAsync(Guid userId);
        Task<BookingCheckedInResponse> CheckInBookingAsync(Guid bookingId);
    }
}
