using Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.IdentityModels;

namespace Api.Controllers
{
    /// <summary>
    /// Handles role management APIs.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController(IRoleService roleService) : ControllerBase
    {
        /// <summary>
        /// Get all roles.
        /// </summary>
        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleModel>>> GetRoles()
        {
            return Ok(await roleService.GetAllRolesAsync());
        }

        /// <summary>
        /// Get role by ID.
        /// </summary>
        [HttpGet("{roleId}")]
        public async Task<ActionResult<RoleModel>> GetRoleById(Guid roleId)
        {
            return Ok(await roleService.GetRoleByIdAsync(roleId));
        }

        /// <summary>
        /// Create a new role.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] string roleName)
        {
            await roleService.CreateRoleAsync(roleName);
            return Ok($"Role {roleName} created successfully.");
        }

        /// <summary>
        /// Update a role.
        /// </summary>
        [HttpPut("{roleId}")]
        public async Task<IActionResult> UpdateRole(Guid roleId, [FromBody] string roleName)
        {
            await roleService.UpdateRoleAsync(roleId, roleName);
            return Ok($"Role with ID {roleId} updated successfully.");
        }

        /// <summary>
        /// Delete a role.
        /// </summary>
        [HttpDelete("{roleId}")]
        public async Task<IActionResult> DeleteRole(Guid roleId)
        {
            await roleService.DeleteRoleAsync(roleId);
            return Ok($"Role with ID {roleId} deleted successfully.");
        }
    }
}
