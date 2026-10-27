using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VietRove.Security;

namespace VietRove.Controllers
{
    [Authorize(Policy = AppRoles.Guide)]
    public class GuideController : Controller
    {
        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
