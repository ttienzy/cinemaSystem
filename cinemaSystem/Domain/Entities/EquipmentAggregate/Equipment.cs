using Domain.Common;
using Domain.Entities.CinemaAggregate;
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
        public Guid? ScreenId { get; private set; } // Null n?u l� thi?t b? chung c?a r?p
        public string EquipmentType { get; private set; }
        public DateTime PurchaseDate { get; private set; }
        public string Status { get; private set; } // working, needs_repair, out_of_order

        private List<MaintenanceLog> _maintenanceLogs = new();
        public IReadOnlyCollection<MaintenanceLog> MaintenanceLogs => _maintenanceLogs.AsReadOnly();

        public Screen? Screen { get; private set; }

        // Factory method
        public static Equipment Create(Guid cinemaId, Guid? screenId, string equipmentType, DateTime purchaseDate, string status)
        {
            return new Equipment
            {
                CinemaId = cinemaId,
                ScreenId = screenId,
                EquipmentType = equipmentType,
                PurchaseDate = purchaseDate,
                Status = status
            };
        }

        public void Update(Guid cinemaId, Guid? screenId, string equipmentType, string status)
        {
            CinemaId = cinemaId;
            ScreenId = screenId;
            EquipmentType = equipmentType;
            Status = status;
        }
    }
}
