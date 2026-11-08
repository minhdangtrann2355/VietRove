
using System.Threading.Tasks;
using VietRove.Models.Admin;

namespace VietRove.Services.Interfaces.Admin
{
    public interface IAdminCategoryService
    {
        Task<AdminCategoryResultViewModel> GetCategoriesAsync(
            AdminCategorySearchViewModel model);

        Task<(bool Success, string Message)> CreateCategoryAsync(
            AdminCategoryCreateViewModel model);

        Task<(bool Success, string Message)> UpdateCategoryAsync(
            int categoryId,
            AdminCategoryUpdateViewModel model);

        Task<(bool Success, string Message)> ToggleCategoryStatusAsync(
            int categoryId);
    }
}
