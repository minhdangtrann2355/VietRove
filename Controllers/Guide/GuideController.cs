
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO;
using VietRove.Models.Guide;
using VietRove.Security;
using VietRove.Services.Interfaces.Guide.Profile;
using VietRove.Services.Interfaces.Guide.Contract;
using VietRove.Services.Interfaces.Authentication.Login;
using VietRove.Services.Interfaces.FileStorage;

using VietRove.Data;
using VietRove.Entities;

namespace VietRove.Controllers.Guide
{
    [Authorize(Policy = AppRoles.Guide)]
    public class GuideController : Controller
    {
        private readonly IGuideProfileService _profileService;
        private readonly ILoginService _loginService;
        private readonly VietRoveDbContext _context;
        private readonly IContractService _contractService;
        private readonly IFileStorageService _fileStorageService;

        public GuideController(
            IGuideProfileService profileService,
            ILoginService loginService,
            VietRoveDbContext context,
            IContractService contractService,
            IFileStorageService fileStorageService)
        {
            _profileService = profileService;
            _loginService = loginService;
            _context = context;
            _contractService = contractService;
            _fileStorageService = fileStorageService;
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
        public async Task<IActionResult> Settings()
        {
            if (TryGetCurrentUserId(out int userId) == false)
            {
                return Unauthorized();
            }

            GuideAccountSettingsViewModel? model =
                await _loginService.GetGuideAccountSettingsAsync(userId);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            GuideAccountSettingsViewModel model)
        {
            if (TryGetCurrentUserId(out int userId) == false)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid == false)
            {
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin mật khẩu.";
                return RedirectToAction(nameof(Settings));
            }

            string resultError = await _loginService.ChangeGuidePasswordAsync(
                userId,
                model);

            if (string.IsNullOrEmpty(resultError) == false)
            {
                TempData["ErrorMessage"] = resultError;
                return RedirectToAction(nameof(Settings));
            }

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";

            return RedirectToAction("Index", "Login");
        }

        // contract

        [HttpGet]
        public async Task<IActionResult> Contract()
        {
            if (TryGetCurrentUserId(out int userId) == false)
            {
                return Unauthorized();
            }

            GuideProfile guide = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (guide == null)
            {
                return NotFound();
            }

            await LoadContractsAsync(guide.GuideId);

            return View(new ContractViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateContract(ContractViewModel model)
        {
            if (TryGetCurrentUserId(out int userId) == false)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid == false)
            {
                await LoadContractsByUserIdAsync(userId);
                return View("Contract", model);
            }

            try
            {
                await _contractService.CreateContractAsync(userId, model);

                TempData["ContractMessage"] = "Tạo hợp đồng thành công.";
                return RedirectToAction(nameof(Contract));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                await LoadContractsByUserIdAsync(userId);

                return View("Contract", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadContractPdf(int id)
        {
            if (TryGetCurrentUserId(out int userId) == false)
            {
                return Unauthorized();
            }

            GuideProfile guide = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (guide == null)
            {
                return NotFound();
            }

            Contract contract = await _context.Contracts
                .FirstOrDefaultAsync(c =>
                    c.ContractId == id &&
                    c.GuideId == guide.GuideId);

            if (contract == null || string.IsNullOrWhiteSpace(contract.PdfFile))
            {
                return NotFound();
            }

            Stream? pdfStream = await _fileStorageService.OpenReadAsync(
                contract.PdfFile);

            if (pdfStream == null)
            {
                return NotFound();
            }

            return File(
                pdfStream,
                "application/pdf",
                "HopDong-" + contract.ContractId + ".pdf");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadSignedContract(
            int id,
            IFormFile signedPdf)
        {
            if (TryGetCurrentUserId(out int userId) == false)
            {
                return Unauthorized();
            }

            GuideProfile guide = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (guide == null)
            {
                return NotFound();
            }

            Contract contract = await _context.Contracts
                .FirstOrDefaultAsync(c =>
                    c.ContractId == id &&
                    c.GuideId == guide.GuideId);

            if (contract == null)
            {
                return NotFound();
            }

            if (contract.Status != 0)
            {
                TempData["ContractError"] =
                    "Hợp đồng không ở trạng thái cho phép tải lên bản đã ký.";

                return RedirectToAction(nameof(Contract));
            }

            string validationMessage =
                await _fileStorageService.ValidateEvidenceAsync(signedPdf);

            if (string.IsNullOrEmpty(validationMessage) == false)
            {
                TempData["ContractError"] = validationMessage;
                return RedirectToAction(nameof(Contract));
            }

            string extension = Path.GetExtension(signedPdf.FileName)
                .ToLowerInvariant();

            if (extension != ".pdf")
            {
                TempData["ContractError"] = "Chỉ chấp nhận file PDF.";
                return RedirectToAction(nameof(Contract));
            }

            string newFileName = await _fileStorageService.SaveFileAsync(signedPdf);
            string oldFileName = contract.SignedFile;

            try
            {
                contract.SignedFile = newFileName;

                await _context.SaveChangesAsync();

                if (string.IsNullOrWhiteSpace(oldFileName) == false)
                {
                    _fileStorageService.DeleteFile(oldFileName);
                }

                TempData["ContractMessage"] =
                    "Tải lên hợp đồng đã ký thành công. Hợp đồng đang chờ Admin xét duyệt.";
            }
            catch
            {
                _fileStorageService.DeleteFile(newFileName);
                throw;
            }

            return RedirectToAction(nameof(Contract));
        }

        private async Task LoadContractsAsync(int guideId)
        {
            List<Contract> contracts = await _context.Contracts
                .Where(c => c.GuideId == guideId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Contracts = contracts;
        }

        private async Task LoadContractsByUserIdAsync(int userId)
        {
            GuideProfile guide = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (guide == null)
            {
                ViewBag.Contracts = new List<Contract>();
                return;
            }

            await LoadContractsAsync(guide.GuideId);
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
