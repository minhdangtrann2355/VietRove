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
                dashboardController = "Guide";
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

            // ghi nhan lich su dang nhap thanh cong vao csdl
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
    }
}