using System.Threading.Tasks;
using VietRove.Models.Authentication;

namespace VietRove.Services.Interfaces.Authentication.Register
{
    public interface IRegisterService
    {
        Task<(bool Success, string Message, string? Field)> RegisterAsync(RegisterViewModel model);
    }
}