using Infrastructure.Identity.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity
{
    public static class IdentityContextSeed
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IConfiguration configuration)
        {
            // Seed Roles
            await SeedRoleAsync(roleManager, RoleConstant.SuperAdmin);
            await SeedRoleAsync(roleManager, RoleConstant.Admin);
            await SeedRoleAsync(roleManager, RoleConstant.Manager);
            await SeedRoleAsync(roleManager, RoleConstant.Staff);
            await SeedRoleAsync(roleManager, RoleConstant.Customer);

            // Get admin email and password from configuration
            var adminEmail = configuration["Identity:DefaultAdminEmail"] ?? "admin@cinema.com";
            var adminPassword = configuration["Identity:DefaultAdminPassword"] ?? "Admin@Cinema2024!";

            // Seed Default Admin
            var defaultAdmin = new ApplicationUser
            {
                UserName = "System Admin",
                Email = adminEmail,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            if (userManager.Users.All(u => u.UserName != defaultAdmin.UserName))
            {
                var user = await userManager.FindByEmailAsync(defaultAdmin.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultAdmin, adminPassword);
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
