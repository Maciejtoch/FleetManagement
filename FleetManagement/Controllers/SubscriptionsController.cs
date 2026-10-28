using FleetManagement.Core.Models;
using FleetManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SubscriptionsController : Controller
    {
        private readonly AppDbContext _context;

        public SubscriptionsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tenants = await _context.Tenants
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();

            ViewBag.Tenants = tenants;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePlan(int tenantId, string planType, DateTime? subscriptionEndsAt)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
            {
                return NotFound();
            }

            var allowedPlans = new[] { "Starter", "Basic", "Standard", "Pro", "Enterprise", "Custom", "Local" };
            if (string.IsNullOrWhiteSpace(planType) || !allowedPlans.Contains(planType, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest();
            }

            tenant.PlanType = planType;
            tenant.SubscriptionEndsAt = subscriptionEndsAt;
            tenant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(int tenantId, string reason)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
            {
                return NotFound();
            }

            tenant.IsActive = false;
            tenant.Status = "Suspended";
            tenant.SuspensionReason = reason;
            tenant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int tenantId)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
            {
                return NotFound();
            }

            tenant.IsActive = true;
            tenant.Status = "Active";
            tenant.SuspensionReason = string.Empty;
            tenant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
