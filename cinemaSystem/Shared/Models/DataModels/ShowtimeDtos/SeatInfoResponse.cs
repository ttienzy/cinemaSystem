using Domain.Entities.CinemaAggreagte.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ShowtimeDtos
{
    public class SeatInfoResponse
    {
        public Guid Id { get; set; }
        public string RowName { get; set; }
        public int Number { get; set; }
        public Guid SeatTypeId { get; set; }
        public string SeatTypeName { get; set; }
        public SeatStatus Status { get; set; }
        public required DateTime LastUpdated { get; set; }
    }
}
