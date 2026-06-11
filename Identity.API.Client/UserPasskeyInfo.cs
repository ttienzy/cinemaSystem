using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.API.Client
{
    public class UserPasskeyInfo
    {
        public string CredentialId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

}
