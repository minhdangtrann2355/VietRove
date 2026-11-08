
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Models.Guide;
using VietRove.Security;
using VietRove.Services.Interfaces.FileStorage;
using VietRove.Services.Interfaces.Guide.Contract;

namespace VietRove.Controllers.Guide
{
    [Authorize(Policy = AppRoles.Guide)]
    public class GuideContractController : Controller
    {
        private readonly VietRoveDbContext _context;
        private readonly IContractService _contractService;
        private readonly IFileStorageService _fileStorageService;

        public GuideContractController(
            VietRoveDbContext context,
            IContractService contractService,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _contractService = contractService;
            _fileStorageService = fileStorageService;
        }

        public async Task<IActionResult> Contract()
        {
            if (!TryGetCurrentUserId(out int userId))
            {
                return Unauthorized();
            }

            GuideProfile? guide = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (guide == null)
            {
                return NotFound();
            }

            await LoadContractsAsync(guide.GuideId);

            return View("~/Views/Guide/Contract.cshtml", new ContractViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateContract(ContractViewModel model)
        {
            if (!TryGetCurrentUserId(out int userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                await LoadContractsByUserIdAsync(userId);

                return View("~/Views/Guide/Contract.cshtml", model);
            }

            try
            {
                await _contractService.CreateContractAsync(userId, model);

                TempData["ContractMessage"] = "Tạo hợp đồng thành công.";

                return RedirectToAction(nameof(Contract));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ContractError"] = ex.Message;

                await LoadContractsByUserIdAsync(userId);

                return View("~/Views/Guide/Contract.cshtml", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadContractPdf(int id)
        {
            if (!TryGetCurrentUserId(out int userId))
            {
                return Unauthorized();
            }

            GuideProfile? guide = await _context.GuideProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (guide == null)
            {
                return NotFound();
            }

            Contract? contract = await _context.Contracts
                .FirstOrDefaultAsync(c =>
                    c.ContractId == id &&
                    c.GuideId == guide.GuideId);

            if (contract == null || string.IsNullOrWhiteSpace(contract.PdfFile))
            {
                return NotFound();
            }

            Stream? stream =
                await _fileStorageService.OpenReadAsync(contract.PdfFile);

            if (stream == null)
            {
                return NotFound();
            }

            return File(
                stream,
                "application/pdf",
                "HopDong_" + contract.ContractId + ".pdf");
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
            GuideProfile? guide = await _context.GuideProfiles
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

            Claim? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return false;
            }

            return int.TryParse(userIdClaim.Value, out userId);
        }
    }
}
