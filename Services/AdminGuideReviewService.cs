using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Models;
using VietRove.Security;
using VietRove.Services.Interfaces;

namespace VietRove.Services
{
    public class AdminGuideReviewService : IAdminGuideReviewService
    {
        private readonly VietRoveDbContext _context;

        public AdminGuideReviewService(VietRoveDbContext context)
        {
            _context = context;
        }

        public async Task<List<AdminGuideReviewListItemViewModel>> GetPendingGuidesAsync()
        {
            var guides = await _context.GuideProfiles
                .AsNoTracking()
                .Include(g => g.User)
                .Include(g => g.CardType)
                .Include(g => g.GuideProvinces)
                    .ThenInclude(gp => gp.Province)
                .Where(g => g.Status == (int)ProfileStatus.Pending)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            var result = new List<AdminGuideReviewListItemViewModel>();

            foreach (var guide in guides)
            {
                string provinces = string.Join(
                    ", ",
                    guide.GuideProvinces
                        .Where(gp => gp.Province != null)
                        .Select(gp => gp.Province.CategoryName)
                );

                result.Add(new AdminGuideReviewListItemViewModel
                {
                    GuideId = guide.GuideId,
                    FullName = guide.FullName,
                    Email = guide.User.Email,
                    CardType = guide.CardType.CategoryName,
                    CreatedAt = guide.CreatedAt,
                    Provinces = provinces
                });
            }

            return result;
        }

        public async Task<AdminGuideReviewDetailViewModel?> GetGuideDetailAsync(int guideId)
        {
            var guide = await _context.GuideProfiles
                .AsNoTracking()
                .Include(g => g.User)
                .Include(g => g.CardType)
                .Include(g => g.GuideProvinces)
                    .ThenInclude(gp => gp.Province)
                .Include(g => g.GuideDocuments)
                .Include(g => g.GuideLanguages)
                    .ThenInclude(gl => gl.Language)
                .FirstOrDefaultAsync(g => g.GuideId == guideId);

            if (guide == null)
            {
                return null;
            }

            var model = new AdminGuideReviewDetailViewModel
            {
                GuideId = guide.GuideId,
                UserId = guide.UserId,
                FullName = guide.FullName,
                Email = guide.User.Email,
                Gender = guide.Gender,
                DateOfBirth = guide.DateOfBirth,
                Address = guide.Address,
                ContactPhone = guide.ContactPhone,
                ContactEmail = guide.ContactEmail,
                ExperienceYears = guide.ExperienceYears,
                CardType = guide.CardType.CategoryName,
                CardNumber = guide.CardNumber,
                IssueDate = guide.IssueDate,
                ExpiryDate = guide.ExpiryDate,
                IdentityNumber = guide.IdentityNumber,
                CreatedAt = guide.CreatedAt
            };

            foreach (var province in guide.GuideProvinces)
            {
                if (province.Province != null)
                {
                    model.Provinces.Add(province.Province.CategoryName);
                }
            }

            foreach (var document in guide.GuideDocuments)
            {
                string originalFileName = document.FileName;

                if (!string.IsNullOrWhiteSpace(document.OriginalFileName))
                {
                    originalFileName = document.OriginalFileName;
                }

                model.Documents.Add(new AdminGuideReviewDocumentViewModel
                {
                    DocumentId = document.DocumentId,
                    DocumentType = document.DocumentType,
                    OriginalFileName = originalFileName,
                    UploadedAt = document.UploadedAt,
                    VerificationStatus = document.VerificationStatus,
                    VerificationReason = document.VerificationReason
                });
            }

            foreach (var language in guide.GuideLanguages)
            {
                string languageName = string.Empty;

                if (language.Language != null)
                {
                    languageName = language.Language.CategoryName;
                }

                model.Languages.Add(new AdminGuideReviewLanguageViewModel
                {
                    GuideLanguageId = language.GuideLanguageId,
                    LanguageName = languageName,
                    CertificateFile = language.CertificateFile,
                    VerificationStatus = language.VerificationStatus,
                    VerificationReason = language.VerificationReason
                });
            }

            return model;
        }

        public async Task<bool> ApproveGuideAsync(int guideId, int adminId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var guide = await _context.GuideProfiles
                .Include(g => g.GuideDocuments)
                .Include(g => g.GuideLanguages)
                .FirstOrDefaultAsync(g => g.GuideId == guideId);

            if (guide == null || guide.Status != (int)ProfileStatus.Pending)
            {
                await transaction.RollbackAsync();
                return false;
            }

            int[] requiredDocumentTypes = { 1, 2, 3 };

            foreach (int requiredType in requiredDocumentTypes)
            {
                bool documentExists = guide.GuideDocuments.Any(
                    d => d.DocumentType == requiredType
                );

                if (!documentExists)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            DateTime now = DateTime.Now;

            guide.Status = (int)ProfileStatus.Approved;
            guide.RejectReason = null;
            guide.UpdatedAt = now;

            foreach (var document in guide.GuideDocuments)
            {
                document.VerificationStatus = 1;
                document.VerifiedBy = adminId;
                document.VerifiedAt = now;
                document.VerificationReason = null;
            }

            foreach (var language in guide.GuideLanguages)
            {
                language.VerificationStatus = 1;
                language.VerifiedBy = adminId;
                language.VerifiedAt = now;
                language.VerificationReason = null;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }

        public async Task<bool> RejectGuideAsync(int guideId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return false;
            }

            reason = reason.Trim();

            if (reason.Length > 1000)
            {
                return false;
            }

            var guide = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.GuideId == guideId);

            if (guide == null || guide.Status != (int)ProfileStatus.Pending)
            {
                return false;
            }

            guide.Status = (int)ProfileStatus.Rejected;
            guide.RejectReason = reason;
            guide.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string?> GetDocumentFileNameAsync(int guideId, int documentId)
        {
            return await _context.GuideDocuments
                .Where(d => d.GuideId == guideId && d.DocumentId == documentId)
                .Select(d => d.FileName)
                .FirstOrDefaultAsync();
        }

        public async Task<string?> GetLanguageCertificateFileNameAsync(
            int guideId,
            int guideLanguageId)
        {
            return await _context.GuideLanguages
                .Where(l => l.GuideId == guideId &&
                            l.GuideLanguageId == guideLanguageId)
                .Select(l => l.CertificateFile)
                .FirstOrDefaultAsync();
        }
    }
}