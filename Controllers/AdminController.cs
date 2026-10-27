using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VietRove.Security;

namespace VietRove.Controllers
{
    [Authorize(Policy = AppRoles.Admin)]
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
