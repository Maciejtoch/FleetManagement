using FleetManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace FleetManagement.Controllers
{
    public class OwnerReportsController : Controller
    {
        private readonly AppDbContext _context;

        public OwnerReportsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reports = await _context.DailyReports
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .Include(r => r.Stops)
                    .ThenInclude(s => s.Stop)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            return View(reports);
        }

        public async Task<IActionResult> Details(int id)
        {
            var report = await _context.DailyReports
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .Include(r => r.Stops)
                    .ThenInclude(s => s.Stop)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                return NotFound();

            return View(report);
        }

    }
}
