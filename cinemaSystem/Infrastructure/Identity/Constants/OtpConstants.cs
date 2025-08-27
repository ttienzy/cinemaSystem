using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity.Constants
{
    public class OtpConstants
    {
        public const int MaxLoginAttempts = 5;
        // expires in 10 minutes
        public const int OtpExpirationMinutes = 10;
    }
}
