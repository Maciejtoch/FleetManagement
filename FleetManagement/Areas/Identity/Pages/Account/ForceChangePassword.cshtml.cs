using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FleetManagement.Core.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class ForceChangePasswordModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public ForceChangePasswordModel(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToPage("/Account/Login");

        await _userManager.RemovePasswordAsync(user);
        await _userManager.AddPasswordAsync(user, NewPassword);

        user.MustChangePassword = false;
        await _userManager.UpdateAsync(user);

        await _signInManager.RefreshSignInAsync(user);

        return RedirectToPage("/Index", "Dashboard");
    }
}