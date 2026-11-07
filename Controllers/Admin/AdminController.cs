
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
        // Dashboard tong quan thong ke
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var model = await _dashboardService.GetDashboardDataAsync();

            return View(model);
        }

        //Quan ly danh sach báo cáo vi phạm
        [HttpGet]
        public async Task<IActionResult> ViolationReports()
        {
            return View();
        }
        
        //Xu ly khien nại vi phạm/tuoc the huong dan vien
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessViolation(int id, string actionType, string notes)
        {
            TempData["SuccessMessage"] = "Đã xử lý báo cáo vi phạm thành công!";
            return RedirectToAction(nameof(ViolationReports));
        }

        // Quan ly hội viên (User / Guide)
        [HttpGet]
        public async Task<IActionResult> UsersManagement()
        {
            return View();
        }

        //Khóa/Mở khóa tài khoản hội viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái tài khoản thành công!";
            return RedirectToAction(nameof(UsersManagement));
        }
    }
}
