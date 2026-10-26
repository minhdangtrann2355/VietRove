using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Models;
using VietRove.Entities;

namespace VietRove.Controllers
{
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

            User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == cleanEmail);
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

            return RedirectToAction("Index", "Home");
        }
    }
}