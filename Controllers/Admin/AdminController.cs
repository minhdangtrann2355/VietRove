
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VietRove.Security;
using VietRove.Services.Interfaces.Admin;

namespace VietRove.Controllers.Admin
{
    [Authorize(Policy = AppRoles.Admin)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AdminController : Controller
    {
        private readonly IAdminDashboardService _dashboardService;

        public AdminController(
            IAdminDashboardService dashboardService)
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
