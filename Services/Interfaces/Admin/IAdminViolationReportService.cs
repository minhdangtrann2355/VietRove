using System.Threading.Tasks;
using VietRove.Models.Admin;

namespace VietRove.Services.Interfaces.Admin;

public interface IAdminViolationReportService
{
    Task<AdminViolationReportResultViewModel> GetReportsAsync(
        AdminViolationReportSearchViewModel searchModel);

    Task<AdminViolationReportDetailViewModel?> GetReportDetailAsync(
        int reportId);

    Task<AdminViolationReportEvidenceFileViewModel?> GetEvidenceAsync(
        int reportId,
        int evidenceId);

    Task<string?> UpdateStatusAsync(
        int reportId,
        int adminId,
        int status,
        string? note);
}