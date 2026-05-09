using Cinema.Shared.Constants;
using Identity.API.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Infrastructure.Persistence.Seeders;

public static class IdentityDataSeeder
{
    public static async Task SeedAsync(IdentityDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        // Seed Roles
        await SeedRolesAsync(roleManager);

        // Seed Users
        await SeedUsersAsync(userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[]
        {
            AppConstants.Roles.Admin,
            AppConstants.Roles.Staff,
            AppConstants.Roles.Customer
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Check if users already exist
        if (await userManager.Users.AnyAsync())
        {
            return;
        }

        // Admin User
        var adminUser = new ApplicationUser
        {
            UserName = "admin@cinema.com",
            Email = "admin@cinema.com",
            EmailConfirmed = true,
            FullName = "System Administrator"
        };

        var adminResult = await userManager.CreateAsync(adminUser, "Admin@123");
        if (adminResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, AppConstants.Roles.Admin);
        }

        // Staff User
        var staffUser = new ApplicationUser
        {
            UserName = "staff@cinema.com",
            Email = "staff@cinema.com",
            EmailConfirmed = true,
            FullName = "Cinema Staff"
        };

        var staffResult = await userManager.CreateAsync(staffUser, "Staff@123");
        if (staffResult.Succeeded)
        {
            await userManager.AddToRoleAsync(staffUser, AppConstants.Roles.Staff);
        }

        // Customer Users
        var customers = new[]
        {
            new { Email = "customer1@example.com", FullName = "John Doe" },
            new { Email = "customer2@example.com", FullName = "Jane Smith" },
            new { Email = "customer3@example.com", FullName = "Bob Johnson" }
        };

        foreach (var customer in customers)
        {
            var customerUser = new ApplicationUser
            {
                UserName = customer.Email,
                Email = customer.Email,
                EmailConfirmed = true,
                FullName = customer.FullName
            };

            var customerResult = await userManager.CreateAsync(customerUser, "Customer@123");
            if (customerResult.Succeeded)
            {
                await userManager.AddToRoleAsync(customerUser, AppConstants.Roles.Customer);
            }
        }
    }
}



