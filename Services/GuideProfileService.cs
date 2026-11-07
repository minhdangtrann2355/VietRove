
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Models;
using VietRove.Security;

namespace VietRove.Services
{
    public class GuideProfileService : IGuideProfileService
    {
        private readonly VietRoveDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public GuideProfileService(
            VietRoveDbContext context,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        public async Task<GuideProfileViewModel?> GetProfileViewModelAsync(int userId)
        {
            GuideProfile? profile = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (profile == null)
            {
                return null;
            }

            List<Category> provinces = await _context.Categories
                .Where(c => c.CategoryType == "Province" && c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            List<Category> languages = await _context.Categories
                .Where(c => c.CategoryType == "Language" && c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            List<int> provinceIds = await _context.GuideProvinces
                .Where(gp => gp.GuideId == profile.GuideId)
                .Select(gp => gp.ProvinceId)
                .ToListAsync();

            List<Category> selectedCategories = await _context.Categories
                .Where(c =>
                    provinceIds.Contains(c.CategoryId) &&
                    c.CategoryType == "Province")
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            string activityProvinceName = "Chưa cập nhật";

            Category? activityProvince = selectedCategories.FirstOrDefault();

            if (activityProvince != null)
            {
                activityProvinceName = activityProvince.CategoryName;
            }

            List<GuideLanguage> guideLanguages = await _context.GuideLanguages
                .Include(gl => gl.Language)
                .Where(gl => gl.GuideId == profile.GuideId)
                .OrderByDescending(gl => gl.CreatedAt)
                .ToListAsync();

            List<GuideExperience> guideExperiences = await _context.GuideExperiences
                .Where(ge => ge.GuideId == profile.GuideId)
                .OrderByDescending(ge => ge.CreatedAt)
                .ToListAsync();

            string statusText = "Đang chờ duyệt";

            if (profile.Status == (int)ProfileStatus.Approved)
            {
                statusText = "Đã xác thực";
            }
            else if (profile.Status == (int)ProfileStatus.Rejected)
            {
                statusText = "Đã từ chối";
            }
            else if (profile.Status == (int)ProfileStatus.Locked)
            {
                statusText = "Đã khóa";
            }
            else if (profile.Status == (int)ProfileStatus.RevisionRequired)
            {
                statusText = "Yêu cầu cập nhật";
            }

            return new GuideProfileViewModel
            {
                Profile = profile,
                Provinces = provinces,
                Languages = languages,
                SelectedCategories = selectedCategories,
                GuideLanguages = guideLanguages,
                GuideExperiences = guideExperiences,
                StatusText = statusText,
                IsApproved = profile.Status == (int)ProfileStatus.Approved,
                IsRejected = profile.Status == (int)ProfileStatus.Rejected,
                IsRevisionRequired = profile.Status == (int)ProfileStatus.RevisionRequired,
                ActivityProvinceName = activityProvinceName
            };
        }

        public async Task<string> UpdateContactInfoAsync(
            int userId,
            GuideContactEditModel model)
        {
            GuideProfile? profile = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (profile == null)
            {
                return "Không tìm thấy hồ sơ hội viên.";
            }

            if (string.IsNullOrWhiteSpace(model.Phone) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Address))
            {
                return "Vui lòng nhập đầy đủ thông tin liên hệ.";
            }

            string newAvatar = string.Empty;

            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                string avatarError = await _fileStorageService.ValidateAvatarAsync(
                    model.AvatarFile);

                if (string.IsNullOrEmpty(avatarError) == false)
                {
                    return avatarError;
                }

                try
                {
                    newAvatar = await _fileStorageService.SaveFileAsync(model.AvatarFile);
                }
                catch (IOException)
                {
                    return "Không thể lưu ảnh đại diện. Vui lòng thử lại.";
                }
                catch (UnauthorizedAccessException)
                {
                    return "Hệ thống không có quyền lưu ảnh đại diện.";
                }
                catch (InvalidOperationException)
                {
                    return "Ảnh đại diện không hợp lệ.";
                }
            }

            if (model.ProvinceId > 0)
            {
                bool provinceExists = await _context.Categories
                    .AnyAsync(c =>
                        c.CategoryId == model.ProvinceId &&
                        c.CategoryType == "Province" &&
                        c.IsActive);

                if (provinceExists == false)
                {
                    if (string.IsNullOrEmpty(newAvatar) == false)
                    {
                        _fileStorageService.DeleteFile(newAvatar);
                    }

                    return "Tỉnh thành không hợp lệ hoặc đã ngừng hoạt động.";
                }
            }

            string? oldAvatar = profile.Avatar;

            profile.ContactPhone = model.Phone.Trim();
            profile.ContactEmail = model.Email.Trim().ToLowerInvariant();
            profile.Address = model.Address.Trim();

            if (string.IsNullOrEmpty(newAvatar) == false)
            {
                profile.Avatar = newAvatar;
            }

            // cap nhat thoi diem sua doi ho so
            profile.UpdatedAt = DateTime.Now;

            if (model.ProvinceId > 0)
            {
                List<GuideProvince> oldProvinceRows = await _context.GuideProvinces
                    .Where(gp => gp.GuideId == profile.GuideId)
                    .ToListAsync();

                _context.GuideProvinces.RemoveRange(oldProvinceRows);

                _context.GuideProvinces.Add(new GuideProvince
                {
                    GuideId = profile.GuideId,
                    ProvinceId = model.ProvinceId
                });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (string.IsNullOrEmpty(newAvatar) == false)
                {
                    _fileStorageService.DeleteFile(newAvatar);
                }

                return "Không thể cập nhật thông tin. Vui lòng thử lại.";
            }

            if (string.IsNullOrEmpty(newAvatar) == false &&
                string.IsNullOrEmpty(oldAvatar) == false)
            {
                _fileStorageService.DeleteFile(oldAvatar);
            }

            return string.Empty;
        }

        public async Task<string> AddLanguageAsync(
            int userId,
            GuideLanguageAddModel model)
        {
            GuideProfile? profile = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (profile == null)
            {
                return "Không tìm thấy hồ sơ hội viên.";
            }

            bool languageExists = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryId == model.LanguageId &&
                    c.CategoryType == "Language" &&
                    c.IsActive);

            if (languageExists == false)
            {
                return "Ngoại ngữ không hợp lệ hoặc đã ngừng hoạt động.";
            }

            bool alreadyExists = await _context.GuideLanguages
                .AnyAsync(gl =>
                    gl.GuideId == profile.GuideId &&
                    gl.LanguageId == model.LanguageId);

            if (alreadyExists)
            {
                return "Bạn đã thêm ngoại ngữ này trong hồ sơ.";
            }

            string fileError = await _fileStorageService.ValidateEvidenceAsync(
                model.CertificateFile);

            if (string.IsNullOrEmpty(fileError) == false)
            {
                return fileError;
            }

            string savedFileName;

            try
            {
                savedFileName = await _fileStorageService.SaveFileAsync(
                    model.CertificateFile);
            }
            catch (IOException)
            {
                return "Không thể lưu tệp minh chứng. Vui lòng thử lại.";
            }
            catch (UnauthorizedAccessException)
            {
                return "Hệ thống không có quyền lưu tệp minh chứng.";
            }
            catch (InvalidOperationException)
            {
                return "Tệp minh chứng không hợp lệ.";
            }

            GuideLanguage guideLanguage = new GuideLanguage
            {
                GuideId = profile.GuideId,
                LanguageId = model.LanguageId,
                CertificateFile = savedFileName,
                VerificationStatus = 0,
                CreatedAt = DateTime.Now
            };

            _context.GuideLanguages.Add(guideLanguage);
            profile.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _fileStorageService.DeleteFile(savedFileName);

                return "Không thể thêm ngoại ngữ. Vui lòng kiểm tra lại thông tin.";
            }

            return string.Empty;
        }

        public async Task<bool> DeleteLanguageAsync(
            int userId,
            int guideLanguageId)
        {
            GuideProfile? profile = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (profile == null)
            {
                return false;
            }

            GuideLanguage? language = await _context.GuideLanguages
                .FirstOrDefaultAsync(gl =>
                    gl.GuideLanguageId == guideLanguageId &&
                    gl.GuideId == profile.GuideId);

            if (language == null)
            {
                return false;
            }

            string? fileToDelete = language.CertificateFile;

            _context.GuideLanguages.Remove(language);
            profile.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return false;
            }

            if (string.IsNullOrEmpty(fileToDelete) == false)
            {
                _fileStorageService.DeleteFile(fileToDelete);
            }

            return true;
        }

        public async Task<string> AddExperienceAsync(
            int userId,
            GuideExperienceAddModel model)
        {
            GuideProfile? profile = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (profile == null)
            {
                return "Không tìm thấy hồ sơ hội viên.";
            }

            if (model.StartDate.HasValue &&
                model.EndDate.HasValue &&
                model.EndDate.Value < model.StartDate.Value)
            {
                return "Ngày kết thúc không được trước ngày bắt đầu.";
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return "Vui lòng nhập tên kinh nghiệm.";
            }

            string fileError = await _fileStorageService.ValidateEvidenceAsync(
                model.EvidenceFile);

            if (string.IsNullOrEmpty(fileError) == false)
            {
                return fileError;
            }

            string savedFileName;

            try
            {
                savedFileName = await _fileStorageService.SaveFileAsync(
                    model.EvidenceFile);
            }
            catch (IOException)
            {
                return "Không thể lưu tệp minh chứng. Vui lòng thử lại.";
            }
            catch (UnauthorizedAccessException)
            {
                return "Hệ thống không có quyền lưu tệp minh chứng.";
            }
            catch (InvalidOperationException)
            {
                return "Tệp minh chứng không hợp lệ.";
            }

            string description = string.Empty;

            if (string.IsNullOrWhiteSpace(model.Description) == false)
            {
                description = model.Description.Trim();
            }

            GuideExperience experience = new GuideExperience
            {
                GuideId = profile.GuideId,
                Title = model.Title.Trim(),
                Description = description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                EvidenceFile = savedFileName,
                VerificationStatus = 0,
                CreatedAt = DateTime.Now
            };

            _context.GuideExperiences.Add(experience);
            profile.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _fileStorageService.DeleteFile(savedFileName);

                return "Không thể thêm kinh nghiệm. Vui lòng kiểm tra lại thông tin.";
            }

            return string.Empty;
        }

        public async Task<bool> DeleteExperienceAsync(
            int userId,
            int experienceId)
        {
            GuideProfile? profile = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (profile == null)
            {
                return false;
            }

            GuideExperience? experience = await _context.GuideExperiences
                .FirstOrDefaultAsync(ge =>
                    ge.ExperienceId == experienceId &&
                    ge.GuideId == profile.GuideId);

            if (experience == null)
            {
                return false;
            }

            string? fileToDelete = experience.EvidenceFile;

            _context.GuideExperiences.Remove(experience);
            profile.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return false;
            }

            if (string.IsNullOrEmpty(fileToDelete) == false)
            {
                _fileStorageService.DeleteFile(fileToDelete);
            }

            return true;
        }
    }
}
