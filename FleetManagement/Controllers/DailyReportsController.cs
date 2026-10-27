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
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public DailyReportsController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

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

            var notes = model.Notes ?? string.Empty;
            if (model.TripDurationMinutes > 0)
            {
                notes = AddTripDurationToNotes(notes, model.TripDurationMinutes);
            }

            var report = new DailyReport
            {
                UserId = user.Id,
                VehicleId = model.VehicleId,
                Date = DateTime.UtcNow,
                Mileage = model.Mileage,
                Notes = notes,
                Stops = model.SelectedStopIds.Select(id => new DailyReportStop
                {
                    StopId = id
                }).ToList()
            };

            _context.DailyReports.Add(report);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var reports = await _context.DailyReports
                .Include(r => r.Vehicle)
                .Include(r => r.Stops)
                    .ThenInclude(s => s.Stop)
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            foreach (var report in reports)
            {
                report.TripDurationMinutes = ParseTripDuration(report.Notes);
            }

            return View(reports);
        }

        public async Task<IActionResult> Details(int id)
        {
            var report = await _context.DailyReports
                .Include(r => r.Vehicle)
                .Include(r => r.Stops)
                    .ThenInclude(s => s.Stop)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                return NotFound();

            report.TripDurationMinutes = ParseTripDuration(report.Notes);
            return View(report);
        }

        private static int? ParseTripDuration(string? notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
                return null;

            var marker = "Trip duration:";
            var index = notes.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return null;

            var value = notes.Substring(index + marker.Length).Trim();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var totalMinutes = 0;
            var segments = value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            for (var i = 0; i < segments.Length - 1; i += 2)
            {
                if (int.TryParse(segments[i], out var number) && segments[i + 1].Length > 0)
                {
                    var unit = segments[i + 1].ToLowerInvariant();
                    if (unit.StartsWith("h"))
                        totalMinutes += number * 60;
                    else if (unit.StartsWith("m"))
                        totalMinutes += number;
                }
            }

            return totalMinutes > 0 ? totalMinutes : null;
        }

        private static string AddTripDurationToNotes(string? notes, int minutes)
        {
            var formatted = DailyReport.FormatDuration(minutes);
            var trimmed = notes?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(trimmed))
                return $"Trip duration: {formatted}";

            if (trimmed.Contains("Trip duration:", StringComparison.OrdinalIgnoreCase))
                return trimmed;

            return $"{trimmed}\nTrip duration: {formatted}";
        }
    }
}