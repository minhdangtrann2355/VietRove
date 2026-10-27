using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Models;
using VietRove.Entities;
using VietRove.Security;

namespace VietRove.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly VietRoveDbContext _context;

        public LoginController(VietRoveDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid == false)
            {
                return View(model);
            }

            string cleanEmail = string.Empty;
            if (model.Email != null)
            {
                cleanEmail = model.Email.Trim().ToLowerInvariant();
            }

            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == cleanEmail);
            if (user == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
            if (isPasswordValid == false)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            if (user.Status != 1)
            {
                ModelState.AddModelError("", "Tài khoản chưa được kích hoạt hoặc đã bị khóa.");
                return View(model);
            }

            string roleName = string.Empty;
            string dashboardController = string.Empty;

            if (user.RoleId == AppRoles.AdminId)
            {
                roleName = AppRoles.Admin;
                dashboardController = "Admin";
            }
            else if (user.RoleId == AppRoles.GuideId)
            {
                roleName = AppRoles.Guide;
                dashboardController = "Guide";
            }
            else
            {
                ModelState.AddModelError("", "Tài khoản không có quyền truy cập hệ thống.");
                return View(model);
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, roleName)
            };
            ClaimsIdentity identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction("Dashboard", dashboardController);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Index));
        }
    }
}