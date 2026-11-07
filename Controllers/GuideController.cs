using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VietRove.Models;
using VietRove.Security;
using VietRove.Services;

namespace VietRove.Controllers
{
    [Authorize(Policy = AppRoles.Guide)]
    public class GuideController : Controller
    {
        private readonly IGuideProfileService _profileService;

        public GuideController(IGuideProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (TryGetCurrentUserId(out int userId) == false)
            {
                return Unauthorized();
            }

            GuideProfileViewModel? model = await _profileService.GetProfileViewModelAsync(userId);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            if (TryGetCurrentUserId(out int userId) == false)
            {
                return Unauthorized();
            }

            GuideProfileViewModel? model = await _profileService.GetProfileViewModelAsync(userId);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContact(GuideContactEditModel model)
        {
            if (TryGetCurrentUserId(out int userId) == false)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid == false)
            {
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin liên hệ vừa nhập.";
                return RedirectToAction(nameof(EditProfile));
            }

            string resultError = await _profileService.UpdateContactInfoAsync(userId, model);
            if (string.IsNullOrEmpty(resultError) == false)
            {
                TempData["ErrorMessage"] = resultError;
                return RedirectToAction(nameof(EditProfile));
            }

            TempData["SuccessMessage"] = "Cập nhật thông tin liên hệ thành công.";
            return RedirectToAction(nameof(EditProfile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLanguage(GuideLanguageAddModel model)
        {
            if (TryGetCurrentUserId(out int userId) == false)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid == false)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ngoại ngữ và tải tệp minh chứng hợp lệ.";
                return RedirectToAction(nameof(EditProfile));
            }

            string resultError = await _profileService.AddLanguageAsync(userId, model);
            if (string.IsNullOrEmpty(resultError) == false)
            {
                TempData["ErrorMessage"] = resultError;
                return RedirectToAction(nameof(EditProfile));
            }

            TempData["SuccessMessage"] = "Thêm ngoại ngữ thành công. Đang chờ Admin xác minh.";
            return RedirectToAction(nameof(EditProfile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLanguage(int id)
        {
            if (TryGetCurrentUserId(out int userId) == false)
            {
                return Unauthorized();
            }

            bool isSuccess = await _profileService.DeleteLanguageAsync(userId, id);
            if (isSuccess == false)
            {
                TempData["ErrorMessage"] = "Không thể xóa ngoại ngữ đã chọn.";
                return RedirectToAction(nameof(EditProfile));
            }

            TempData["SuccessMessage"] = "Đã xóa ngoại ngữ khỏi hồ sơ.";
            return RedirectToAction(nameof(EditProfile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExperience(GuideExperienceAddModel model)
        {
            if (TryGetCurrentUserId(out int userId) == false)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid == false)
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên kinh nghiệm và tải tệp minh chứng hợp lệ.";
                return RedirectToAction(nameof(EditProfile));
            }

            string resultError = await _profileService.AddExperienceAsync(userId, model);
            if (string.IsNullOrEmpty(resultError) == false)
            {
                TempData["ErrorMessage"] = resultError;
                return RedirectToAction(nameof(EditProfile));
            }

            TempData["SuccessMessage"] = "Thêm kinh nghiệm thành công. Đang chờ Admin xác minh.";
            return RedirectToAction(nameof(EditProfile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExperience(int id)
        {
            if (TryGetCurrentUserId(out int userId) == false)
            {
                return Unauthorized();
            }

            bool isSuccess = await _profileService.DeleteExperienceAsync(userId, id);
            if (isSuccess == false)
            {
                TempData["ErrorMessage"] = "Không thể xóa kinh nghiệm đã chọn.";
                return RedirectToAction(nameof(EditProfile));
            }

            TempData["SuccessMessage"] = "Đã xóa kinh nghiệm khỏi hồ sơ.";
            return RedirectToAction(nameof(EditProfile));
        }

        [HttpGet]
        public IActionResult Contract()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            userId = 0;

            Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                return false;
            }

            if (int.TryParse(claim.Value, out userId) == false || userId <= 0)
            {
                userId = 0;
                return false;
            }

            return true;
        }
    }
}