using FleetManagement.Core.Models;
using FleetManagement.Data;
using FleetManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Controllers
{
    [Authorize(Roles = "User,Owner")]
    public class DailyReportsController : Controller
    {
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.Users
                .Include(u => u.Vehicle)
                .FirstAsync(u => u.UserName == User.Identity.Name);

            if (user == null)
                return Unauthorized();

            if (user.Vehicle == null)
                return BadRequest("User has no assigned vehicle");

            return View(new DailyReportViewModel
            {
                VehicleId = user.Vehicle.Id,
                VehicleRegistration = user.Vehicle.RegistrationNumber,
                Stops = await _context.Stops.ToListAsync()
            });
        }


        [HttpPost]
        public async Task<IActionResult> Create(DailyReportViewModel model)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState
        .Where(x => x.Value.Errors.Count > 0)
        .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}");

                throw new Exception("MODELSTATE INVALID: " + string.Join(" | ", errors));
            }

            if (model.SelectedStopIds == null)
                throw new Exception("SelectedStopIds IS NULL");

            if (!model.SelectedStopIds.Any())
                throw new Exception("NO STOPS SELECTED");

            if (model.VehicleId == 0)
                throw new Exception("VehicleId == 0");

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();


            var report = new DailyReport
            {
                UserId = user.Id,
                VehicleId = model.VehicleId,
                Date = DateTime.UtcNow,
                Mileage = model.Mileage,
                Notes = model.Notes,
                Stops = model.SelectedStopIds.Select(id => new DailyReportStop
                {
                    StopId = id
                }).ToList()
            };

            _context.DailyReports.Add(report);

            Console.WriteLine($"VehicleId: {model.VehicleId}");
            Console.WriteLine($"Stops: {model.SelectedStopIds.Count}");

            await _context.SaveChangesAsync();

            Console.WriteLine("SAVED");

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var reports = await _context.DailyReports
                .Include(r => r.Vehicle)
                .Include(r => r.Stops)
                    .ThenInclude(s => s.Stop)
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            return View(reports);
        }

        public async Task<IActionResult> Details(int id)
        {
            var report = await _context.DailyReports
                .Include(r => r.Vehicle)
                .Include(r => r.Stops)
                    .ThenInclude(s => s.Stop)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null) return NotFound();

            return View(report);
        }



        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public DailyReportsController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
    }

}
