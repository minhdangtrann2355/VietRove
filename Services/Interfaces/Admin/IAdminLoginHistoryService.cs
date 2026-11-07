using VietRove.Models.Admin;

namespace VietRove.Services.Interfaces.Admin
{
    public interface IAdminLoginHistoryService
    {
        Task<AdminLoginHistoryResultViewModel> GetLoginHistoryAsync(
            AdminLoginHistorySearchViewModel model);
    }
}