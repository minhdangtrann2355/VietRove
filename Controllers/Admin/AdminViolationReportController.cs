using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using VietRove.Models.Admin;
using VietRove.Security;
using VietRove.Services.Interfaces.Admin;
using VietRove.Services.Interfaces.FileStorage;

namespace VietRove.Controllers.Admin;

[Authorize(Policy = AppRoles.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AdminViolationReportController : Controller
{
    private readonly IAdminViolationReportService _reportService;
    private readonly IFileStorageService _fileStorageService;

    public AdminViolationReportController(
        IAdminViolationReportService reportService,
        IFileStorageService fileStorageService)
    {
        _reportService = reportService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View("~/Views/Admin/ViolationReports.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] AdminViolationReportSearchViewModel searchModel)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(new
            {
                success = false,
                message = "Thông tin tìm kiếm không hợp lệ."
            });
        }

        if (searchModel.Status.HasValue &&
            (searchModel.Status.Value < 0 || searchModel.Status.Value > 3))
        {
            return BadRequest(new
            {
                success = false,
                message = "Trạng thái không hợp lệ."
            });
        }

        if (searchModel.FromDate.HasValue &&
            searchModel.ToDate.HasValue &&
            searchModel.FromDate.Value.Date > searchModel.ToDate.Value.Date)
        {
            return BadRequest(new
            {
                success = false,
                message = "Ngày bắt đầu không được lớn hơn ngày kết thúc."
            });
        }

        if (searchModel.Page < 1)
        {
            searchModel.Page = 1;
        }

        AdminViolationReportResultViewModel result =
            await _reportService.GetReportsAsync(searchModel);

        return Json(new
        {
            success = true,
            data = result
        });
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        AdminViolationReportDetailViewModel? model =
            await _reportService.GetReportDetailAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View("~/Views/Admin/ViolationReportDetail.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] AdminViolationReportUpdateStatusViewModel model)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Mã báo cáo không hợp lệ."
            });
        }

        if (model == null || ModelState.IsValid == false)
        {
            return BadRequest(new
            {
                success = false,
                message = "Dữ liệu cập nhật không hợp lệ."
            });
        }

        Claim adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (adminIdClaim == null ||
            int.TryParse(adminIdClaim.Value, out int adminId) == false)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Không xác định được tài khoản Admin."
            });
        }

        string? errorMessage = await _reportService.UpdateStatusAsync(
            id,
            adminId,
            model.Status,
            model.Note);

        if (errorMessage != null)
        {
            return BadRequest(new
            {
                success = false,
                message = errorMessage
            });
        }

        return Json(new
        {
            success = true,
            message = "Cập nhật trạng thái báo cáo thành công."
        });
    }

    [HttpGet]
    public async Task<IActionResult> PreviewEvidence(int reportId, int evidenceId)
    {
        return await GetEvidenceFileAsync(reportId, evidenceId, false);
    }

    [HttpGet]
    public async Task<IActionResult> DownloadEvidence(int reportId, int evidenceId)
    {
        return await GetEvidenceFileAsync(reportId, evidenceId, true);
    }

    private async Task<IActionResult> GetEvidenceFileAsync(
        int reportId,
        int evidenceId,
        bool download)
    {
        if (reportId <= 0 || evidenceId <= 0)
        {
            return BadRequest();
        }

        AdminViolationReportEvidenceFileViewModel? evidence =
            await _reportService.GetEvidenceAsync(reportId, evidenceId);

        if (evidence == null)
        {
            return NotFound();
        }

        string extension = Path.GetExtension(evidence.FileName).ToLowerInvariant();

        string contentType = extension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(contentType))
        {
            return BadRequest();
        }

        Stream? fileStream = await _fileStorageService.OpenReadAsync(
            evidence.FileName);

        if (fileStream == null)
        {
            return NotFound();
        }

        Response.Headers[HeaderNames.XContentTypeOptions] = "nosniff";
        Response.Headers[HeaderNames.CacheControl] = "no-store";

        if (download)
        {
            string downloadName = Path.GetFileName(
                evidence.OriginalFileName);

            if (string.IsNullOrWhiteSpace(downloadName))
            {
                downloadName = "minh-chung" + extension;
            }

            return File(fileStream, contentType, downloadName);
        }

        Response.Headers[HeaderNames.ContentDisposition] = "inline";

        return File(fileStream, contentType);
    }
}