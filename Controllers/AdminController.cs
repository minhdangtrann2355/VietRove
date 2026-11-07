using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VietRove.Security;
using VietRove.Services.Interfaces;

namespace VietRove.Controllers
{
    [Authorize(Policy = AppRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly IAdminDashboardService _dashboardService;

        public AdminController(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var model = await _dashboardService.GetDashboardDataAsync();

            return View(model);
        }
    }
}