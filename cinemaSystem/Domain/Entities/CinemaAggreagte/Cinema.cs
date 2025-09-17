using Domain.Common;
using Domain.Entities.CinemaAggreagte.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.CinemaAggreagte
{
    public class Cinema : BaseEntity, IAggregateRoot
    {
        public string CinemaName { get; private set; }
        public string Address { get; private set; }
        public string? Phone { get; private set; }
        public string? Email { get; private set; }
        public string? Image { get; private set; }
        public string ManagerName { get; private set; }
        public CinemaStatus Status { get; private set; }

        private readonly List<Screen> _screens = new();
        public IReadOnlyCollection<Screen> Screens => _screens.AsReadOnly();

        public Cinema()
        {
        }
        public Cinema(string cinemaName, string address, string? phone, string? email, string? image, string managerName, CinemaStatus status)
        {
            CinemaName = cinemaName;
            Address = address;
            Phone = phone;
            Email = email;
            Image = image;
            ManagerName = managerName;
            Status = status;
        }
        public void UpdateDetails(string cinemaName, string address, string? phone, string? email, string? image, string managerName, CinemaStatus status)
        {
            CinemaName = cinemaName;
            Address = address;
            Phone = phone;
            Email = email;
            Image = image;
            ManagerName = managerName;
            Status = status;
        }
        public Screen? GetScreenById(Guid screenId)
        {
            return _screens.FirstOrDefault(s => s.Id == screenId);
        }
        public void AddItem(Screen screen)
        {
            _screens.Add(screen);
        }
        public void RemoveItem()
        {
            _screens.Clear();
        }
    }
}
