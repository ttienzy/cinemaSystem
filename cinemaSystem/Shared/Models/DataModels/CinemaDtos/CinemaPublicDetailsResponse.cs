using Domain.Entities.CinemaAggreagte.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.CinemaDtos
{
    public class CinemaPublicDetailsResponse
    {
        public CinemaDetails Cinema {  get; set; }
        public List<ScreenToCinemaDetails> Screens {  get; set; }
    }
    public class CinemaDetails 
    { 
        public string CinemaName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Image { get; set; }
        public string ManagerName { get; set; }
        public CinemaStatus Status { get; set; }

    }
    public class ScreenToCinemaDetails
    {
        public Guid Id { get; set; }
        public string ScreenName { get; set; }
        public ScreenType Type { get; set; }
        public ScreenStatus Status { get; set; }
        public int SeatCount { get; set; }
    }

}
