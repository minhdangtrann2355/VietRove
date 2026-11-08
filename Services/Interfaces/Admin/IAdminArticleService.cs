using System.Threading.Tasks;
using VietRove.Models.Admin;

namespace VietRove.Services.Interfaces.Admin
{
    public interface IAdminArticleService
    {
        Task<AdminArticleResultViewModel> GetArticlesAsync(
            AdminArticleSearchViewModel model);

        Task<AdminArticleDetailViewModel?> GetArticleDetailAsync(
            int articleId);

        Task<string?> GetThumbnailFileNameAsync(
            int articleId);

        Task<(bool Success, string Message)> CreateArticleAsync(
            AdminArticleCreateViewModel model,
            int adminId);

        Task<(bool Success, string Message)> UpdateArticleAsync(
            AdminArticleUpdateViewModel model);

        Task<(bool Success, string Message)> ChangeArticleStatusAsync(
            int articleId,
            int status);
    }
}