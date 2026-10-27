using FleetManagement.Core.Models;
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

            report.TripDurationMinutes = ParseTripDuration(report.Notes);
            return View(report);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var report = await _context.DailyReports
                .Include(r => r.Vehicle)
                .Include(r => r.Stops)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                return NotFound();

            var model = new FleetManagement.Models.DailyReportViewModel
            {
                VehicleId = report.VehicleId,
                VehicleRegistration = report.Vehicle?.RegistrationNumber,
                Mileage = report.Mileage,
                TripDurationMinutes = report.TripDurationMinutes ?? ParseTripDuration(report.Notes) ?? 0,
                Notes = report.Notes,
                SelectedStopIds = report.Stops.Select(s => s.StopId).ToList(),
                Stops = await _context.Stops.ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FleetManagement.Models.DailyReportViewModel model)
        {
            var report = await _context.DailyReports
                .Include(r => r.Stops)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.Stops = await _context.Stops.ToListAsync();
                return View(model);
            }

            if (model.SelectedStopIds == null || !model.SelectedStopIds.Any())
            {
                ModelState.AddModelError(nameof(model.SelectedStopIds), "Select at least one stop.");
                model.Stops = await _context.Stops.ToListAsync();
                return View(model);
            }

            var notes = model.Notes ?? string.Empty;
            if (model.TripDurationMinutes > 0)
            {
                notes = AddTripDurationToNotes(notes, model.TripDurationMinutes);
            }

            report.Mileage = model.Mileage;
            report.Notes = notes;
            report.TripDurationMinutes = model.TripDurationMinutes > 0 ? model.TripDurationMinutes : ParseTripDuration(notes);

            _context.DailyReportStops.RemoveRange(report.Stops);
            report.Stops = model.SelectedStopIds.Select(stopId => new DailyReportStop { StopId = stopId }).ToList();

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = report.Id });
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
            var formatted = FleetManagement.Core.Models.DailyReport.FormatDuration(minutes);
            var trimmed = notes?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(trimmed))
                return $"Trip duration: {formatted}";

            if (trimmed.Contains("Trip duration:", StringComparison.OrdinalIgnoreCase))
                return trimmed;

            return $"{trimmed}\nTrip duration: {formatted}";
        }
    }
}
