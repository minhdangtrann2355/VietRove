
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Models.Authentication;
using VietRove.Security;
using VietRove.Services.Interfaces.Authentication.Register;
using VietRove.Services.Interfaces.FileStorage;

namespace VietRove.Services.Authentication.Register
{
    public class RegisterService : IRegisterService
    {
        private readonly VietRoveDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        private const int IdentityFrontDocument = 1;
        private const int IdentityBackDocument = 2;
        private const int GuideCardDocument = 3;

        private const string InternationalCardName = "Hướng dẫn viên quốc tế";

        public RegisterService(
            VietRoveDbContext context,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        public async Task<(bool Success, string Message, string? Field)> RegisterAsync(
            RegisterViewModel model)
        {
            List<string> savedFiles = new List<string>();

            try
            {
                if (model == null)
                {
                    return (false, "Thông tin đăng ký không hợp lệ.", null);
                }

                // kiem tra cac thong tin bat buoc
                if (string.IsNullOrWhiteSpace(model.Email) ||
                    string.IsNullOrWhiteSpace(model.Password) ||
                    string.IsNullOrWhiteSpace(model.FullName) ||
                    string.IsNullOrWhiteSpace(model.Phone) ||
                    string.IsNullOrWhiteSpace(model.IdCard) ||
                    string.IsNullOrWhiteSpace(model.Address) ||
                    string.IsNullOrWhiteSpace(model.CardType) ||
                    string.IsNullOrWhiteSpace(model.CardNumber))
                {
                    return (false, "Vui lòng nhập đầy đủ thông tin bắt buộc.", null);
                }

                if (model.Password.Length < 8 || model.Password.Length > 128)
                {
                    return (false, "Mật khẩu phải có từ 8 đến 128 ký tự.", "Password");
                }

                if (model.Gender != 1 && model.Gender != 2)
                {
                    return (false, "Giới tính không hợp lệ.", "Gender");
                }

                if (model.ExperienceYears < 0 || model.ExperienceYears > 40)
                {
                    return (false, "Số năm kinh nghiệm không hợp lệ.", "ExperienceYears");
                }

                model.Email = model.Email.Trim().ToLowerInvariant();
                model.FullName = model.FullName.Trim();
                model.Phone = model.Phone.Trim();
                model.IdCard = model.IdCard.Trim();
                model.Address = model.Address.Trim();
                model.CardType = model.CardType.Trim();
                model.CardNumber = model.CardNumber.Trim();

                bool emailExists = await _context.Users
                    .AnyAsync(u => u.Email == model.Email);

                if (emailExists)
                {
                    return (false, "Email này đã được đăng ký trên hệ thống.", "Email");
                }

                VietRove.Entities.Category? cardType = await _context.Categories
                    .FirstOrDefaultAsync(c =>
                        c.CategoryType == "CardType" &&
                        c.CategoryName == model.CardType &&
                        c.IsActive);

                if (cardType == null)
                {
                    return (false, "Loại thẻ không hợp lệ hoặc đã ngừng hoạt động.", "CardType");
                }

                VietRove.Entities.Category? province = await _context.Categories
                    .FirstOrDefaultAsync(c =>
                        c.CategoryId == model.ProvinceId &&
                        c.CategoryType == "Province" &&
                        c.IsActive);

                if (province == null)
                {
                    return (false, "Tỉnh/thành phố hoạt động không hợp lệ.", "ProvinceId");
                }

                bool isInternationalCard = IsInternationalCard(cardType.CategoryName);
                List<int> validLanguageIds = new List<int>();
                List<string> certificateFileNames = new List<string>();

                if (isInternationalCard)
                {
                    if (model.LanguageIds == null ||
                        model.LanguageCertificates == null ||
                        model.LanguageIds.Count == 0)
                    {
                        return (false, "Vui lòng chọn ít nhất một ngoại ngữ.", "LanguageIds");
                    }

                    if (model.LanguageIds.Count != model.LanguageCertificates.Count)
                    {
                        return (false, "Thông tin ngoại ngữ và chứng chỉ không khớp.", "LanguageCertificates");
                    }

                    if (model.LanguageIds.Any(id => id <= 0))
                    {
                        return (false, "Vui lòng chọn đầy đủ ngoại ngữ.", "LanguageIds");
                    }

                    if (model.LanguageIds.Distinct().Count() != model.LanguageIds.Count)
                    {
                        return (false, "Không được chọn trùng ngoại ngữ.", "LanguageIds");
                    }

                    validLanguageIds = model.LanguageIds.ToList();

                    List<int> activeLanguageIds = await _context.Categories
                        .Where(c =>
                            c.CategoryType == "Language" &&
                            c.IsActive &&
                            validLanguageIds.Contains(c.CategoryId))
                        .Select(c => c.CategoryId)
                        .ToListAsync();

                    if (activeLanguageIds.Count != validLanguageIds.Count)
                    {
                        return (false, "Có ngoại ngữ không hợp lệ hoặc đã ngừng hoạt động.", "LanguageIds");
                    }

                    for (int i = 0; i < model.LanguageIds.Count; i++)
                    {
                        IFormFile? certificateFile = model.LanguageCertificates[i];

                        if (certificateFile == null || certificateFile.Length == 0)
                        {
                            return (false, "Vui lòng tải minh chứng cho từng ngoại ngữ đã chọn.", "LanguageCertificates");
                        }

                        string certificateError =
                            await _fileStorageService.ValidateEvidenceAsync(certificateFile);

                        if (string.IsNullOrEmpty(certificateError) == false)
                        {
                            return (false, certificateError, "LanguageCertificates");
                        }
                    }
                }

                // kiem tra ngay thang
                if (model.DateOfBirth == default ||
                    model.DateOfBirth.Date >= DateTime.Today)
                {
                    return (false, "Ngày sinh không hợp lệ.", "DateOfBirth");
                }

                if (model.IssueDate == default ||
                    model.IssueDate.Date > DateTime.Today)
                {
                    return (false, "Ngày cấp thẻ không hợp lệ.", "IssueDate");
                }

                if (model.ExpiryDate == default ||
                    model.ExpiryDate.Date <= DateTime.Today ||
                    model.ExpiryDate.Date <= model.IssueDate.Date)
                {
                    return (false, "Ngày hết hạn phải sau ngày hiện tại và ngày cấp thẻ.", "ExpiryDate");
                }

                // kiem tra file
                string avatarError =
                    await _fileStorageService.ValidateAvatarAsync(model.AvatarFile);

                if (string.IsNullOrEmpty(avatarError) == false)
                {
                    return (false, avatarError, "AvatarFile");
                }

                string frontError =
                    await _fileStorageService.ValidateEvidenceAsync(model.IdCardFrontFile);

                if (string.IsNullOrEmpty(frontError) == false)
                {
                    return (false, frontError, "IdCardFrontFile");
                }

                string backError =
                    await _fileStorageService.ValidateEvidenceAsync(model.IdCardBackFile);

                if (string.IsNullOrEmpty(backError) == false)
                {
                    return (false, backError, "IdCardBackFile");
                }

                string cardError =
                    await _fileStorageService.ValidateEvidenceAsync(model.CardScanFile);

                if (string.IsNullOrEmpty(cardError) == false)
                {
                    return (false, cardError, "CardScanFile");
                }

                // luu file truoc khi mo transaction database
                string avatarFileName =
                    await _fileStorageService.SaveFileAsync(model.AvatarFile);

                savedFiles.Add(avatarFileName);

                string frontFileName =
                    await _fileStorageService.SaveFileAsync(model.IdCardFrontFile);

                savedFiles.Add(frontFileName);

                string backFileName =
                    await _fileStorageService.SaveFileAsync(model.IdCardBackFile);

                savedFiles.Add(backFileName);

                string cardFileName =
                    await _fileStorageService.SaveFileAsync(model.CardScanFile);

                savedFiles.Add(cardFileName);

                if (isInternationalCard)
                {
                    for (int i = 0; i < model.LanguageIds.Count; i++)
                    {
                        string certificateFileName =
                            await _fileStorageService.SaveFileAsync(
                                model.LanguageCertificates[i]);

                        savedFiles.Add(certificateFileName);
                        certificateFileNames.Add(certificateFileName);
                    }
                }

                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    User newUser = new User
                    {
                        Email = model.Email,
                        Password = BCrypt.Net.BCrypt.HashPassword(
                            model.Password,
                            workFactor: 12),
                        RoleId = AppRoles.GuideId,
                        Status = 1,
                        CreatedAt = DateTime.Now
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    GuideProfile profile = new GuideProfile
                    {
                        UserId = newUser.UserId,
                        FullName = model.FullName,
                        Gender = GetGenderName(model.Gender),
                        DateOfBirth = model.DateOfBirth,
                        Avatar = avatarFileName,
                        CardTypeId = cardType.CategoryId,
                        CardNumber = model.CardNumber,
                        IssueDate = model.IssueDate,
                        ExpiryDate = model.ExpiryDate,
                        IdentityNumber = model.IdCard,
                        Address = model.Address,
                        ContactPhone = model.Phone,
                        ContactEmail = model.Email,
                        ExperienceYears = model.ExperienceYears,
                        Status = (int)ProfileStatus.Pending,
                        RejectReason = null,
                        CreatedAt = DateTime.Now
                    };

                    _context.GuideProfiles.Add(profile);
                    await _context.SaveChangesAsync();

                    GuideProvince guideProvince = new GuideProvince
                    {
                        GuideId = profile.GuideId,
                        ProvinceId = province.CategoryId
                    };

                    _context.GuideProvinces.Add(guideProvince);

                    AddGuideDocument(
                        profile.GuideId,
                        IdentityFrontDocument,
                        frontFileName,
                        model.IdCardFrontFile.FileName);

                    AddGuideDocument(
                        profile.GuideId,
                        IdentityBackDocument,
                        backFileName,
                        model.IdCardBackFile.FileName);

                    AddGuideDocument(
                        profile.GuideId,
                        GuideCardDocument,
                        cardFileName,
                        model.CardScanFile.FileName);

                    if (isInternationalCard)
                    {
                        for (int i = 0; i < validLanguageIds.Count; i++)
                        {
                            GuideLanguage guideLanguage = new GuideLanguage
                            {
                                GuideId = profile.GuideId,
                                LanguageId = validLanguageIds[i],
                                CertificateFile = certificateFileNames[i],
                                VerificationStatus = 0,
                                CreatedAt = DateTime.Now
                            };

                            _context.GuideLanguages.Add(guideLanguage);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (
                        true,
                        "Nộp hồ sơ hội viên thành công! Hồ sơ đang chờ Ban quản lý xét duyệt.",
                        null);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (DbUpdateException)
            {
                DeleteSavedFiles(savedFiles);

                return (
                    false,
                    "Dữ liệu đăng ký bị trùng hoặc không hợp lệ. Vui lòng kiểm tra lại thông tin.",
                    null);
            }
            catch (Exception)
            {
                DeleteSavedFiles(savedFiles);

                return (
                    false,
                    "Đã xảy ra sự cố trong quá trình lưu hồ sơ. Vui lòng thử lại sau.",
                    null);
            }
        }

        private void AddGuideDocument(
            int guideId,
            int documentType,
            string fileName,
            string originalFileName)
        {
            GuideDocument document = new GuideDocument
            {
                GuideId = guideId,
                DocumentType = documentType,
                FileName = fileName,
                OriginalFileName = Path.GetFileName(originalFileName),
                VerificationStatus = 0,
                UploadedAt = DateTime.Now
            };

            _context.GuideDocuments.Add(document);
        }

        private void DeleteSavedFiles(List<string> savedFiles)
        {
            foreach (string fileName in savedFiles)
            {
                try
                {
                    _fileStorageService.DeleteFile(fileName);
                }
                catch (Exception)
                {
                    // bo qua loi don dep de khong anh huong ket qua dang ky
                }
            }
        }

        private bool IsInternationalCard(string cardName)
        {
            if (string.IsNullOrWhiteSpace(cardName))
            {
                return false;
            }

            string clean = cardName.Trim().ToLowerInvariant();

            if (clean.Contains("quoc te") ||
                clean.Contains("quốc tế") ||
                clean.Contains("qu?c t?") ||
                string.Equals(clean, "hướng dẫn viên quốc tế", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private string GetGenderName(int gender)
        {
            if (gender == 1)
            {
                return "Nam";
            }

            if (gender == 2)
            {
                return "Nữ";
            }

            return string.Empty;
        }
    }
}
