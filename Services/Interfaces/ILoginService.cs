
using VietRove.Models;

namespace VietRove.Services.Interfaces
{
    public interface ILoginService
    {
        Task<AuthenticationResult> AuthenticateAsync(LoginViewModel model);

        Task<GuideAccountSettingsViewModel?> GetGuideAccountSettingsAsync(int userId);

        Task<string> ChangeGuidePasswordAsync(
            int userId,
            GuideAccountSettingsViewModel model);
    }
}
