using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Models.Guest;
using VietRove.Security;
using VietRove.Services.Interfaces.Guest;

namespace VietRove.Controllers.Guest
{
    public class GuestViolationReportController : Controller
    {
        private readonly VietRoveDbContext _context;
        private readonly IGuestViolationReportService _reportService;

        public GuestViolationReportController(
            VietRoveDbContext context,
            IGuestViolationReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int guideId)
        {
            var guide = await _context.GuideProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(g =>
                    g.GuideId == guideId &&
                    g.Status == (int)ProfileStatus.Approved);

            if (guide == null)
            {
                return NotFound();
            }

            ViewBag.GuideName = guide.FullName;

            ViolationReportCreateViewModel model =
                new ViolationReportCreateViewModel
                {
                    GuideId = guide.GuideId
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ViolationReportCreateViewModel model)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Vui lòng kiểm tra lại thông tin báo cáo."
                });
            }

            string result = await _reportService.CreateReportAsync(
                model.GuideId,
                model.ReporterName,
                model.ReporterPhone,
                model.Description,
                model.EvidenceFiles);

            if (string.IsNullOrEmpty(result) == false)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result
                });
            }

            return Json(new
            {
                success = true,
                message = "Gửi báo cáo thành công.",
                redirectUrl = Url.Action(
                    "GuideProfile",
                    "GuestGuide",
                    new { id = model.GuideId })
            });
        }
    }
}