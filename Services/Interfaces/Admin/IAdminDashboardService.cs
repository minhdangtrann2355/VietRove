using System.Threading.Tasks;
using VietRove.Models.Admin;

namespace VietRove.Services.Interfaces.Admin
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardViewModel> GetDashboardDataAsync();
    }
}