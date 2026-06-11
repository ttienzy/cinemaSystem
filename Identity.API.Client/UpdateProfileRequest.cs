using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.API.Client
{
    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
