using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Models.Admin;
using VietRove.Services.Interfaces.Admin;

namespace VietRove.Services.Admin
{
    public class AdminCompetencyReviewService : IAdminCompetencyReviewService
    {
        private readonly VietRoveDbContext _context;

        public AdminCompetencyReviewService(VietRoveDbContext context)
        {
            _context = context;
        }

        public async Task<AdminCompetencyReviewViewModel> GetPendingCompetenciesAsync()
        {
            AdminCompetencyReviewViewModel model = new AdminCompetencyReviewViewModel();

            model.Languages = await _context.GuideLanguages
                .AsNoTracking()
                .Include(gl => gl.Language)
                .Include(gl => gl.GuideProfile)
                    .ThenInclude(g => g.User)
                .Where(gl => gl.VerificationStatus == 0)
                .OrderByDescending(gl => gl.CreatedAt)
                .Select(gl => new AdminPendingLanguageViewModel
                {
                    GuideLanguageId = gl.GuideLanguageId,
                    GuideId = gl.GuideId,
                    GuideName = gl.GuideProfile.FullName,
                    GuideEmail = gl.GuideProfile.User.Email,
                    LanguageName = gl.Language.CategoryName,
                    CertificateFile = gl.CertificateFile,
                    CreatedAt = gl.CreatedAt
                })
                .ToListAsync();

            model.Experiences = await _context.GuideExperiences
                .AsNoTracking()
                .Include(ge => ge.GuideProfile)
                    .ThenInclude(g => g.User)
                .Where(ge => ge.VerificationStatus == 0)
                .OrderByDescending(ge => ge.CreatedAt)
                .Select(ge => new AdminPendingExperienceViewModel
                {
                    ExperienceId = ge.ExperienceId,
                    GuideId = ge.GuideId,
                    GuideName = ge.GuideProfile.FullName,
                    GuideEmail = ge.GuideProfile.User.Email,
                    Title = ge.Title,
                    Description = ge.Description,
                    StartDate = ge.StartDate,
                    EndDate = ge.EndDate,
                    EvidenceFile = ge.EvidenceFile,
                    CreatedAt = ge.CreatedAt
                })
                .ToListAsync();

            return model;
        }

        public async Task<string?> GetLanguageCertificateFileNameAsync(int guideLanguageId)
        {
            return await _context.GuideLanguages
                .AsNoTracking()
                .Where(gl => gl.GuideLanguageId == guideLanguageId)
                .Select(gl => gl.CertificateFile)
                .FirstOrDefaultAsync();
        }

        public async Task<string?> GetExperienceEvidenceFileNameAsync(int experienceId)
        {
            return await _context.GuideExperiences
                .AsNoTracking()
                .Where(ge => ge.ExperienceId == experienceId)
                .Select(ge => ge.EvidenceFile)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ApproveLanguageAsync(int guideLanguageId, int adminId)
        {
            GuideLanguage guideLanguage = await _context.GuideLanguages
                .FirstOrDefaultAsync(gl => gl.GuideLanguageId == guideLanguageId);

            if (guideLanguage == null ||
                guideLanguage.VerificationStatus != 0 ||
                string.IsNullOrWhiteSpace(guideLanguage.CertificateFile))
            {
                return false;
            }

            guideLanguage.VerificationStatus = 1;
            guideLanguage.VerificationReason = null;
            guideLanguage.VerifiedBy = adminId;
            guideLanguage.VerifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectLanguageAsync(
            int guideLanguageId,
            int adminId,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 1000)
            {
                return false;
            }

            GuideLanguage guideLanguage = await _context.GuideLanguages
                .FirstOrDefaultAsync(gl => gl.GuideLanguageId == guideLanguageId);

            if (guideLanguage == null || guideLanguage.VerificationStatus != 0)
            {
                return false;
            }

            guideLanguage.VerificationStatus = 2;
            guideLanguage.VerificationReason = reason.Trim();
            guideLanguage.VerifiedBy = adminId;
            guideLanguage.VerifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ApproveExperienceAsync(int experienceId, int adminId)
        {
            GuideExperience experience = await _context.GuideExperiences
                .FirstOrDefaultAsync(ge => ge.ExperienceId == experienceId);

            if (experience == null ||
                experience.VerificationStatus != 0 ||
                string.IsNullOrWhiteSpace(experience.EvidenceFile))
            {
                return false;
            }

            experience.VerificationStatus = 1;
            experience.VerificationReason = null;
            experience.VerifiedBy = adminId;
            experience.VerifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectExperienceAsync(
            int experienceId,
            int adminId,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 1000)
            {
                return false;
            }

            GuideExperience experience = await _context.GuideExperiences
                .FirstOrDefaultAsync(ge => ge.ExperienceId == experienceId);

            if (experience == null || experience.VerificationStatus != 0)
            {
                return false;
            }

            experience.VerificationStatus = 2;
            experience.VerificationReason = reason.Trim();
            experience.VerifiedBy = adminId;
            experience.VerifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}