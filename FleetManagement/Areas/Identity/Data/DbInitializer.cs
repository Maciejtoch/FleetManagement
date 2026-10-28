using FleetManagement.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FleetManagement.Data
{
    public static class DbInitializer
    {
        private static async Task RemoveDuplicateSeedUsersAsync(AppDbContext context, UserManager<AppUser> userManager, params string[] emails)
        {
            foreach (var email in emails.Distinct())
            {
                var normalizedName = userManager.NormalizeName(email);
                var normalizedEmail = userManager.NormalizeEmail(email);

                var duplicates = await context.Users
                    .IgnoreQueryFilters()
                    .Where(u => u.NormalizedUserName == normalizedName || u.NormalizedEmail == normalizedEmail || u.Email == email || u.UserName == email)
                    .OrderBy(u => u.Id)
                    .ToListAsync();

                if (duplicates.Count <= 1)
                {
                    continue;
                }

                foreach (var duplicate in duplicates.Skip(1))
                {
                    var roles = await userManager.GetRolesAsync(duplicate);
                    if (roles.Count > 0)
                    {
                        await userManager.RemoveFromRolesAsync(duplicate, roles);
                    }

                    context.Users.Remove(duplicate);
                }

                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedSampleTenantDataAsync(AppDbContext context, UserManager<AppUser> userManager, Tenant tenant)
        {
            if (await context.Vehicles.AnyAsync(v => v.TenantId == tenant.Id))
            {
                return;
            }

            var tenantDriver = await userManager.Users.FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.Email != null && u.Email.Contains("kierowca"));
            if (tenantDriver == null)
            {
                return;
            }

            var vehicles = new[]
            {
                new Vehicle { TenantId = tenant.Id, RegistrationNumber = "DW 1111 AB", Make = "Mercedes", Model = "Sprinter", Year = 2022, Mileage = 118500, VIN = "WDBA1234567890123", AssignedUserId = tenantDriver.Id },
                new Vehicle { TenantId = tenant.Id, RegistrationNumber = "DW 2222 CD", Make = "Volvo", Model = "FH", Year = 2021, Mileage = 214800, VIN = "YV2A1234567890124", AssignedUserId = tenantDriver.Id },
                new Vehicle { TenantId = tenant.Id, RegistrationNumber = "DW 3333 EF", Make = "MAN", Model = "TGX", Year = 2023, Mileage = 96350, VIN = "WMA1234567890125", AssignedUserId = tenantDriver.Id }
            };

            context.Vehicles.AddRange(vehicles);
            await context.SaveChangesAsync();

            var stops = new[]
            {
                new Stop { TenantId = tenant.Id, CompanyName = "PrzewozyXYZ - Centrum Logistyczne", Address = "ul. Warecka 12, Warszawa", IsOnMap = true, Latitude = 52.2297, Longitude = 21.0122 },
                new Stop { TenantId = tenant.Id, CompanyName = "B2B Retail Hub", Address = "ul. Powstańców Śląskich 34, Katowice", IsOnMap = true, Latitude = 50.2649, Longitude = 19.0238 },
                new Stop { TenantId = tenant.Id, CompanyName = "Punkt Dostawczy Wrocław", Address = "ul. Oławska 9, Wrocław", IsOnMap = true, Latitude = 51.1079, Longitude = 17.0385 }
            };

            context.Stops.AddRange(stops);
            await context.SaveChangesAsync();

            var firstVehicle = context.Vehicles.First(v => v.TenantId == tenant.Id);
            var secondVehicle = context.Vehicles.Skip(1).First(v => v.TenantId == tenant.Id);

            context.DailyReports.AddRange(
                new DailyReport { TenantId = tenant.Id, UserId = tenantDriver.Id, VehicleId = firstVehicle.Id, Date = DateTime.UtcNow.AddDays(-1), Mileage = 118600, Notes = "Trip duration: 4 h 20 min. Delivery completed on time.", User = tenantDriver, Vehicle = firstVehicle },
                new DailyReport { TenantId = tenant.Id, UserId = tenantDriver.Id, VehicleId = secondVehicle.Id, Date = DateTime.UtcNow.AddDays(-3), Mileage = 214900, Notes = "Trip duration: 5 h 15 min. Customer confirmed delivery.", User = tenantDriver, Vehicle = secondVehicle }
            );

            context.ServiceRecords.AddRange(
                new ServiceRecord { TenantId = tenant.Id, VehicleId = firstVehicle.Id, ServiceDate = DateTime.UtcNow.AddDays(-15), Mileage = 117900, Description = "Wymiana oleju i filtra powietrza", Cost = 680.00m },
                new ServiceRecord { TenantId = tenant.Id, VehicleId = secondVehicle.Id, ServiceDate = DateTime.UtcNow.AddDays(-22), Mileage = 214200, Description = "Kontrola hamulców i wymiana klocków", Cost = 970.50m }
            );

            await context.SaveChangesAsync();
        }

        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var seedData = configuration.GetSection("SeedData");

            var platformAdminEmail = seedData["PlatformAdminEmail"] ?? "admin@fleet.com";
            var platformAdminPassword = seedData["PlatformAdminPassword"] ?? "Admin123!";
            var tenantOwnerEmail = seedData["TenantOwnerEmail"] ?? "admin.kowalski@przewozyxyz.pl";
            var tenantOwnerPassword = seedData["TenantOwnerPassword"] ?? "AdminKowalski123!";
            var tenantDriverEmail = seedData["TenantDriverEmail"] ?? "kierowca.mirek@przewozyxyz.pl";
            var tenantDriverPassword = seedData["TenantDriverPassword"] ?? "KierowcaMirek123!";
            var tenantName = seedData["TenantName"] ?? "PrzewozyXYZ";
            var tenantSlug = seedData["TenantSlug"] ?? "przewozyxyz";
            var tenantDomain = seedData["TenantDomain"] ?? "przewozyxyz.pl";

            await RemoveDuplicateSeedUsersAsync(context, userManager, platformAdminEmail, tenantOwnerEmail, tenantDriverEmail);

            string[] roles = { "Admin", "Owner", "TenantAdmin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var platformAdmin = await userManager.FindByEmailAsync(platformAdminEmail);
            if (platformAdmin == null)
            {
                platformAdmin = new AppUser
                {
                    UserName = platformAdminEmail,
                    Email = platformAdminEmail,
                    FullName = "Platform Administrator"
                };

                var createResult = await userManager.CreateAsync(platformAdmin, platformAdminPassword);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create platform admin: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            if (platformAdmin.MustChangePassword)
            {
                platformAdmin.MustChangePassword = false;
                await userManager.UpdateAsync(platformAdmin);
            }

            if (!await userManager.IsInRoleAsync(platformAdmin, "Admin"))
            {
                var roleResult = await userManager.AddToRoleAsync(platformAdmin, "Admin");
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to assign platform admin to Admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }

            var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == tenantSlug || t.Domain == tenantDomain || t.Name == tenantName);
            if (tenant == null)
            {
                tenant = new Tenant
                {
                    Name = tenantName,
                    Slug = tenantSlug,
                    Domain = tenantDomain,
                    Status = "Active",
                    PlanType = "Enterprise",
                    IsActive = true
                };

                context.Tenants.Add(tenant);
                await context.SaveChangesAsync();
            }

            var tenantOwner = await userManager.FindByEmailAsync(tenantOwnerEmail);
            if (tenantOwner == null)
            {
                tenantOwner = new AppUser
                {
                    UserName = tenantOwnerEmail,
                    Email = tenantOwnerEmail,
                    FullName = "Admin Kowalski",
                    TenantId = tenant.Id,
                    MustChangePassword = false
                };

                var createResult = await userManager.CreateAsync(tenantOwner, tenantOwnerPassword);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create tenant owner: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            if (tenantOwner.TenantId != tenant.Id || tenantOwner.MustChangePassword)
            {
                tenantOwner.TenantId = tenant.Id;
                tenantOwner.MustChangePassword = false;
                await userManager.UpdateAsync(tenantOwner);
            }

            if (!await userManager.IsInRoleAsync(tenantOwner, "Owner"))
            {
                var roleResult = await userManager.AddToRoleAsync(tenantOwner, "Owner");
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to assign tenant owner to Owner role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }

            var tenantDriver = await userManager.FindByEmailAsync(tenantDriverEmail);
            if (tenantDriver == null)
            {
                tenantDriver = new AppUser
                {
                    UserName = tenantDriverEmail,
                    Email = tenantDriverEmail,
                    FullName = "Mirek Kierowca",
                    TenantId = tenant.Id,
                    MustChangePassword = false
                };

                var createResult = await userManager.CreateAsync(tenantDriver, tenantDriverPassword);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create tenant driver: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            if (tenantDriver.TenantId != tenant.Id || tenantDriver.MustChangePassword)
            {
                tenantDriver.TenantId = tenant.Id;
                tenantDriver.MustChangePassword = false;
                await userManager.UpdateAsync(tenantDriver);
            }

            if (!await userManager.IsInRoleAsync(tenantDriver, "User"))
            {
                var roleResult = await userManager.AddToRoleAsync(tenantDriver, "User");
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to assign tenant driver to User role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }

            await SeedSampleTenantDataAsync(context, userManager, tenant);
        }
    }
}
