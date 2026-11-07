
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Models;
using VietRove.Security;
using VietRove.Services.Interfaces;

namespace VietRove.Services
{
    public class LoginService : ILoginService
    {
        private readonly VietRoveDbContext _context;

        public LoginService(VietRoveDbContext context)
        {
            _context = context;
        }

        public async Task<AuthenticationResult> AuthenticateAsync(
            LoginViewModel model)
        {
            string cleanEmail = string.Empty;

            if (model.Email != null)
            {
                cleanEmail = model.Email.Trim().ToLowerInvariant();
            }

            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == cleanEmail);

            if (user == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Email hoặc mật khẩu không chính xác."
                };
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
                model.Password,
                user.Password);

            if (isPasswordValid == false)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Email hoặc mật khẩu không chính xác."
                };
            }

            if (user.Status != 1)
            {
                string lockMessage =
                    "Tài khoản của bạn hiện không hoạt động.";

                if (string.IsNullOrWhiteSpace(user.LockReason) == false)
                {
                    lockMessage =
                        $"Tài khoản của bạn đã bị khóa vì vi phạm: {user.LockReason}";
                }

                return new AuthenticationResult
                {
                    Success = false,
                    Message = lockMessage
                };
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
                dashboardController = "Home";
            }
            else
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Tài khoản không có quyền truy cập hệ thống."
                };
            }

            GuideProfile? guide = null;

            if (user.RoleId == AppRoles.GuideId)
            {
                guide = await _context.GuideProfiles
                    .FirstOrDefaultAsync(g => g.UserId == user.UserId);
            }

            LoginHistory loginHistory = new LoginHistory
            {
                UserId = user.UserId,
                LoginTime = DateTime.Now
            };

            _context.LoginHistories.Add(loginHistory);
            await _context.SaveChangesAsync();

            return new AuthenticationResult
            {
                Success = true,
                User = user,
                GuideProfile = guide,
                RoleName = roleName,
                DashboardController = dashboardController
            };
        }

        public async Task<GuideAccountSettingsViewModel?> GetGuideAccountSettingsAsync(
            int userId)
        {
            User? user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.UserId == userId &&
                    u.RoleId == AppRoles.GuideId);

            if (user == null)
            {
                return null;
            }

            return new GuideAccountSettingsViewModel
            {
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<string> ChangeGuidePasswordAsync(
            int userId,
            GuideAccountSettingsViewModel model)
        {
            User? user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserId == userId &&
                    u.RoleId == AppRoles.GuideId);

            if (user == null)
            {
                return "Không tìm thấy tài khoản hội viên.";
            }

            if (user.Status != 1)
            {
                return "Tài khoản hiện không hoạt động.";
            }

            bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(
                model.CurrentPassword,
                user.Password);

            if (isCurrentPasswordValid == false)
            {
                return "Mật khẩu hiện tại không chính xác.";
            }

            if (BCrypt.Net.BCrypt.Verify(model.NewPassword, user.Password))
            {
                return "Mật khẩu mới phải khác mật khẩu hiện tại.";
            }

            string email = user.Email.Trim();
            string emailName = email;

            int atIndex = email.IndexOf('@');

            if (atIndex >= 0)
            {
                emailName = email.Substring(0, atIndex);
            }

            if (model.NewPassword.Contains(
                email,
                StringComparison.OrdinalIgnoreCase))
            {
                return "Mật khẩu mới không được chứa email đăng nhập.";
            }

            if (emailName.Length >= 3 &&
                model.NewPassword.Contains(
                    emailName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Mật khẩu mới không được chứa tên tài khoản email.";
            }

            GuideProfile? guide = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (guide != null &&
                string.IsNullOrWhiteSpace(guide.FullName) == false)
            {
                string[] nameParts = guide.FullName.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (string namePart in nameParts)
                {
                    if (namePart.Length >= 3 &&
                        model.NewPassword.Contains(
                            namePart,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return "Mật khẩu mới không được chứa các phần họ tên của bạn.";
                    }
                }
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(
                model.NewPassword,
                workFactor: 12);

            await _context.SaveChangesAsync();

            return string.Empty;
        }
    }
}
