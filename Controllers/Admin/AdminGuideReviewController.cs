
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VietRove.Security;
using VietRove.Services.Interfaces.Admin;
using VietRove.Services.Interfaces.FileStorage;

namespace VietRove.Controllers.Admin
{
    [Authorize(Policy = AppRoles.Admin)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AdminGuideReviewController : Controller
    {
        private readonly IAdminGuideReviewService _guideReviewService;
        private readonly IFileStorageService _fileStorageService;

        public AdminGuideReviewController(
            IAdminGuideReviewService guideReviewService,
            IFileStorageService fileStorageService)
        {
            _guideReviewService = guideReviewService;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await _guideReviewService.GetPendingGuidesAsync();

            return View("~/Views/Admin/GuideReview.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int guideId)
        {
            if (guideId <= 0)
            {
                return BadRequest();
            }

            var model = await _guideReviewService.GetGuideDetailAsync(guideId);

            if (model == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/GuideReviewDetail.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int guideId)
        {
            if (guideId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Ma ho so khong hop le."
                });
            }

            string? adminIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (int.TryParse(adminIdValue, out int adminId) == false)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Khong xac dinh duoc tai khoan Admin."
                });
            }

            bool result = await _guideReviewService.ApproveGuideAsync(
                guideId,
                adminId
            );

            if (result == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Ho so khong ton tai, khong con cho duyet hoac chua du giay to."
                });
            }

            return Json(new
            {
                success = true,
                message = "Da duyet ho so Guide."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int guideId, string reason)
        {
            if (guideId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Ma ho so khong hop le."
                });
            }

            if (string.IsNullOrWhiteSpace(reason) || reason.Length > 1000)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Ly do tu choi khong hop le."
                });
            }

            bool result = await _guideReviewService.RejectGuideAsync(
                guideId,
                reason.Trim()
            );

            if (result == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Ly do tu choi khong hop le hoac ho so khong con cho duyet."
                });
            }

            return Json(new
            {
                success = true,
                message = "Da tu choi ho so Guide."
            });
        }

        [HttpGet]
        public async Task<IActionResult> GuideDocument(
            int guideId,
            int documentId)
        {
            if (guideId <= 0 || documentId <= 0)
            {
                return BadRequest();
            }

            string? fileName =
                await _guideReviewService.GetDocumentFileNameAsync(
                    guideId,
                    documentId
                );

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound();
            }

            Stream? stream =
                await _fileStorageService.OpenReadAsync(fileName);

            if (stream == null)
            {
                return NotFound();
            }

            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Cache-Control"] = "no-store";

            return File(stream, GetFileContentType(fileName));
        }

        [HttpGet]
        public async Task<IActionResult> GuideLanguageCertificate(
            int guideId,
            int guideLanguageId)
        {
            if (guideId <= 0 || guideLanguageId <= 0)
            {
                return BadRequest();
            }

            string? fileName =
                await _guideReviewService.GetLanguageCertificateFileNameAsync(
                    guideId,
                    guideLanguageId
                );

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound();
            }

            Stream? stream =
                await _fileStorageService.OpenReadAsync(fileName);

            if (stream == null)
            {
                return NotFound();
            }

            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Cache-Control"] = "no-store";

            return File(stream, GetFileContentType(fileName));
        }

        private string GetFileContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (extension == ".pdf")
            {
                return "application/pdf";
            }

            if (extension == ".png")
            {
                return "image/png";
            }

            if (extension == ".jpg" || extension == ".jpeg")
            {
                return "image/jpeg";
            }

            return "application/octet-stream";
        }
    }
}
