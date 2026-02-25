using Shared.Common.Base;
using Shared.Models.IdentityModels;

namespace Application.Common.Interfaces.Security
{
    /// <summary>
    /// Role management — CRUD operations for Identity roles.
    /// </summary>
    public interface IRoleService
    {
        Task<BaseResponse<string>> CreateRoleAsync(string nameRole);
        Task<BaseResponse<string>> DeleteRoleAsync(Guid roleId);
        Task<BaseResponse<IEnumerable<RoleModel>>> GetAllRolesAsync();
        Task<BaseResponse<RoleModel>> GetRoleByIdAsync(Guid roleId);
        Task<BaseResponse<string>> UpdateRoleAsync(Guid roleId, string nameRole);
    }
}
