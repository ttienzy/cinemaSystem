using Infrastructure.Identity.Constants;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity
{
    public static class IdentityContextSeed
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            // Seed Roles
            await SeedRoleAsync(roleManager, RoleConstant.SuperAdmin);
            await SeedRoleAsync(roleManager, RoleConstant.Admin);
            await SeedRoleAsync(roleManager, RoleConstant.Manager);
            await SeedRoleAsync(roleManager, RoleConstant.Staff);
            await SeedRoleAsync(roleManager, RoleConstant.Customer);

            // Seed Default Admin
            var defaultAdmin = new ApplicationUser
            {
                UserName = "System Admin",
                Email = "admin@cinema.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            if (userManager.Users.All(u => u.UserName != defaultAdmin.UserName))
            {
                var user = await userManager.FindByEmailAsync(defaultAdmin.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultAdmin, "Admin123!");
                    await userManager.AddToRoleAsync(defaultAdmin, RoleConstant.SuperAdmin);
                }
            }
        }

        private static async Task SeedRoleAsync(RoleManager<IdentityRole<Guid>> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }
}
