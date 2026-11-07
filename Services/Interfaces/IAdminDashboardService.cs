using System.Threading.Tasks;
using VietRove.Models;

namespace VietRove.Services.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardViewModel> GetDashboardDataAsync();
    }
}