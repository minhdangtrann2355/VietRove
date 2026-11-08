namespace VietRove.Services.Interfaces.Guest
{
    public interface IGuestViolationReportService
    {
        Task<string> CreateReportAsync(
            int guideId,
            string reporterName,
            string? reporterPhone,
            string description,
            List<IFormFile> evidenceFiles);
    }
}