using FleetManagement.Core.Models;
using FleetManagement.Data;
using FleetManagement.Models;
using FleetManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Controllers
{
    [Authorize(Roles = "Admin,Owner")]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public UsersController(
            UserManager<AppUser> userManager,
            AppDbContext context,
            ITenantContext tenantContext)
        {
            _userManager = userManager;
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<IActionResult> Index()
        {
            var isAdmin = User.IsInRole("Admin");
            var tenantId = _tenantContext.TenantId;
            var usersQuery = _userManager.Users.AsQueryable();

            if (!isAdmin && tenantId.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.TenantId == tenantId.Value);
            }

            var users = await usersQuery.ToListAsync();

            var model = new List<UserListViewModel>();
            foreach (var user in users)
            {
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "-";
                model.Add(new UserListViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = role,
                    IsActive = user.LockoutEnd == null
                });
            }

            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> EditRole(string id, string? returnUrl = null)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var canManageAllTenants = User.IsInRole("Admin");
            if (!canManageAllTenants && _tenantContext.TenantId.HasValue && user.TenantId != _tenantContext.TenantId.Value)
            {
                return Forbid();
            }

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";

            return View(new EditUserRoleViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Role = role,
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditUserRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var canManageAllTenants = User.IsInRole("Admin");
            if (!canManageAllTenants && _tenantContext.TenantId.HasValue && user.TenantId != _tenantContext.TenantId.Value)
            {
                return Forbid();
            }

            var allowedTenantRoles = new[] { "User", "TenantAdmin" };
            if (!allowedTenantRoles.Contains(model.Role, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.Role), "Dozwolone role to: Kierowca lub Admin (dla tenanta).");
                return View(model);
            }

            if (string.Equals(model.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.Role), "Nie można nadać roli SuperAdmin z tego widoku.");
                return View(model);
            }

            if (string.Equals(model.Role, "Owner", StringComparison.OrdinalIgnoreCase) && user.TenantId.HasValue)
            {
                var ownerUsers = await _userManager.GetUsersInRoleAsync("Owner");
                var tenantOwnerCount = ownerUsers.Count(u => u.TenantId == user.TenantId && u.Id != user.Id);
                if (tenantOwnerCount == 0)
                {
                    var currentUserRoles = await _userManager.GetRolesAsync(user);
                    if (!currentUserRoles.Contains("Owner"))
                    {
                        ModelState.AddModelError(string.Empty, "Nie można zmienić roli ostatniego właściciela tenanta.");
                        return View(model);
                    }
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains("Owner") && user.TenantId.HasValue)
            {
                var ownerUsers = await _userManager.GetUsersInRoleAsync("Owner");
                var ownerCount = ownerUsers.Count(u => u.TenantId == user.TenantId);
                if (ownerCount <= 1 && !string.Equals(model.Role, "Owner", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, "Nie można usunąć lub zmienić roli ostatniego ownera tenanta.");
                    return View(model);
                }
            }

            var existingRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, existingRoles);
            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.ReturnUrl) || !Url.IsLocalUrl(model.ReturnUrl))
            {
                return RedirectToAction(nameof(Index));
            }

            return Redirect(model.ReturnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var allowedTenantRoles = new[] { "User", "TenantAdmin" };
            if (!allowedTenantRoles.Contains(model.Role, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.Role), "Dozwolone role to: Kierowca lub Admin (dla tenanta).");
                return View(model);
            }

            var currentTenantId = User.IsInRole("Admin") ? null : _tenantContext.TenantId;
            if (!User.IsInRole("Admin") && !currentTenantId.HasValue)
            {
                return Forbid();
            }

            var email = model.Email.Trim();
            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Użytkownik o takim adresie e-mail już istnieje.");
                return View(model);
            }

            if (!User.IsInRole("Admin") && currentTenantId.HasValue)
            {
                var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == currentTenantId.Value);
                var tenantDomain = tenant?.Domain?.Trim();
                if (!string.IsNullOrWhiteSpace(tenantDomain) &&
                    !email.EndsWith($"@{tenantDomain}", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(model.Email), $"Adres e-mail musi należeć do domeny {tenantDomain}.");
                    return View(model);
                }
            }

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = model.FullName,
                EmailConfirmed = true,
                MustChangePassword = false,
                TenantId = currentTenantId
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var canManageAllTenants = User.IsInRole("Admin");
            if (!canManageAllTenants && _tenantContext.TenantId.HasValue && user.TenantId != _tenantContext.TenantId.Value)
            {
                return Forbid();
            }

            var vehicles = _context.Vehicles
                .Where(v => canManageAllTenants || !v.TenantId.HasValue || v.TenantId == _tenantContext.TenantId)
                .ToList();

            return View(new EditUserViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                VehicleId = user.VehicleId,
                Vehicles = vehicles
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var canManageAllTenants = User.IsInRole("Admin");
            if (!canManageAllTenants && _tenantContext.TenantId.HasValue && user.TenantId != _tenantContext.TenantId.Value)
            {
                return Forbid();
            }

            user.FullName = model.FullName;
            user.VehicleId = model.VehicleId;

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ResetPassword(string id, string? returnUrl = null)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var canManageAllTenants = User.IsInRole("Admin");
            if (!canManageAllTenants && _tenantContext.TenantId.HasValue && user.TenantId != _tenantContext.TenantId.Value)
            {
                return Forbid();
            }

            return View(new EditUserSecurityViewModel
            {
                UserId = user.Id,
                FullName = user.FullName ?? user.Email ?? string.Empty,
                NewPassword = string.Empty,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(EditUserSecurityViewModel model, string? returnUrl = null)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var canManageAllTenants = User.IsInRole("Admin");
            if (!canManageAllTenants && _tenantContext.TenantId.HasValue && user.TenantId != _tenantContext.TenantId.Value)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Nowe hasło jest wymagane.");
                return View(model);
            }

            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, model.NewPassword);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user?.VehicleId == null)
                return View(null);

            var session = await _context.LocationShareSessions
                .Where(s => s.VehicleId == user.VehicleId && s.IsActive)
                .OrderByDescending(s => s.ExpiresAt)
                .FirstOrDefaultAsync();

            return View(session);
        }
    }
}
