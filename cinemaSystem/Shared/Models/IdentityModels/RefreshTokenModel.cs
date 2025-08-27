using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.IdentityModels
{
    public class RefreshTokenModel
    {
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}
