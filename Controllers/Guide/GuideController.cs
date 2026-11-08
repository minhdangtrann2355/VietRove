
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VietRove.Security;

namespace VietRove.Controllers.Guide
{
    [Authorize(Policy = AppRoles.Guide)]
    public class GuideController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
