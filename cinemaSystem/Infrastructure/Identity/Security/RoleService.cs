using Application.Common.Exceptions;
using Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Models.IdentityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Identity.Security
{
    public class RoleService(RoleManager<IdentityRole<Guid>> roleManager) : IRoleService
    {
        public async Task CreateRoleAsync(string nameRole)
        {
            var checkRole = await roleManager.FindByNameAsync(nameRole);
            if (checkRole != null)
                throw new ConflictException($"Role {nameRole} already exists.");

            var role = new IdentityRole<Guid>
            {
                Name = nameRole,
                NormalizedName = nameRole.ToUpperInvariant()
            };

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ValidationException("Role", errors);
            }
        }

        public async Task DeleteRoleAsync(Guid roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId.ToString())
                ?? throw new NotFoundException("Role", roleId);

            var result = await roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ValidationException("Role", errors);
            }
        }

        public async Task<IEnumerable<RoleModel>> GetAllRolesAsync()
        {
            return await roleManager.Roles.Select(role => new RoleModel
            {
                Id = role.Id,
                Name = role.Name!
            }).ToListAsync();
        }

        public async Task<RoleModel> GetRoleByIdAsync(Guid roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId.ToString())
                ?? throw new NotFoundException("Role", roleId);

            return new RoleModel
            {
                Id = role.Id,
                Name = role.Name!
            };
        }

        public async Task UpdateRoleAsync(Guid roleId, string nameRole)
        {
            var role = await roleManager.FindByIdAsync(roleId.ToString())
                ?? throw new NotFoundException("Role", roleId);

            role.Name = nameRole;
            role.NormalizedName = nameRole.ToUpperInvariant();
            
            var result = await roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ValidationException("Role", errors);
            }
        }
    }
}
