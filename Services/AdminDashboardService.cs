using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Models;
using VietRove.Security;
using VietRove.Services.Interfaces;

namespace VietRove.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly VietRoveDbContext _context;

        public AdminDashboardService(VietRoveDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetDashboardDataAsync()
        {
            var model = new AdminDashboardViewModel();

            model.TotalUsers = await _context.Users
                .CountAsync();

            model.TotalGuides = await _context.Users
                .CountAsync(user => user.RoleId == AppRoles.GuideId);

            model.PendingGuideProfiles = await _context.GuideProfiles
                .CountAsync(profile => profile.Status == 0);

            model.PendingViolationReports = await _context.ViolationReports
                .CountAsync(report => report.Status == 0);

            model.PendingGuideLanguages = await _context.GuideLanguages
                .CountAsync(language => language.VerificationStatus == 0);

            model.PendingGuideExperiences = await _context.GuideExperiences
                .CountAsync(experience => experience.VerificationStatus == 0);

            model.RecentLogins = await _context.LoginHistories
                .AsNoTracking()
                .OrderByDescending(history => history.LoginTime)
                .Take(5)
                .Select(history => new AdminDashboardRecentLoginViewModel
                {
                    Email = history.User.Email,
                    RoleName = history.User.Role.RoleName,
                    LoginTime = history.LoginTime
                })
                .ToListAsync();

            return model;
        }
    }
}