using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.API.Client
{
    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
