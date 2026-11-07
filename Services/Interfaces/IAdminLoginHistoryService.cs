using VietRove.Models;

namespace VietRove.Services.Interfaces
{
    public interface IAdminLoginHistoryService
    {
        Task<AdminLoginHistoryResultViewModel> GetLoginHistoryAsync(
            AdminLoginHistorySearchViewModel model);
    }
}