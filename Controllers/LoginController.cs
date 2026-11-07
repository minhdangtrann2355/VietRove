
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VietRove.Entities;
using VietRove.Models;
using VietRove.Security;
using VietRove.Services.Interfaces;

namespace VietRove.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (Request.Cookies.TryGetValue("SysAuthMsg", out string authMsg))
            {
                if (authMsg == "approved")
                {
                    ViewBag.SysMessage =
                        "Tài khoản của bạn đã được duyệt, vui lòng đăng nhập lại.";

                    ViewBag.SysMessageType = "success";
                }
                else if (authMsg == "locked")
                {
                    ViewBag.SysMessage =
                        "Phiên đăng nhập đã kết thúc do tài khoản bị khóa.";

                    ViewBag.SysMessageType = "danger";
                }

                Response.Cookies.Delete("SysAuthMsg");
            }

            return View("Login", new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid == false)
            {
                return View("Login", model);
            }

            AuthenticationResult result =
                await _loginService.AuthenticateAsync(model);

            if (result.Success == false)
            {
                ModelState.AddModelError("", result.Message);
                return View("Login", model);
            }

            if (result.User == null)
            {
                ModelState.AddModelError(
                    "",
                    "Không thể xác thực tài khoản.");

                return View("Login", model);
            }

            string dashboardController = string.Empty;

            if (result.RoleName == AppRoles.Admin)
            {
                dashboardController = "Admin";
            }
            else if (result.RoleName == AppRoles.Guide)
            {
                dashboardController = "Guide";
            }
            else
            {
                ModelState.AddModelError(
                    "",
                    "Vai trò tài khoản không hợp lệ.");

                return View("Login", model);
            }

            User user = result.User;
            GuideProfile guide = result.GuideProfile;

            string fullName = user.Email.Split('@')[0];
            int profileStatus = -1;
            string cardNumber = string.Empty;
            string avatar = string.Empty;

            if (guide != null)
            {
                if (string.IsNullOrWhiteSpace(guide.FullName) == false)
                {
                    fullName = guide.FullName;
                }

                profileStatus = guide.Status;

                if (string.IsNullOrWhiteSpace(guide.CardNumber) == false)
                {
                    cardNumber = guide.CardNumber;
                }

                if (string.IsNullOrWhiteSpace(guide.Avatar) == false)
                {
                    avatar = guide.Avatar;
                }
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()),

                new Claim(
                    ClaimTypes.Role,
                    result.RoleName),

                new Claim(
                    ClaimTypes.Name,
                    fullName),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    "UserStatus",
                    user.Status.ToString()),

                new Claim(
                    "ProfileStatus",
                    profileStatus.ToString()),

                new Claim(
                    "CardNumber",
                    cardNumber),

                new Claim(
                    "Avatar",
                    avatar),

                new Claim(
                    "LastValidatedUtc",
                    DateTime.UtcNow.ToString("o"))
            };

            ClaimsIdentity identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            AuthenticationProperties authProperties =
                new AuthenticationProperties
                {
                    IsPersistent = false
                };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            return RedirectToAction(
                "Dashboard",
                dashboardController);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}
