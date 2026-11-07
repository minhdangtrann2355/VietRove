using VietRove.Models;

namespace VietRove.Services
{
    public interface IGuideProfileService
    {
        Task<GuideProfileViewModel?> GetProfileViewModelAsync(int userId);
        Task<string> UpdateContactInfoAsync(int userId, GuideContactEditModel model);
        Task<string> AddLanguageAsync(int userId, GuideLanguageAddModel model);
        Task<bool> DeleteLanguageAsync(int userId, int guideLanguageId);
        Task<string> AddExperienceAsync(int userId, GuideExperienceAddModel model);
        Task<bool> DeleteExperienceAsync(int userId, int experienceId);
    }
}
