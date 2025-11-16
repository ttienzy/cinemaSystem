using Shared.Models.DataModels.DashboardDtos;
using Shared.Models.DataModels.DashboardDtos.Subs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences.Repo
{
    public interface ICinemaInsightRepository
    {
        /// <summary>
        /// Lấy thông tin chi tiết về doanh thu vé và đồ ăn trong một lần gọi duy nhất.
        /// </summary>
        /// <param name="request">Yêu cầu chứa CinemaId và khoảng thời gian.</param>
        /// <returns>Một Tuple chứa báo cáo vé và báo cáo đồ ăn.</returns>
        Task<(TicketReportDto TicketReport, ConcessionReportDto ConcessionReport)> FetchSalesInsightsAsync(CinemaDashboardRequest request);

        /// <summary>
        /// Quét tỷ lệ lấp đầy của các suất chiếu cho một rạp.
        /// </summary>
        Task<List<ShowtimeOccupancyDto>> ScanShowtimeCapacityAsync(Guid cinemaId);

        /// <summary>
        /// Thăm dò tín hiệu tồn kho (hết hàng, tồn kho thấp).
        /// </summary>
        Task<List<InventoryStatusDto>> ProbeInventorySignalsAsync(Guid cinemaId);

        /// <summary>
        /// Đánh giá tình hình nhân sự trong ngày (tổng số, đã check-in, đi trễ).
        /// </summary>
        Task<StaffStatusSummaryDto> EvaluateDailyManpowerAsync(Guid cinemaId);
    }

}
