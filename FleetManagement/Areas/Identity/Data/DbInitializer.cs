using FleetManagement.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace FleetManagement.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            // Roles
            string[] roles = { "Owner", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Admin user
            string ownerEmail = "admin@fleet.com";
            string ownerPassword = "Admin123!";

            if (await userManager.FindByEmailAsync(ownerEmail) == null)
            {
                var owner = new AppUser
                {
                    UserName = ownerEmail,
                    Email = ownerEmail,
                    FullName = "System Owner"
                };

                await userManager.CreateAsync(owner, ownerPassword);
                await userManager.AddToRoleAsync(owner, "Owner");
            }

        }
    }
}
