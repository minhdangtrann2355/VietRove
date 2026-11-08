
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Security;
using VietRove.Services.Interfaces.FileStorage;
using VietRove.Services.Interfaces.Guest;

namespace VietRove.Services.Guest
{
    public class GuestViolationReportService : IGuestViolationReportService
    {
        private readonly VietRoveDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        private const int MaxEvidenceFileCount = 5;
        private const long MaxFileSize = 10 * 1024 * 1024;
        private const long MaxTotalEvidenceSize = 25 * 1024 * 1024;

        public GuestViolationReportService(
            VietRoveDbContext context,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        public async Task<string> CreateReportAsync(
            int guideId,
            string reporterName,
            string? reporterPhone,
            string description,
            List<IFormFile> evidenceFiles)
        {
            GuideProfile? guide = await _context.GuideProfiles
                .FirstOrDefaultAsync(g =>
                    g.GuideId == guideId &&
                    g.Status == (int)ProfileStatus.Approved);

            if (guide == null)
            {
                return "Không tìm thấy hướng dẫn viên hoặc hồ sơ không còn được duyệt.";
            }

            if (string.IsNullOrWhiteSpace(reporterName))
            {
                return "Vui lòng nhập họ và tên người báo cáo.";
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                return "Vui lòng nhập nội dung báo cáo.";
            }

            if (reporterName.Trim().Length > 150)
            {
                return "Họ tên không được vượt quá 150 ký tự.";
            }

            if (string.IsNullOrWhiteSpace(reporterPhone) == false &&
                reporterPhone.Trim().Length > 20)
            {
                return "Số điện thoại không được vượt quá 20 ký tự.";
            }

            List<IFormFile> validFiles = new List<IFormFile>();

            if (evidenceFiles != null)
            {
                foreach (IFormFile file in evidenceFiles)
                {
                    if (file == null || file.Length == 0)
                    {
                        continue;
                    }

                    validFiles.Add(file);
                }
            }

            if (validFiles.Count > MaxEvidenceFileCount)
            {
                return "Mỗi báo cáo chỉ được đính kèm tối đa 5 tệp minh chứng.";
            }

            long totalEvidenceSize = 0;

            foreach (IFormFile file in validFiles)
            {
                if (file.Length > MaxFileSize)
                {
                    return "Mỗi tệp minh chứng không được vượt quá 10 MB.";
                }

                if (totalEvidenceSize > MaxTotalEvidenceSize - file.Length)
                {
                    return "Tổng dung lượng các tệp minh chứng không được vượt quá 25 MB.";
                }

                totalEvidenceSize += file.Length;

                string fileError =
                    await _fileStorageService.ValidateEvidenceAsync(file);

                if (string.IsNullOrEmpty(fileError) == false)
                {
                    return fileError;
                }
            }

            List<string> savedFiles = new List<string>();

            try
            {
                ViolationReport report = new ViolationReport
                {
                    GuideId = guideId,
                    ReporterName = reporterName.Trim(),
                    ReporterPhone = string.IsNullOrWhiteSpace(reporterPhone)
                        ? null
                        : reporterPhone.Trim(),
                    Description = description.Trim(),
                    Status = 0,
                    CreatedAt = DateTime.Now,
                    Evidence = new List<ViolationReportEvidence>(),
                    Actions = new List<ViolationReportAction>()
                };

                foreach (IFormFile file in validFiles)
                {
                    string savedFileName =
                        await _fileStorageService.SaveFileAsync(file);

                    savedFiles.Add(savedFileName);

                    string originalFileName = Path.GetFileName(file.FileName);

                    if (originalFileName.Length > 255)
                    {
                        originalFileName =
                            originalFileName.Substring(0, 255);
                    }

                    ViolationReportEvidence evidence =
                        new ViolationReportEvidence
                        {
                            FileName = savedFileName,
                            OriginalFileName = originalFileName,
                            UploadedAt = DateTime.Now
                        };

                    report.Evidence.Add(evidence);
                }

                _context.ViolationReports.Add(report);
                await _context.SaveChangesAsync();

                return string.Empty;
            }
            catch
            {
                foreach (string fileName in savedFiles)
                {
                    _fileStorageService.DeleteFile(fileName);
                }

                return "Không thể gửi báo cáo. Vui lòng thử lại.";
            }
        }
    }
}
