
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Models.Admin;
using VietRove.Security;
using VietRove.Services.Interfaces.Admin;

namespace VietRove.Services.Admin
{
    public class AdminLoginHistoryService : IAdminLoginHistoryService
    {
        private readonly VietRoveDbContext _context;

        private const int PageSize = 10;

        public AdminLoginHistoryService(VietRoveDbContext context)
        {
            _context = context;
        }

        public async Task<AdminLoginHistoryResultViewModel> GetLoginHistoryAsync(
            AdminLoginHistorySearchViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            int currentPage = model.Page;

            if (currentPage < 1)
            {
                currentPage = 1;
            }

            var query =
                from history in _context.LoginHistories.AsNoTracking()
                join user in _context.Users.AsNoTracking()
                    on history.UserId equals user.UserId
                join role in _context.Roles.AsNoTracking()
                    on user.RoleId equals role.RoleId
                where user.RoleId == AppRoles.AdminId ||
                      user.RoleId == AppRoles.GuideId
                select new LoginHistoryItemViewModel
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    RoleId = user.RoleId,
                    RoleName = role.RoleName,
                    LoginTime = history.LoginTime
                };

            if (string.IsNullOrWhiteSpace(model.Search) == false)
            {
                string searchText = model.Search.Trim();

                query = query.Where(item =>
                    item.Email.Contains(searchText));
            }

            if (model.RoleId.HasValue)
            {
                query = query.Where(item =>
                    item.RoleId == model.RoleId.Value);
            }

            if (model.FromDate.HasValue)
            {
                DateTime fromDate = model.FromDate.Value.Date;

                query = query.Where(item =>
                    item.LoginTime >= fromDate);
            }

            if (model.ToDate.HasValue)
            {
                DateTime toDateExclusive =
                    model.ToDate.Value.Date.AddDays(1);

                query = query.Where(item =>
                    item.LoginTime < toDateExclusive);
            }

            int totalRecords = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)PageSize);

            if (totalPages < 1)
            {
                totalPages = 1;
            }

            if (currentPage > totalPages)
            {
                currentPage = totalPages;
            }

            List<LoginHistoryItemViewModel> items = await query
                .OrderByDescending(item => item.LoginTime)
                .ThenByDescending(item => item.UserId)
                .Skip((currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return new AdminLoginHistoryResultViewModel
            {
                Items = items,
                TotalRecords = totalRecords,
                CurrentPage = currentPage,
                PageSize = PageSize,
                TotalPages = totalPages
            };
        }
    }
}
