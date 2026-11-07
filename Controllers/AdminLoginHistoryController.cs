
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VietRove.Models;
using VietRove.Security;
using VietRove.Services.Interfaces;

namespace VietRove.Controllers
{
    [Authorize(Policy = AppRoles.Admin)]
    [ResponseCache(
        NoStore = true,
        Location = ResponseCacheLocation.None)]
    public class AdminLoginHistoryController : Controller
    {
        private readonly IAdminLoginHistoryService _loginHistoryService;

        public AdminLoginHistoryController(
            IAdminLoginHistoryService loginHistoryService)
        {
            _loginHistoryService = loginHistoryService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "Lịch sử đăng nhập";

            return View("~/Views/Admin/LoginHistory.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] AdminLoginHistorySearchViewModel model)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu tìm kiếm không hợp lệ."
                });
            }

            if (model.RoleId.HasValue &&
                model.RoleId.Value != AppRoles.AdminId &&
                model.RoleId.Value != AppRoles.GuideId)
            {
                return BadRequest(new
                {
                    message = "Vai trò không hợp lệ."
                });
            }

            if (model.Search != null &&
                model.Search.Length > 254)
            {
                return BadRequest(new
                {
                    message = "Từ khóa tìm kiếm quá dài."
                });
            }

            if (model.Page < 1 || model.Page > 1000000)
            {
                return BadRequest(new
                {
                    message = "Số trang không hợp lệ."
                });
            }

            if (model.FromDate.HasValue &&
                model.ToDate.HasValue &&
                model.FromDate.Value.Date > model.ToDate.Value.Date)
            {
                return BadRequest(new
                {
                    message = "Ngày bắt đầu không được lớn hơn ngày kết thúc."
                });
            }

            AdminLoginHistoryResultViewModel result =
                await _loginHistoryService.GetLoginHistoryAsync(model);

            return Json(result);
        }
    }
}
