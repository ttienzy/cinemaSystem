using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.CinemaAggregate.Enum
{
    public enum ScreenType
    {
        Standard, // Standard screen without special features
        TwoD, // Standard 2D screen
        ThreeD, // 3D screen
        IMAX, // IMAX screen
        FourDX, // 4DX screen with motion and effects
        VIP

    }
    public enum ScreenStatus
    {
        Active, // Screen is active and available for bookings
        Maintenance, // Screen is under maintenance
        Closed // Screen is permanently closed
    }
}
