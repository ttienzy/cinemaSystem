using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.CinemaAggreagte.Enum
{
    public enum ScreenType
    {
        Standard = 1, // Standard screen without special features
        TwoD = 2, // Standard 2D screen
        ThreeD = 3, // 3D screen
        IMAX = 4, // IMAX screen
        FourDX = 5, // 4DX screen with motion and effects
        VIP = 6 // VIP screen with luxury seating and services

    }
    public enum ScreenStatus
    {
        Active = 1, // Screen is active and available for bookings
        Maintenance = 2, // Screen is under maintenance
        Closed = 3 // Screen is permanently closed
    }
}
