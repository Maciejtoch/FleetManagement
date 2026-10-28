using FleetManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetManagement.Core.Models;

namespace FleetManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AuditController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var tenants = await _context.Tenants
                .OrderBy(t => t.Name)
                .ToListAsync();

            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            ViewBag.Tenants = tenants;
            ViewBag.Users = users;
            return View();
        }
    }
}
