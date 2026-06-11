using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.API.Client
{
    public class PasskeyAttestationRequest
    {
        public string CredentialJson { get; set; } = string.Empty;
        public string? Name { get; set; }
    }

}
