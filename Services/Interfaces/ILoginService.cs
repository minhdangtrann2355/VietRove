using VietRove.Models;

namespace VietRove.Services.Interfaces
{
    public interface ILoginService
    {
        Task<AuthenticationResult> AuthenticateAsync(
            LoginViewModel model);
    }
}