using Application.Interfaces.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Base;
using Shared.Models.IdentityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity.Security
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        public RoleService(RoleManager<IdentityRole<Guid>> roleManager)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<BaseResponse<string>> CreateRoleAsync(string nameRole)
        {
            try
            {
                var checkRole = await _roleManager.FindByNameAsync(nameRole);
                if (checkRole != null)
                {
                    return BaseResponse<string>.Failure(Error.Conflict($"Role {nameRole} already exists."));
                }
                var role = new IdentityRole<Guid>
                {
                    Name = nameRole,
                    NormalizedName = nameRole.ToUpperInvariant()
                };

                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BaseResponse<string>.Failure(Error.BadRequest(errors));
                }
                return BaseResponse<string>.Success($"Role {nameRole} created successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<string>> DeleteRoleAsync(Guid roleId)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role == null)
                {
                    return BaseResponse<string>.Failure(Error.NotFound($"Role with ID {roleId} not found."));
                }
                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BaseResponse<string>.Failure(Error.BadRequest(errors));
                }
                return BaseResponse<string>.Success($"Role with ID {roleId} deleted successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<RoleModel>>> GetAllRolesAsync()
        {
            try
            {
                var roles = _roleManager.Roles.Select(role => new RoleModel
                {
                    Id = role.Id,
                    Name = role.Name!
                });
                return BaseResponse<IEnumerable<RoleModel>>.Success(await roles.ToListAsync());
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<RoleModel>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<RoleModel>> GetRoleByIdAsync(Guid roleId)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role == null)
                {
                    return BaseResponse<RoleModel>.Failure(Error.NotFound($"Role with ID {roleId} not found."));
                }
                return BaseResponse<RoleModel>.Success(new RoleModel
                {
                    Id = role.Id,
                    Name = role.Name!
                });
            }
            catch (Exception ex)
            {
                return BaseResponse<RoleModel>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<string>> UpdateRoleAsync(Guid roleId, string nameRole)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role == null)
                {
                    return BaseResponse<string>.Failure(Error.NotFound($"Role with ID {roleId} not found."));
                }
                role.Name = nameRole;
                role.NormalizedName = nameRole.ToUpperInvariant();
                var result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BaseResponse<string>.Failure(Error.BadRequest(errors));
                }
                return BaseResponse<string>.Success($"Role with ID {roleId} updated successfully.");
            }

            catch (Exception ex)
            {
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
        }
    }
}
