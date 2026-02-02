using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.Controllers
{
    [Authorize(Roles = "Owner")]
    public class OwnerController : Controller
    {
        // GET: /Owner
        public IActionResult Index()
        {
            return View(); // Szuka Views/Owner/Index.cshtml
        }
    }
}
