
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VietRove.Models.Admin;
using VietRove.Security;
using VietRove.Services.Interfaces.Admin;

namespace VietRove.Controllers.Admin
{
    [Authorize(Policy = AppRoles.Admin)]
    [ResponseCache(
        NoStore = true,
        Location = ResponseCacheLocation.None)]
    public class AdminGuideManagementController : Controller
    {
        private readonly IAdminGuideManagementService _guideManagementService;

        public AdminGuideManagementController(
            IAdminGuideManagementService guideManagementService)
        {
            _guideManagementService = guideManagementService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "Quản lý hướng dẫn viên";

            return View("~/Views/Admin/GuideManagement.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] AdminGuideManagementSearchViewModel model)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu tìm kiếm không hợp lệ."
                });
            }

            if (model.AccountStatus.HasValue &&
                model.AccountStatus.Value != 0 &&
                model.AccountStatus.Value != 1)
            {
                return BadRequest(new
                {
                    message = "Trạng thái tài khoản không hợp lệ."
                });
            }

            if (model.ProfileStatus.HasValue &&
                Enum.IsDefined(
                    typeof(ProfileStatus),
                    model.ProfileStatus.Value) == false)
            {
                return BadRequest(new
                {
                    message = "Trạng thái hồ sơ không hợp lệ."
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

            AdminGuideManagementResultViewModel result =
                await _guideManagementService.GetGuidesAsync(model);

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest();
            }

            AdminGuideManagementDetailViewModel? model =
                await _guideManagementService.GetGuideDetailAsync(userId);

            if (model == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Chi tiết hướng dẫn viên";

            return View("~/Views/Admin/GuideManagementDetail.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(
            [FromForm] int userId,
            [FromForm] string reason)
        {
            if (userId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tài khoản không hợp lệ."
                });
            }

            if (string.IsNullOrWhiteSpace(reason) ||
                reason.Trim().Length > 500)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Lý do khóa phải có nội dung và không vượt quá 500 ký tự."
                });
            }

            bool result = await _guideManagementService.LockGuideAccountAsync(
                userId,
                reason);

            if (result == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể khóa tài khoản. Tài khoản có thể không tồn tại hoặc đã bị khóa."
                });
            }

            return Json(new
            {
                success = true,
                message = "Đã khóa tài khoản hướng dẫn viên."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(
            [FromForm] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tài khoản không hợp lệ."
                });
            }

            bool result =
                await _guideManagementService.UnlockGuideAccountAsync(userId);

            if (result == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể mở khóa tài khoản. Tài khoản có thể không tồn tại hoặc đang hoạt động."
                });
            }

            return Json(new
            {
                success = true,
                message = "Đã mở khóa tài khoản hướng dẫn viên."
            });
        }
    }
}
