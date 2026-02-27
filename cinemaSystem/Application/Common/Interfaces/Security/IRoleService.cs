using Shared.Models.IdentityModels;

namespace Application.Common.Interfaces.Security
{
    /// <summary>
    /// Role management — CRUD operations for Identity roles.
    /// </summary>
    public interface IRoleService
    {
        Task CreateRoleAsync(string nameRole);
        Task DeleteRoleAsync(Guid roleId);
        Task<IEnumerable<RoleModel>> GetAllRolesAsync();
        Task<RoleModel> GetRoleByIdAsync(Guid roleId);
        Task UpdateRoleAsync(Guid roleId, string nameRole);
    }
}
