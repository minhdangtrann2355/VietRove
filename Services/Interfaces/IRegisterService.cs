using System.Threading.Tasks;
using VietRove.Models;

namespace VietRove.Services.Interfaces
{
    public interface IRegisterService
    {
        Task<(bool Success, string Message, string? Field)> RegisterAsync(RegisterViewModel model);
    }
}