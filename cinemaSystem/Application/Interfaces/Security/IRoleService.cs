using Shared.Common.Base;
using Shared.Models.IdentityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Security
{
    public interface IRoleService
    {
        Task<BaseResponse<IEnumerable<RoleModel>>> GetAllRolesAsync();
        Task<BaseResponse<RoleModel>> GetRoleByIdAsync(Guid roleId);
        Task<BaseResponse<string>> CreateRoleAsync(string nameRole);
        Task<BaseResponse<string>> UpdateRoleAsync(Guid roleId, string nameRole);
        Task<BaseResponse<string>> DeleteRoleAsync(Guid roleId);

    }
}
