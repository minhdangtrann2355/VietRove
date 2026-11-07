
using VietRove.Models.Authentication;
using VietRove.Models.Guide;

namespace VietRove.Services.Interfaces.Authentication.Login
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
