using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VietRove.Models.Admin;
using VietRove.Services.Interfaces.Admin;
using VietRove.Services.Interfaces.FileStorage;
using VietRove.Services.FileStorage;
using VietRove.Security;

namespace VietRove.Controllers.Admin
{
    [Authorize(Policy = AppRoles.Admin)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AdminCompetencyReviewController : Controller
    {
        private readonly IAdminCompetencyReviewService _competencyReviewService;
        private readonly IFileStorageService _fileStorageService;

        public AdminCompetencyReviewController(
            IAdminCompetencyReviewService competencyReviewService,
            IFileStorageService fileStorageService)
        {
            _competencyReviewService = competencyReviewService;
            _fileStorageService = fileStorageService;
        }

        public async Task<IActionResult> Index()
        {
            AdminCompetencyReviewViewModel model =
                await _competencyReviewService.GetPendingCompetenciesAsync();

            ViewData["Title"] = "Xét duyệt năng lực";

            return View("~/Views/Admin/CompetencyReview.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> LanguageCertificate(int guideLanguageId)
        {
            if (guideLanguageId <= 0)
            {
                return NotFound();
            }

            string fileName =
                await _competencyReviewService.GetLanguageCertificateFileNameAsync(guideLanguageId);

            return await OpenEvidenceFileAsync(fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExperienceEvidence(int experienceId)
        {
            if (experienceId <= 0)
            {
                return NotFound();
            }

            string fileName =
                await _competencyReviewService.GetExperienceEvidenceFileNameAsync(experienceId);

            return await OpenEvidenceFileAsync(fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLanguage(int guideLanguageId)
        {
            int adminId = GetCurrentAdminId();

            if (adminId <= 0 || guideLanguageId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Yêu cầu không hợp lệ."
                });
            }

            bool result = await _competencyReviewService.ApproveLanguageAsync(
                guideLanguageId,
                adminId);

            if (result == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể phê duyệt ngôn ngữ này. Vui lòng kiểm tra trạng thái và chứng chỉ."
                });
            }

            return Json(new
            {
                success = true,
                message = "Đã phê duyệt ngôn ngữ."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectLanguage(int guideLanguageId, string reason)
        {
            int adminId = GetCurrentAdminId();

            if (adminId <= 0 || guideLanguageId <= 0 ||
                string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 1000)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Thông tin từ chối không hợp lệ."
                });
            }

            bool result = await _competencyReviewService.RejectLanguageAsync(
                guideLanguageId,
                adminId,
                reason);

            if (result == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể từ chối ngôn ngữ này."
                });
            }

            return Json(new
            {
                success = true,
                message = "Đã từ chối ngôn ngữ."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveExperience(int experienceId)
        {
            int adminId = GetCurrentAdminId();

            if (adminId <= 0 || experienceId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Yêu cầu không hợp lệ."
                });
            }

            bool result = await _competencyReviewService.ApproveExperienceAsync(
                experienceId,
                adminId);

            if (result == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể phê duyệt kinh nghiệm này. Vui lòng kiểm tra trạng thái và minh chứng."
                });
            }

            return Json(new
            {
                success = true,
                message = "Đã phê duyệt kinh nghiệm."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectExperience(int experienceId, string reason)
        {
            int adminId = GetCurrentAdminId();

            if (adminId <= 0 || experienceId <= 0 ||
                string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 1000)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Thông tin từ chối không hợp lệ."
                });
            }

            bool result = await _competencyReviewService.RejectExperienceAsync(
                experienceId,
                adminId,
                reason);

            if (result == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể từ chối kinh nghiệm này."
                });
            }

            return Json(new
            {
                success = true,
                message = "Đã từ chối kinh nghiệm."
            });
        }

        private int GetCurrentAdminId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userId, out int adminId) == false)
            {
                return 0;
            }

            return adminId;
        }

        private async Task<IActionResult> OpenEvidenceFileAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound();
            }

            Stream fileStream = await _fileStorageService.OpenReadAsync(fileName);

            if (fileStream == null)
            {
                return NotFound();
            }

            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            string contentType = "application/octet-stream";

            if (extension == ".pdf")
            {
                contentType = "application/pdf";
            }
            else if (extension == ".jpg" || extension == ".jpeg")
            {
                contentType = "image/jpeg";
            }
            else if (extension == ".png")
            {
                contentType = "image/png";
            }

            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Cache-Control"] = "no-store";

            return File(fileStream, contentType);
        }
    }
}