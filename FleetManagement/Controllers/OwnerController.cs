using FleetManagement.Core.Models;
using FleetManagement.Data;
using FleetManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Controllers
{
    [Authorize(Roles = "Admin,Owner")]
    public class OwnerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public OwnerController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            IQueryable<Tenant> query = _context.Tenants.AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.TenantId == null)
                {
                    return Forbid();
                }

                query = query.Where(t => t.Id == currentUser.TenantId.Value);
            }

            var tenants = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tenants);
        }

        public async Task<IActionResult> Details(int id)
        {
            var tenant = await _context.Tenants
                .Include(t => t.Users)
                .Include(t => t.Vehicles)
                .Include(t => t.Stops)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tenant == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.TenantId != tenant.Id)
                {
                    return Forbid();
                }
            }

            return View(tenant);
        }

        public IActionResult Create()
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(new TenantCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TenantCreateViewModel model)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Slug == model.Slug || t.Domain == model.Domain);

            if (existingTenant != null)
            {
                ModelState.AddModelError(string.Empty, "Tenant o tej samej nazwie lub domenie już istnieje.");
                return View(model);
            }

            var tenant = new Tenant
            {
                Name = model.Name,
                Slug = model.Slug,
                Domain = model.Domain,
                Status = "Active",
                PlanType = "Local",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            var tenantOwner = new AppUser
            {
                UserName = model.AdminEmail,
                Email = model.AdminEmail,
                FullName = "Tenant Owner",
                TenantId = tenant.Id,
                MustChangePassword = false
            };

            var ownerResult = await _userManager.CreateAsync(tenantOwner, model.AdminPassword);
            if (!ownerResult.Succeeded)
            {
                foreach (var error in ownerResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            await _userManager.AddToRoleAsync(tenantOwner, "Owner");

            return RedirectToAction(nameof(Index));
        }
    }
}
