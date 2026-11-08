
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Models.Admin;
using VietRove.Security;
using VietRove.Services.Interfaces.Admin;

namespace VietRove.Services.Admin
{
    public class AdminGuideManagementService : IAdminGuideManagementService
    {
        private readonly VietRoveDbContext _context;

        private const int PageSize = 10;

        public AdminGuideManagementService(VietRoveDbContext context)
        {
            _context = context;
        }

        public async Task<AdminGuideManagementResultViewModel> GetGuidesAsync(
            AdminGuideManagementSearchViewModel model)
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
                from user in _context.Users.AsNoTracking()
                where user.RoleId == AppRoles.GuideId
                join profile in _context.GuideProfiles.AsNoTracking()
                    on user.UserId equals profile.UserId into profileGroup
                from profile in profileGroup.DefaultIfEmpty()
                select new
                {
                    User = user,
                    Profile = profile
                };

            if (string.IsNullOrWhiteSpace(model.Search) == false)
            {
                string searchText = model.Search.Trim();

                query = query.Where(item =>
                    item.User.Email.Contains(searchText) ||
                    (item.Profile != null &&
                     item.Profile.FullName.Contains(searchText)) ||
                    (item.Profile != null &&
                     item.Profile.ContactPhone != null &&
                     item.Profile.ContactPhone.Contains(searchText)));
            }

            if (model.AccountStatus.HasValue)
            {
                query = query.Where(item =>
                    item.User.Status == model.AccountStatus.Value);
            }

            if (model.ProfileStatus.HasValue)
            {
                query = query.Where(item =>
                    item.Profile != null &&
                    item.Profile.Status == model.ProfileStatus.Value);
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

            var pageUsers = await query
                .OrderByDescending(item => item.User.CreatedAt)
                .ThenByDescending(item => item.User.UserId)
                .Skip((currentPage - 1) * PageSize)
                .Take(PageSize)
                .Select(item => new
                {
                    item.User.UserId,
                    item.User.Email,
                    item.User.Status,
                    item.User.LockReason,
                    item.User.CreatedAt
                })
                .ToListAsync();

            List<int> userIds = pageUsers
                .Select(item => item.UserId)
                .ToList();

            var profiles = await _context.GuideProfiles
                .AsNoTracking()
                .Where(item => userIds.Contains(item.UserId))
                .ToListAsync();

            var profileMap = profiles.ToDictionary(item => item.UserId);

            List<AdminGuideManagementListItemViewModel> items =
                new List<AdminGuideManagementListItemViewModel>();

            foreach (var user in pageUsers)
            {
                AdminGuideManagementListItemViewModel item =
                    new AdminGuideManagementListItemViewModel
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        AccountStatus = user.Status,
                        LockReason = user.LockReason,
                        CreatedAt = user.CreatedAt
                    };

                if (profileMap.ContainsKey(user.UserId))
                {
                    var profile = profileMap[user.UserId];

                    item.GuideId = profile.GuideId;
                    item.FullName = profile.FullName;
                    item.ContactPhone = profile.ContactPhone;
                    item.ProfileStatus = profile.Status;
                }
                else
                {
                    item.FullName = user.Email;
                }

                items.Add(item);
            }

            return new AdminGuideManagementResultViewModel
            {
                Items = items,
                TotalRecords = totalRecords,
                CurrentPage = currentPage,
                PageSize = PageSize,
                TotalPages = totalPages
            };
        }

        public async Task<AdminGuideManagementDetailViewModel?> GetGuideDetailAsync(
            int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(item =>
                    item.UserId == userId &&
                    item.RoleId == AppRoles.GuideId);

            if (user == null)
            {
                return null;
            }

            var profile = await _context.GuideProfiles
                .AsNoTracking()
                .Include(item => item.CardType)
                .Include(item => item.GuideProvinces)
                    .ThenInclude(item => item.Province)
                .Include(item => item.GuideLanguages)
                    .ThenInclude(item => item.Language)
                .Include(item => item.GuideExperiences)
                .FirstOrDefaultAsync(item => item.UserId == userId);

            AdminGuideManagementDetailViewModel result =
                new AdminGuideManagementDetailViewModel
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    AccountStatus = user.Status,
                    LockReason = user.LockReason,
                    AccountCreatedAt = user.CreatedAt
                };

            if (profile == null)
            {
                return result;
            }

            result.GuideId = profile.GuideId;
            result.FullName = profile.FullName;
            result.Gender = profile.Gender;
            result.DateOfBirth = profile.DateOfBirth;
            result.Address = profile.Address;
            result.ContactPhone = profile.ContactPhone;
            result.ContactEmail = profile.ContactEmail;
            result.ExperienceYears = profile.ExperienceYears;
            result.ProfileStatus = profile.Status;
            result.RejectReason = profile.RejectReason;
            result.CardNumber = profile.CardNumber;
            result.IssueDate = profile.IssueDate;
            result.ExpiryDate = profile.ExpiryDate;

            if (profile.CardType != null)
            {
                result.CardTypeName = profile.CardType.CategoryName;
            }

            foreach (var province in profile.GuideProvinces)
            {
                if (province.Province != null)
                {
                    result.Provinces.Add(province.Province.CategoryName);
                }
            }

            foreach (var language in profile.GuideLanguages)
            {
                AdminGuideLanguageItemViewModel languageItem =
                    new AdminGuideLanguageItemViewModel
                    {
                        VerificationStatus = language.VerificationStatus,
                        VerificationReason = language.VerificationReason,
                        HasCertificate = string.IsNullOrWhiteSpace(
                            language.CertificateFile) == false
                    };

                if (language.Language != null)
                {
                    languageItem.LanguageName =
                        language.Language.CategoryName;
                }

                result.Languages.Add(languageItem);
            }

            foreach (var experience in profile.GuideExperiences)
            {
                result.Experiences.Add(
                    new AdminGuideExperienceItemViewModel
                    {
                        Title = experience.Title,
                        Description = experience.Description,
                        StartDate = experience.StartDate,
                        EndDate = experience.EndDate,
                        VerificationStatus = experience.VerificationStatus,
                        VerificationReason = experience.VerificationReason,
                        HasEvidence = string.IsNullOrWhiteSpace(
                            experience.EvidenceFile) == false
                    });
            }

            return result;
        }

        public async Task<bool> LockGuideAccountAsync(
            int userId,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason) ||
                reason.Trim().Length > 500)
            {
                return false;
            }

            var user = await _context.Users.FirstOrDefaultAsync(item =>
                item.UserId == userId &&
                item.RoleId == AppRoles.GuideId);

            if (user == null || user.Status != 1)
            {
                return false;
            }

            user.Status = 0;
            user.LockReason = reason.Trim();

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnlockGuideAccountAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(item =>
                item.UserId == userId &&
                item.RoleId == AppRoles.GuideId);

            if (user == null || user.Status != 0)
            {
                return false;
            }

            user.Status = 1;
            user.LockReason = null;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
