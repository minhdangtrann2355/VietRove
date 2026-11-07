using Microsoft.AspNetCore.Http;

namespace VietRove.Services.Interfaces.FileStorage
{
    public interface IFileStorageService
    {
        Task<string> ValidateAvatarAsync(IFormFile file);

        Task<string> ValidateEvidenceAsync(IFormFile file);

        Task<string> SaveFileAsync(IFormFile file);

        void DeleteFile(string fileName);

        Task<Stream?> OpenReadAsync(string fileName);
        Task<string> SaveGeneratedPdfAsync(byte[] pdfBytes);
    }
}