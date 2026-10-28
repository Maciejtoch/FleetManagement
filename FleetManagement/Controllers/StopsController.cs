using FleetManagement.Core.Models;
using FleetManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Owner")]
public class StopsController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public StopsController(AppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public class NominatimResult
    {
        public string Lat { get; set; }
        public string Lon { get; set; }
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Stops.ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Stop stop)
    {
        if (!ModelState.IsValid) return View(stop);

        _context.Stops.Add(stop);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var stop = await _context.Stops.FindAsync(id);
        if (stop == null) return NotFound();
        return View(stop);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Stop stop)
    {
        if (!ModelState.IsValid) return View(stop);

        _context.Update(stop);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var stop = await _context.Stops.FindAsync(id);
        if (stop == null) return NotFound();
        return View(stop);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var stop = await _context.Stops.FindAsync(id);
        _context.Stops.Remove(stop);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var stop = await _context.Stops.FindAsync(id);
        if (stop == null) return NotFound();
        return View(stop);
    }

    private async Task<(double lat, double lon)?> GeocodeAddress(string address)
    {
        using var client = new HttpClient();

        var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}";
        client.DefaultRequestHeaders.Add("User-Agent", "FleetManagementApp");

        var result = await client.GetFromJsonAsync<List<NominatimResult>>(url);

        if (result == null || result.Count == 0)
            return null;

        var lat = double.Parse(result[0].Lat, System.Globalization.CultureInfo.InvariantCulture);
        var lon = double.Parse(result[0].Lon, System.Globalization.CultureInfo.InvariantCulture);

        return (lat, lon);
    }

    public async Task<IActionResult> AddToMap(int id)
    {
        var stop = await _context.Stops.FindAsync(id);
        if (stop == null) return NotFound();

        var coords = await GeocodeAddress(stop.Address);

        if (coords == null)
        {
            TempData["Error"] = "Nie znaleziono adresu.";
            return RedirectToAction(nameof(Index));
        }

        stop.Latitude = coords.Value.lat;
        stop.Longitude = coords.Value.lon;
        stop.IsOnMap = true;

        _context.Update(stop);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> RemoveFromMap(int id)
    {
        var stop = await _context.Stops.FindAsync(id);
        if (stop == null) return NotFound();

        stop.IsOnMap = false;
        stop.Latitude = null;
        stop.Longitude = null;

        _context.Update(stop);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetMapStops()
    {
        var stops = await _context.Stops
            .Where(s => s.IsOnMap && s.Latitude != null && s.Longitude != null)
            .Select(s => new {
                s.CompanyName,
                s.Address,
                s.Latitude,
                s.Longitude
            })
            .ToListAsync();

        return Json(stops);
    }
}

