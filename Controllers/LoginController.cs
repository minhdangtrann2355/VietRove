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

            string cleanEmail = string.Empty;
            if (model.Email != null)
            {
                cleanEmail = model.Email.Trim().ToLowerInvariant();
            }

            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == cleanEmail);
            if (user == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
                return View("Login", model);
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
            if (isPasswordValid == false)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
                return View("Login", model);
            }

            if (user.Status == 2)
            {
                ModelState.AddModelError("", "Hồ sơ của bạn đã bị từ chối.");
                return View("Login", model);
            }

            if (user.Status == 3)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                return View("Login", model);
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
                return View("Login", model);
            }

            //lay thong tin ho so hoi vien de nap vao claims
            GuideProfile? guide = null;
            if (user.RoleId == AppRoles.GuideId)
            {
                guide = await _context.GuideProfiles.FirstOrDefaultAsync(g => g.UserId == user.UserId);
            }

            string fullName = string.Empty;
            if (guide != null && string.IsNullOrWhiteSpace(guide.FullName) == false)
            {
                fullName = guide.FullName;
            }
            else
            {
                fullName = user.Email.Split('@')[0];
            }

            string status = string.Empty;
            if (guide != null)
            {
                status = guide.Status.ToString();
            }
            else
            {
                status = user.Status.ToString();
            }

            string cardNumber = string.Empty;
            if (guide != null && string.IsNullOrWhiteSpace(guide.CardNumber) == false)
            {
                cardNumber = guide.CardNumber;
            }

            string avatar = string.Empty;
            if (guide != null && string.IsNullOrWhiteSpace(guide.Avatar) == false)
            {
                avatar = guide.Avatar;
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, roleName),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Status", status),
                new Claim("CardNumber", cardNumber),
                new Claim("Avatar", avatar)
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
            return RedirectToAction("Index", "Home");
        }
    }
}
