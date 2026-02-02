using FleetManagement.Core.Models;
using FleetManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Owner")]
public class StopsController : Controller
{
    private readonly AppDbContext _context;

    public StopsController(AppDbContext context)
    {
        _context = context;
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
}

