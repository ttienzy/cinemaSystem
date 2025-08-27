using Domain.Common;
using Domain.Entities.CinemaAggreagte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.EquipmentAggregate
{
    public class Equipment : BaseEntity, IAggregateRoot
    {
        public Guid CinemaId { get; private set; }
        public Guid? ScreenId { get; private set; } // Null nếu là thiết bị chung của rạp
        public string EquipmentType { get; private set; }
        public DateTime PurchaseDate { get; private set; }
        public string Status { get; private set; } // working, needs_repair, out_of_order

        private List<MaintenanceLog> _maintenanceLogs;
        public IReadOnlyCollection<MaintenanceLog> MaintenanceLogs => _maintenanceLogs.AsReadOnly();

        public Screen? Screen { get; private set; }
    }
}
