using VietRove.Models;

namespace VietRove.Services.Interfaces
{
    public interface IAdminGuideReviewService
    {
        Task<List<AdminGuideReviewListItemViewModel>> GetPendingGuidesAsync();

        Task<AdminGuideReviewDetailViewModel?> GetGuideDetailAsync(int guideId);

        Task<bool> ApproveGuideAsync(int guideId, int adminId);

        Task<bool> RejectGuideAsync(int guideId, string reason);

        Task<string?> GetDocumentFileNameAsync(int guideId, int documentId);

        Task<string?> GetLanguageCertificateFileNameAsync(int guideId, int guideLanguageId);
    }
}