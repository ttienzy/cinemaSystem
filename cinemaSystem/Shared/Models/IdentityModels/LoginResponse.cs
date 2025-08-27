using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.IdentityModels
{
    public class LoginResponse
    {
        public UserProfileResponse UserProfile { get; set; } = new UserProfileResponse();
        public TokenResponse Token { get; set; } = new TokenResponse();
    }
}
