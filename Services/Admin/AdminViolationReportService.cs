using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Models.Admin;
using VietRove.Services.Interfaces.Admin;
using VietRove.Security;

namespace VietRove.Services.Admin;

public class AdminViolationReportService : IAdminViolationReportService
{
    private readonly VietRoveDbContext _context;

    public AdminViolationReportService(VietRoveDbContext context)
    {
        _context = context;
    }

    public async Task<AdminViolationReportResultViewModel> GetReportsAsync(
        AdminViolationReportSearchViewModel searchModel)
    {
        IQueryable<ViolationReport> query = _context.ViolationReports
            .AsNoTracking()
            .Include(r => r.GuideProfile);

        if (string.IsNullOrWhiteSpace(searchModel.Search) == false)
        {
            string keyword = searchModel.Search.Trim();

            query = query.Where(r =>
                r.ReporterName.Contains(keyword) ||
                r.Description.Contains(keyword) ||
                r.GuideProfile.FullName.Contains(keyword));
        }

        if (searchModel.Status.HasValue)
        {
            query = query.Where(r => r.Status == searchModel.Status.Value);
        }

        if (searchModel.FromDate.HasValue)
        {
            DateTime fromDate = searchModel.FromDate.Value.Date;

            query = query.Where(r => r.CreatedAt >= fromDate);
        }

        if (searchModel.ToDate.HasValue)
        {
            DateTime toDate = searchModel.ToDate.Value.Date.AddDays(1);

            query = query.Where(r => r.CreatedAt < toDate);
        }

        int totalRecords = await query.CountAsync();

        const int pageSize = 10;

        int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        int currentPage = searchModel.Page;

        if (totalPages > 0 && currentPage > totalPages)
        {
            currentPage = totalPages;
        }

        List<AdminViolationReportListItemViewModel> items = await query
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.ReportId)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new AdminViolationReportListItemViewModel
            {
                ReportId = r.ReportId,
                ReporterName = r.ReporterName,
                ReporterPhone = r.ReporterPhone,
                GuideName = r.GuideProfile.FullName,
                CreatedAt = r.CreatedAt,
                Status = r.Status,
                EvidenceCount = r.Evidence.Count
            })
            .ToListAsync();

        return new AdminViolationReportResultViewModel
        {
            Items = items,
            TotalRecords = totalRecords,
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<AdminViolationReportDetailViewModel?> GetReportDetailAsync(
        int reportId)
    {
        ViolationReport report = await _context.ViolationReports
            .AsNoTracking()
            .Include(r => r.GuideProfile)
                .ThenInclude(g => g.User)
            .Include(r => r.Evidence)
            .Include(r => r.Actions)
                .ThenInclude(a => a.Admin)
            .FirstOrDefaultAsync(r => r.ReportId == reportId);

        if (report == null)
        {
            return null;
        }

        AdminViolationReportDetailViewModel result =
            new AdminViolationReportDetailViewModel
            {
                ReportId = report.ReportId,
                ReporterName = report.ReporterName,
                ReporterPhone = report.ReporterPhone,
                Description = report.Description,
                GuideName = report.GuideProfile.FullName,
                GuideEmail = report.GuideProfile.User.Email,
                GuidePhone = report.GuideProfile.ContactPhone,
                CreatedAt = report.CreatedAt,
                ResolvedAt = report.ResolvedAt,
                Status = report.Status
            };

        foreach (ViolationReportEvidence evidence in report.Evidence
            .OrderBy(e => e.UploadedAt))
        {
            result.Evidence.Add(new AdminViolationReportEvidenceViewModel
            {
                EvidenceId = evidence.EvidenceId,
                OriginalFileName = evidence.OriginalFileName,
                Extension = Path.GetExtension(evidence.FileName).ToLowerInvariant(),
                UploadedAt = evidence.UploadedAt
            });
        }

        foreach (ViolationReportAction action in report.Actions
            .OrderByDescending(a => a.CreatedAt)
            .ThenByDescending(a => a.ActionId))
        {
            result.Actions.Add(new AdminViolationReportActionViewModel
            {
                ActionId = action.ActionId,
                ActionType = action.ActionType,
                Note = action.Note,
                AdminEmail = action.Admin.Email,
                CreatedAt = action.CreatedAt
            });
        }

        return result;
    }

    public async Task<AdminViolationReportEvidenceFileViewModel?> GetEvidenceAsync(
        int reportId,
        int evidenceId)
    {
        ViolationReportEvidence evidence = await _context.ViolationReportEvidence
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.ReportId == reportId &&
                e.EvidenceId == evidenceId);

        if (evidence == null)
        {
            return null;
        }

        return new AdminViolationReportEvidenceFileViewModel
        {
            FileName = evidence.FileName,
            OriginalFileName = evidence.OriginalFileName
        };
    }

    public async Task<string?> UpdateStatusAsync(
        int reportId,
        int adminId,
        int status,
        string? note)
    {
        if (status < 0 || status > 3)
        {
            return "Trạng thái không hợp lệ.";
        }

        if (string.IsNullOrWhiteSpace(note))
        {
            note = null;
        }
        else
        {
            note = note.Trim();
        }

        if (status == 2 && note == null)
        {
            return "Vui lòng nhập lý do xác nhận vi phạm để khóa tài khoản Guide.";
        }

        if (status == 2 && note.Length > 500)
        {
            return "Lý do khóa tài khoản không được vượt quá 500 ký tự.";
        }

        if (status != 2 && note != null && note.Length > 1000)
        {
            return "Ghi chú không được vượt quá 1000 ký tự.";
        }

        bool adminExists = await _context.Users
            .AnyAsync(u =>
                u.UserId == adminId &&
                u.RoleId == AppRoles.AdminId &&
                u.Status == 1);

        if (adminExists == false)
        {
            return "Không xác định được tài khoản Admin thực hiện.";
        }

        ViolationReport report = await _context.ViolationReports
            .Include(r => r.GuideProfile)
                .ThenInclude(g => g.User)
            .FirstOrDefaultAsync(r => r.ReportId == reportId);

        if (report == null)
        {
            return "Không tìm thấy báo cáo vi phạm.";
        }

        if (report.Status == status)
        {
            return "Trạng thái báo cáo hiện tại đã được chọn.";
        }

        if (report.GuideProfile == null || report.GuideProfile.User == null)
        {
            return "Không tìm thấy tài khoản Guide liên quan đến báo cáo.";
        }

        DateTime now = DateTime.Now;

        report.Status = status;

        if (status == 2 || status == 3)
        {
            report.ResolvedAt = now;
        }
        else
        {
            report.ResolvedAt = null;
        }

        if (status == 2)
        {
            report.GuideProfile.User.Status = 0;
            report.GuideProfile.User.LockReason = note;
        }

        ViolationReportAction action = new ViolationReportAction
        {
            ReportId = reportId,
            AdminId = adminId,
            ActionType = status,
            Note = note,
            CreatedAt = now
        };

        _context.ViolationReportActions.Add(action);

        await _context.SaveChangesAsync();

        return null;
    }
}