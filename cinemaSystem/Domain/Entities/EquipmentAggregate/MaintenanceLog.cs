using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.EquipmentAggregate
{
    public class MaintenanceLog : BaseEntity
    {
        public Guid EquipmentId { get; private set; }
        public DateTime MaintenanceDate { get; private set; }
        public decimal? Cost { get; private set; }
        public string IssuesFound { get; private set; }
        public bool IsCompleted { get; private set; }

        public static MaintenanceLog Create(Guid equipmentId, DateTime maintenanceDate, decimal? cost, string issuesFound, bool isCompleted)
        {
            return new MaintenanceLog
            {
                EquipmentId = equipmentId,
                MaintenanceDate = maintenanceDate,
                Cost = cost,
                IssuesFound = issuesFound,
                IsCompleted = isCompleted
            };
        }
    }
}
