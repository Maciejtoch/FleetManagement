using FleetManagement.Core.Models;
using FleetManagement.Data;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


namespace FleetManagement.Controllers
{
    [Authorize(Roles = "Owner")]
    public class ServiceRecordsController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceRecordsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var records = await _context.ServiceRecords
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.ServiceDate)
                .ToListAsync();

            return View(records);
        }

        public IActionResult Create()
        {
            ViewBag.Vehicles = _context.Vehicles.ToList();
            return View(new ServiceRecord
            {
                ServiceDate = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRecord record)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}");

                throw new Exception("MODELSTATE INVALID: " + string.Join(" | ", errors));
            }

            var vehicleExists = await _context.Vehicles
                .AnyAsync(v => v.Id == record.VehicleId);

            if (!vehicleExists)
                throw new Exception("VEHICLE NOT FOUND");

            _context.ServiceRecords.Add(record);
            await _context.SaveChangesAsync();

            
            return RedirectToAction("Index", "ServiceRecords");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var record = await _context.ServiceRecords
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (record == null)
                return NotFound();

            ViewBag.Vehicles = _context.Vehicles.ToList();
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceRecord record)
        {
            if (id != record.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Vehicles = _context.Vehicles.ToList();
                return View(record);
            }

            var exists = await _context.ServiceRecords.AnyAsync(r => r.Id == id);
            if (!exists)
                return NotFound();

            _context.Update(record);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.ServiceRecords
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (record == null)
                return NotFound();

            return View(record);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var record = await _context.ServiceRecords.FindAsync(id);

            if (record == null)
                return NotFound();

            _context.ServiceRecords.Remove(record);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
