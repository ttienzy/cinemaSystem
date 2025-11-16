using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.DashboardDtos.Subs
{
    public class TicketReportDto
    {
        public int TotalTickets { get; set; }
        public decimal TotalTicketsAmount { get; set; }
        public int Transactions { get; set; }
    }

}
