using FleetManagement.Core.Models;
using FleetManagement.Data;
using FleetManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public DashboardController(UserManager<AppUser> userManager, AppDbContext context, ITenantContext tenantContext)
        {
            _userManager = userManager;
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<IActionResult> Index()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userName))
            {
                return Challenge();
            }

            var isAdmin = User.IsInRole("Admin");
            var isOwner = User.IsInRole("Owner");
            var currentTenantId = _tenantContext.TenantId;

            var userQuery = _userManager.Users
                .Include(u => u.Vehicle)
                .AsQueryable();

            if (!isAdmin && currentTenantId.HasValue)
            {
                userQuery = userQuery.Where(u => u.TenantId == currentTenantId.Value);
            }

            var user = await userQuery.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                return Forbid();
            }

            var vehiclesQuery = _context.Vehicles.AsQueryable();
            var usersQuery = _userManager.Users.AsQueryable();
            var stopsQuery = _context.Stops.AsQueryable();
            var reportsQuery = _context.DailyReports.AsQueryable();
            var serviceRecordsQuery = _context.ServiceRecords.AsQueryable();

            if (!isAdmin && currentTenantId.HasValue)
            {
                vehiclesQuery = vehiclesQuery.Where(v => v.TenantId == currentTenantId.Value);
                usersQuery = usersQuery.Where(u => u.TenantId == currentTenantId.Value);
                stopsQuery = stopsQuery.Where(s => s.TenantId == currentTenantId.Value);
                reportsQuery = reportsQuery.Where(r => r.TenantId == currentTenantId.Value);
                serviceRecordsQuery = serviceRecordsQuery.Where(r => r.TenantId == currentTenantId.Value);
            }

            ViewBag.VehicleCount = await vehiclesQuery.CountAsync();
            ViewBag.UserCount = await usersQuery.CountAsync();
            ViewBag.StopCount = await stopsQuery.CountAsync();
            ViewBag.ReportCount = await reportsQuery.CountAsync();
            ViewBag.ServiceCount = await serviceRecordsQuery.CountAsync();

            var recentReportsQuery = _context.DailyReports
                .Include(r => r.Vehicle)
                .Include(r => r.User)
                .AsQueryable();

            if (!isAdmin && currentTenantId.HasValue)
            {
                recentReportsQuery = recentReportsQuery.Where(r => r.TenantId == currentTenantId.Value);
            }

            ViewBag.RecentReports = await recentReportsQuery
                .OrderByDescending(r => r.Date)
                .Take(5)
                .ToListAsync();

            return View(user);
        }
    }
}
