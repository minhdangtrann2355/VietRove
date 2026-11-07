using System.Threading.Tasks;
using VietRove.Models.Admin;

namespace VietRove.Services.Interfaces.Admin
{
    public interface IAdminCompetencyReviewService
    {
        Task<AdminCompetencyReviewViewModel> GetPendingCompetenciesAsync();

        Task<string?> GetLanguageCertificateFileNameAsync(int guideLanguageId);

        Task<string?> GetExperienceEvidenceFileNameAsync(int experienceId);

        Task<bool> ApproveLanguageAsync(int guideLanguageId, int adminId);

        Task<bool> RejectLanguageAsync(int guideLanguageId, int adminId, string reason);

        Task<bool> ApproveExperienceAsync(int experienceId, int adminId);

        Task<bool> RejectExperienceAsync(int experienceId, int adminId, string reason);
    }
}