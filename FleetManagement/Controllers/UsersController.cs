using FleetManagement.Core.Models;
using FleetManagement.Data;
using FleetManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;


namespace FleetManagement.Controllers
{
    [Authorize(Roles = "Owner")]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public UsersController(
    UserManager<AppUser> userManager,
    AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // LISTA USERÓW
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();

            var model = users.Select(u => new UserListViewModel
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Role = _userManager.GetRolesAsync(u).Result.FirstOrDefault() ?? "-",
                IsActive = u.LockoutEnd == null
            }).ToList();

            return View(model);
        }


        // FORMULARZ
        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> EditRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            return View(new EditUserRoleViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Role = role
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditUserRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);

            return RedirectToAction(nameof(Index));
        }



        // CREATE USER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }


        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var vehicles = _context.Vehicles.ToList();

            return View(new EditUserViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                VehicleId = user.VehicleId,
                Vehicles = vehicles
            }
                );
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.VehicleId = model.VehicleId;

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(EditUserSecurityViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, model.NewPassword);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user?.VehicleId == null)
                return View(null);

            var session = await _context.LocationShareSessions
                .Where(s => s.VehicleId == user.VehicleId && s.IsActive)
                .OrderByDescending(s => s.ExpiresAt)
                .FirstOrDefaultAsync();

            return View(session);
        }

    }
}
