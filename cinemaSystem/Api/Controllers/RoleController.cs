using Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.IdentityModels;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")]
    public class RoleController(IRoleService roleService) : ControllerBase
    {
        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleModel>>> GetRoles()
        {
            return Ok(await roleService.GetAllRolesAsync());
        }

        [HttpGet("{roleId}")]
        public async Task<ActionResult<RoleModel>> GetRoleById(Guid roleId)
        {
            return Ok(await roleService.GetRoleByIdAsync(roleId));
        }

        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] string roleName)
        {
            await roleService.CreateRoleAsync(roleName);
            return Ok($"Role {roleName} created successfully.");
        }

        [HttpPut("{roleId}")]
        public async Task<IActionResult> UpdateRole(Guid roleId, [FromBody] string roleName)
        {
            await roleService.UpdateRoleAsync(roleId, roleName);
            return Ok($"Role with ID {roleId} updated successfully.");
        }

        [HttpDelete("{roleId}")]
        public async Task<IActionResult> DeleteRole(Guid roleId)
        {
            await roleService.DeleteRoleAsync(roleId);
            return Ok($"Role with ID {roleId} deleted successfully.");
        }
    }
}
