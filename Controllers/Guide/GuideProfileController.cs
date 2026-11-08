using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VietRove.Models.Guide;
using VietRove.Security;
using VietRove.Services.Interfaces.Guide.Profile;
using VietRove.Services.Interfaces.FileStorage;

namespace VietRove.Controllers.Guide
{
    [Authorize(Policy = AppRoles.Guide)]
    public class GuideProfileController : Controller
    {
        private readonly IGuideProfileService _profileService;
        private readonly IFileStorageService _fileStorageService;

        public GuideProfileController(
            IGuideProfileService profileService,
            IFileStorageService fileStorageService)
        {
            _profileService = profileService;
            _fileStorageService = fileStorageService;
        }

        public async Task<IActionResult> Profile()
        {
            if (!TryGetCurrentUserId(out int userId))
            {
                return Unauthorized();
            }

            GuideProfileViewModel? model =
                await _profileService.GetProfileViewModelAsync(userId);

            if (model == null)
            {
                return NotFound();
            }

            return View("~/Views/Guide/Profile.cshtml", model);
        }

        public async Task<IActionResult> EditProfile()
        {
            if (!TryGetCurrentUserId(out int userId))
            {
                return Unauthorized();
            }

            GuideProfileViewModel? model =
                await _profileService.GetProfileViewModelAsync(userId);

            if (model == null)
            {
                return NotFound();
            }

            return View("~/Views/Guide/EditProfile.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvatar(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound();
            }

            if (!TryGetCurrentUserId(out int userId))
            {
                return Unauthorized();
            }

            string? avatarFileName =
                await _profileService.GetAvatarFileNameAsync(userId);

            if (string.IsNullOrWhiteSpace(avatarFileName))
            {
                return NotFound();
            }

            if (!string.Equals(
                avatarFileName,
                fileName,
                StringComparison.Ordinal))
            {
                return NotFound();
            }

            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            string contentType = string.Empty;

            if (extension == ".jpg" || extension == ".jpeg")
            {
                contentType = "image/jpeg";
            }
            else if (extension == ".png")
            {
                contentType = "image/png";
            }
            else
            {
                return NotFound();
            }

            Stream? stream = await _fileStorageService.OpenReadAsync(fileName);

            if (stream == null)
            {
                return NotFound();
            }

            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Cache-Control"] = "no-store";

            return File(stream, contentType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContact(GuideContactEditModel model)
        {
            if (!TryGetCurrentUserId(out int userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                GuideProfileViewModel? profile =
                    await _profileService.GetProfileViewModelAsync(userId);

                if (profile == null)
                {
                    return NotFound();
                }

                return View("~/Views/Guide/EditProfile.cshtml", profile);
            }

            string result =
                await _profileService.UpdateContactInfoAsync(userId, model);

            if (string.IsNullOrWhiteSpace(result))
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin liên hệ thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = result;
            }

            return RedirectToAction(nameof(EditProfile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLanguage(GuideLanguageAddModel model)
        {
            if (!TryGetCurrentUserId(out int userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin ngoại ngữ không hợp lệ.";
                return RedirectToAction(nameof(EditProfile));
            }

            string result = await _profileService.AddLanguageAsync(userId, model);

            if (string.IsNullOrWhiteSpace(result))
            {
                TempData["SuccessMessage"] = "Thêm ngoại ngữ thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = result;
            }

            return RedirectToAction(nameof(EditProfile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLanguage(int id)
        {
            if (!TryGetCurrentUserId(out int userId))
            {
                return Unauthorized();
            }

            bool result = await _profileService.DeleteLanguageAsync(userId, id);

            if (result)
            {
                TempData["SuccessMessage"] = "Xóa ngoại ngữ thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa ngoại ngữ này.";
            }

            return RedirectToAction(nameof(EditProfile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExperience(GuideExperienceAddModel model)
        {
            if (!TryGetCurrentUserId(out int userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin kinh nghiệm không hợp lệ.";
                return RedirectToAction(nameof(EditProfile));
            }

            string result =
                await _profileService.AddExperienceAsync(userId, model);

            if (string.IsNullOrWhiteSpace(result))
            {
                TempData["SuccessMessage"] = "Thêm kinh nghiệm thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = result;
            }

            return RedirectToAction(nameof(EditProfile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExperience(int id)
        {
            if (!TryGetCurrentUserId(out int userId))
            {
                return Unauthorized();
            }

            bool result =
                await _profileService.DeleteExperienceAsync(userId, id);

            if (result)
            {
                TempData["SuccessMessage"] = "Xóa kinh nghiệm thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa kinh nghiệm này.";
            }

            return RedirectToAction(nameof(EditProfile));
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            userId = 0;

            Claim? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return false;
            }

            return int.TryParse(userIdClaim.Value, out userId);
        }
    }
}
