using FleetManagement.Core.Models;
using FleetManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace FleetManagement.Controllers
{
    [Authorize]
    public class LocationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public LocationController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // OWNER
        [Authorize(Roles = "Owner")]
        [HttpPost]
        public async Task<IActionResult> Request([FromBody] LocationRequestDto dto)
        {
            var session = new LocationShareSession
            {
                VehicleId = dto.VehicleId,
                StartedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(dto.Minutes)
            };

            _context.LocationShareSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // USER
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Report([FromBody] VehicleLocation dto)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user?.VehicleId == null)
                return BadRequest();

            var activeSession = await _context.LocationShareSessions
                .AnyAsync(s =>
                    s.VehicleId == user.VehicleId &&
                    s.IsActive);

            if (!activeSession)
                return Forbid();

            dto.VehicleId = user.VehicleId.Value;
            _context.VehicleLocations.Add(dto);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // OWNER
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Latest(int vehicleId)
        {
            var loc = await _context.VehicleLocations
                .Where(v => v.VehicleId == vehicleId)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (loc == null)
                return Json(null);

            return Json(new { loc.Latitude, loc.Longitude });
        }

       

    }


}
