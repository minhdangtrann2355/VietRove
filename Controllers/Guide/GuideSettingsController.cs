
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VietRove.Models.Guide;
using VietRove.Security;
using VietRove.Services.Interfaces.Authentication.Login;

namespace VietRove.Controllers.Guide
{
    [Authorize(Policy = AppRoles.Guide)]
    public class GuideSettingsController : Controller
    {
        private readonly ILoginService _loginService;

        public GuideSettingsController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        public async Task<IActionResult> Settings()
        {
            if (!TryGetCurrentUserId(out int userId))
            {
                return Unauthorized();
            }

            GuideAccountSettingsViewModel? model =
                await _loginService.GetGuideAccountSettingsAsync(userId);

            if (model == null)
            {
                return NotFound();
            }

            return View("~/Views/Guide/Settings.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            GuideAccountSettingsViewModel model)
        {
            if (!TryGetCurrentUserId(out int userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                GuideAccountSettingsViewModel? settings =
                    await _loginService.GetGuideAccountSettingsAsync(userId);

                if (settings == null)
                {
                    return NotFound();
                }

                model.Email = settings.Email;
                model.CreatedAt = settings.CreatedAt;

                return View("~/Views/Guide/Settings.cshtml", model);
            }

            string result =
                await _loginService.ChangeGuidePasswordAsync(userId, model);

            if (string.IsNullOrWhiteSpace(result))
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                TempData["SuccessMessage"] =
                    "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";

                return RedirectToAction("Index", "Login");
            }

            TempData["ErrorMessage"] = result;

            return RedirectToAction(nameof(Settings));
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            userId = 0;

            Claim? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return false;
            }

            return int.TryParse(userIdClaim.Value, out userId);
        }
    }
}
