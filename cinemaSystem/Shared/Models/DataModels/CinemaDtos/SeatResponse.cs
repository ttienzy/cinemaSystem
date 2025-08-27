using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.CinemaDtos
{
    public class SeatResponse
    {
        public Guid Id { get; set; }
        public string RowName { get; set; }
        public int Number { get; set; }
        public bool IsActive { get; set; }
        public bool IsBlocked { get; set; }
        public required string SeatTypeName { get; set; }
    }
}
