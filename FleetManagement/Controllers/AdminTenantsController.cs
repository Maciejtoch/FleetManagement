using FleetManagement.Core.Models;
using FleetManagement.Data;
using FleetManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Tenants")]
    public class AdminTenantsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AdminTenantsController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tenants = await _context.Tenants
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();

            return View(tenants);
        }

        [HttpGet("Details/{tenantId:int}")]
        public async Task<IActionResult> Details(int tenantId, string tab = "overview")
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                return NotFound();
            }

            var users = await _userManager.Users
                .AsNoTracking()
                .Where(u => u.TenantId == tenantId)
                .OrderBy(u => u.Email)
                .ToListAsync();

            var userList = new List<UserListViewModel>();
            foreach (var user in users)
            {
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";
                userList.Add(new UserListViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    Role = role,
                    IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTime.UtcNow
                });
            }

            var vehicles = await _context.Vehicles
                .AsNoTracking()
                .Where(v => v.TenantId == tenantId)
                .OrderBy(v => v.Make)
                .ThenBy(v => v.Model)
                .ToListAsync();

            var auditLogs = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.TenantId == tenantId)
                .OrderByDescending(a => a.DateUtc)
                .Take(30)
                .ToListAsync();

            var overview = new AdminTenantDetailsViewModel
            {
                Tenant = tenant,
                Users = userList,
                Vehicles = vehicles,
                AuditLogs = auditLogs,
                UserCount = userList.Count,
                VehicleCount = vehicles.Count,
                LastLoginUtc = auditLogs
                    .Where(a => a.Action.Contains("login", StringComparison.OrdinalIgnoreCase) || a.Details.Contains("login", StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.DateUtc)
                    .FirstOrDefault(),
                LastLoginError = auditLogs
                    .Where(a => a.Action.Contains("login", StringComparison.OrdinalIgnoreCase) || a.Details.Contains("login", StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.Details)
                    .FirstOrDefault(),
                ActiveTab = tab
            };

            return View(overview);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (tenant == null)
            {
                return NotFound();
            }

            return View(tenant);
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tenant model)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id);
            if (tenant == null)
            {
                return NotFound();
            }

            tenant.Name = model.Name;
            tenant.Domain = model.Domain;
            tenant.Status = model.Status;
            tenant.IsActive = model.IsActive;
            tenant.SubscriptionEndsAt = model.SubscriptionEndsAt;
            tenant.SuspensionReason = model.SuspensionReason;
            tenant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await LogTenantAsync(tenant.Id, "tenant_edited", $"Zmieniono dane podatnika: {tenant.Name}");

            return RedirectToAction(nameof(Details), new { tenantId = tenant.Id });
        }

        [HttpPost("CreateOwner/{tenantId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOwner(int tenantId, string email, string fullName, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "E-mail i hasło są wymagane.";
                return RedirectToAction(nameof(Details), new { tenantId, tab = "users" });
            }

            var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
            {
                return NotFound();
            }

            var normalizedEmail = email.Trim();
            var tenantDomain = tenant.Domain?.Trim();
            if (!string.IsNullOrWhiteSpace(tenantDomain) &&
                !normalizedEmail.EndsWith($"@{tenantDomain}", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = $"Adres e-mail musi należeć do domeny {tenantDomain}.";
                return RedirectToAction(nameof(Details), new { tenantId, tab = "users" });
            }

            if (await _userManager.FindByEmailAsync(normalizedEmail) != null)
            {
                TempData["Error"] = "Użytkownik o takim adresie e-mail już istnieje.";
                return RedirectToAction(nameof(Details), new { tenantId, tab = "users" });
            }

            var user = new AppUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                FullName = fullName,
                TenantId = tenantId,
                EmailConfirmed = true,
                MustChangePassword = false
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Details), new { tenantId, tab = "users" });
            }

            await _userManager.AddToRoleAsync(user, "Owner");
            await LogTenantAsync(tenantId, "owner_created", $"Dodano ownera {normalizedEmail}");

            return RedirectToAction(nameof(Details), new { tenantId, tab = "users" });
        }

        [HttpPost("CreateDriver/{tenantId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDriver(int tenantId, string email, string fullName, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "E-mail i hasło są wymagane.";
                return RedirectToAction(nameof(Details), new { tenantId, tab = "users" });
            }

            var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
            {
                return NotFound();
            }

            var normalizedEmail = email.Trim();
            var tenantDomain = tenant.Domain?.Trim();
            if (!string.IsNullOrWhiteSpace(tenantDomain) &&
                !normalizedEmail.EndsWith($"@{tenantDomain}", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = $"Adres e-mail musi należeć do domeny {tenantDomain}.";
                return RedirectToAction(nameof(Details), new { tenantId, tab = "users" });
            }

            if (await _userManager.FindByEmailAsync(normalizedEmail) != null)
            {
                TempData["Error"] = "Użytkownik o takim adresie e-mail już istnieje.";
                return RedirectToAction(nameof(Details), new { tenantId, tab = "users" });
            }

            var user = new AppUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                FullName = fullName,
                TenantId = tenantId,
                EmailConfirmed = true,
                MustChangePassword = false
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Details), new { tenantId, tab = "users" });
            }

            await _userManager.AddToRoleAsync(user, "User");
            await LogTenantAsync(tenantId, "driver_created", $"Dodano kierowcę {normalizedEmail}");

            return RedirectToAction(nameof(Details), new { tenantId, tab = "users" });
        }

        private async Task LogTenantAsync(int tenantId, string action, string details)
        {
            var adminUser = await _userManager.GetUserAsync(User);
            _context.AuditLogs.Add(new AuditLog
            {
                AdminUserId = adminUser?.Id ?? string.Empty,
                TenantId = tenantId,
                Action = action,
                Details = details,
                DateUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
    }
}
