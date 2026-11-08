
using VietRove.Models.Admin;

namespace VietRove.Services.Interfaces.Admin
{
    public interface IAdminGuideManagementService
    {
        Task<AdminGuideManagementResultViewModel> GetGuidesAsync(
            AdminGuideManagementSearchViewModel model);

        Task<AdminGuideManagementDetailViewModel?> GetGuideDetailAsync(
            int userId);

        Task<bool> LockGuideAccountAsync(
            int userId,
            string reason);

        Task<bool> UnlockGuideAccountAsync(
            int userId);
    }
}
