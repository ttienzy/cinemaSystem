using Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Base;
using Shared.Models.IdentityModels;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RoleController(IRoleService roleService)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        }
        [HttpGet("roles")]
        public async Task<IActionResult> GetRolesAsync()
        {
            var response = await _roleService.GetAllRolesAsync();
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<IEnumerable<RoleModel>>.WithError(response);
        }
        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetRoleByIdAsync(Guid roleId)
        {
            var response = await _roleService.GetRoleByIdAsync(roleId);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<RoleModel>.WithError(response);
        }
        [HttpPost]
        public async Task<IActionResult> AddRoleAsync([FromBody] string roleName)
        {
            if (roleName == null)
            {
                return BadRequest("Role request cannot be null.");
            }
            var response = await _roleService.CreateRoleAsync(roleName);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<string>.WithError(response);
        }
        [HttpPut("{roleId}")]
        public async Task<IActionResult> UpdateRoleAsync(Guid roleId, [FromBody] string roleName)
        {
            if (roleName == null)
            {
                return BadRequest("Role request cannot be null.");
            }
            var response = await _roleService.UpdateRoleAsync(roleId, roleName);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<string>.WithError(response);
        }
        [HttpDelete("{roleId}")]
        public async Task<IActionResult> DeleteRoleAsync(Guid roleId)
        {
            var response = await _roleService.DeleteRoleAsync(roleId);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<string>.WithError(response);
        }
    }
}
