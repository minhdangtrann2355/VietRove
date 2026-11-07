
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VietRove.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorageService> _logger;

        private const long MaxFileSize = 10 * 1024 * 1024;

        private static readonly Dictionary<string, byte[]> ImageSignatures =
            new Dictionary<string, byte[]>
            {
                { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
                { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
                { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } }
            };

        private static readonly Dictionary<string, byte[]> EvidenceSignatures =
            new Dictionary<string, byte[]>
            {
                { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
                { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
                { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
                { ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } }
            };

        public FileStorageService(
            IWebHostEnvironment environment,
            ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public Task<string> ValidateAvatarAsync(IFormFile file)
        {
            return Task.FromResult(ValidateFile(file, ImageSignatures, "Ảnh đại diện"));
        }

        public Task<string> ValidateEvidenceAsync(IFormFile file)
        {
            return Task.FromResult(ValidateFile(file, EvidenceSignatures, "Tệp minh chứng"));
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0 || file.Length > MaxFileSize)
            {
                throw new InvalidOperationException("Tệp tải lên không hợp lệ.");
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            Dictionary<string, byte[]> allowedSignatures = null;

            if (ImageSignatures.ContainsKey(extension))
            {
                allowedSignatures = ImageSignatures;
            }
            else if (EvidenceSignatures.ContainsKey(extension))
            {
                allowedSignatures = EvidenceSignatures;
            }

            if (allowedSignatures == null)
            {
                throw new InvalidOperationException("Định dạng tệp không được hỗ trợ.");
            }

            if (CheckFileSignature(file, allowedSignatures) == false)
            {
                throw new InvalidOperationException("Nội dung tệp không đúng định dạng.");
            }

            string uploadDirectory = GetUploadDirectory();

            Directory.CreateDirectory(uploadDirectory);

            string uniqueFileName = Guid.NewGuid().ToString("N") + extension;
            string fullPath = Path.Combine(uploadDirectory, uniqueFileName);

            try
            {
                using (FileStream stream = new FileStream(
                    fullPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await file.CopyToAsync(stream);
                    await stream.FlushAsync();
                }

                return uniqueFileName;
            }
            catch
            {
                if (File.Exists(fullPath))
                {
                    try
                    {
                        File.Delete(fullPath);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(
                            ex,
                            "Khong the xoa file tam: {FileName}",
                            uniqueFileName);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        _logger.LogError(
                            ex,
                            "Khong co quyen xoa file tam: {FileName}",
                            uniqueFileName);
                    }
                }

                throw;
            }
        }

        public void DeleteFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            string safeFileName = Path.GetFileName(fileName);

            if (safeFileName != fileName)
            {
                _logger.LogWarning(
                    "Tu choi xoa file co ten khong hop le: {FileName}",
                    fileName);
                return;
            }

            string extension = Path.GetExtension(safeFileName).ToLowerInvariant();

            bool isAllowed = ImageSignatures.ContainsKey(extension)
                || EvidenceSignatures.ContainsKey(extension);

            if (isAllowed == false)
            {
                _logger.LogWarning(
                    "Tu choi xoa file co dinh dang khong hop le: {FileName}",
                    safeFileName);
                return;
            }

            string nameWithoutExtension = Path.GetFileNameWithoutExtension(safeFileName);

            if (Guid.TryParseExact(nameWithoutExtension, "N", out Guid fileId) == false)
            {
                _logger.LogWarning(
                    "Tu choi xoa file khong duoc he thong tao: {FileName}",
                    safeFileName);
                return;
            }

            string uploadDirectory = GetUploadDirectory();
            string fullPath = Path.Combine(uploadDirectory, safeFileName);

            if (File.Exists(fullPath) == false)
            {
                return;
            }

            try
            {
                File.Delete(fullPath);
            }
            catch (IOException ex)
            {
                _logger.LogError(
                    ex,
                    "Khong the xoa file: {FileName}",
                    safeFileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(
                    ex,
                    "Khong co quyen xoa file: {FileName}",
                    safeFileName);
            }
        }

        private string ValidateFile(
            IFormFile file,
            Dictionary<string, byte[]> signatures,
            string fileType)
        {
            if (file == null || file.Length == 0)
            {
                return "Vui lòng chọn " + fileType.ToLowerInvariant() + ".";
            }

            if (file.Length > MaxFileSize)
            {
                return fileType + " không được vượt quá 10MB.";
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (signatures.ContainsKey(extension) == false)
            {
                return "Định dạng " + fileType.ToLowerInvariant() + " không được hỗ trợ.";
            }

            if (CheckFileSignature(file, signatures) == false)
            {
                return "Nội dung tệp không đúng định dạng chuẩn.";
            }

            return string.Empty;
        }

        private bool CheckFileSignature(
            IFormFile file,
            Dictionary<string, byte[]> signatures)
        {
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (signatures.TryGetValue(extension, out byte[] signature) == false)
            {
                return false;
            }

            using (BinaryReader reader = new BinaryReader(file.OpenReadStream()))
            {
                byte[] header = reader.ReadBytes(signature.Length);

                return header.Length == signature.Length
                    && header.SequenceEqual(signature);
            }
        }

        private string GetUploadDirectory()
        {
            return Path.Combine(
                _environment.ContentRootPath,
                "ProtectedUploads");
        }
    }
}
