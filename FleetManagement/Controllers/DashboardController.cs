using FleetManagement.Core.Models;
using FleetManagement.Data;
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

        public DashboardController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.Users
                .Include(u => u.Vehicle)
                .FirstAsync(u => u.UserName == User.Identity.Name);

            ViewBag.VehicleCount = await _context.Vehicles.CountAsync();
            ViewBag.UserCount = await _userManager.Users.CountAsync();
            ViewBag.StopCount = await _context.Stops.CountAsync();
            ViewBag.ReportCount = await _context.DailyReports.CountAsync();
            ViewBag.ServiceCount = await _context.ServiceRecords.CountAsync();

            ViewBag.RecentReports = await _context.DailyReports
                .Include(r => r.Vehicle)
                .Include(r => r.User)
                .OrderByDescending(r => r.Date)
                .Take(5)
                .ToListAsync();

            return View(user);
        }
    }
}
