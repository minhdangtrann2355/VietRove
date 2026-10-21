using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VietRove.Models;
using VietRove.Data; 
using VietRove.Entities; 
using Microsoft.EntityFrameworkCore; 

namespace VietRove.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly VietRoveDbContext _context; 

        private static readonly Dictionary<string, List<byte[]>> FileSignatures = new Dictionary<string, List<byte[]>>
        {
            { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }
        };

        public RegisterController(IWebHostEnvironment environment, VietRoveDbContext context)
        {
            _environment = environment;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Provinces = await _context.Categories
                .Where(c => c.CategoryType == "Province")
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            ViewBag.Languages = await _context.Categories
                .Where(c => c.CategoryType == "Language")
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            var model = new RegisterViewModel
            {
                DateOfBirth = DateTime.Today,
                IssueDate = DateTime.Today,
                ExpiryDate = DateTime.Today.AddYears(2) 
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(100 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
        public async Task<IActionResult> Index(RegisterViewModel model)
        {
            if (string.IsNullOrEmpty(model.Website) == false)
            {
                return Json(new { success = true, redirectUrl = "/" });
            }
            if (model.Email != null)
            {
                model.Email = model.Email.Trim().ToLowerInvariant();
            }
            else
            {
                model.Email = string.Empty;
            }

            if (model.FullName != null)
            {
                model.FullName = model.FullName.Trim();
            }
            else
            {
                model.FullName = string.Empty;
            }

            if (model.Phone != null)
            {
                model.Phone = model.Phone.Trim();
            }
            else
            {
                model.Phone = string.Empty;
            }

            if (model.IdCard != null)
            {
                model.IdCard = model.IdCard.Trim();
            }
            else
            {
                model.IdCard = string.Empty;
            }

            if (model.ExpiryDate != DateTime.MinValue)
            {
                if (model.ExpiryDate.Date <= DateTime.Today)
                {
                    ModelState.AddModelError("ExpiryDate", "Thẻ HDV đã hết hạn. Ngày hết hạn phải lớn hơn ngày hiện tại.");
                }
            }

            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                await ValidateUploadedFileSecurityAsync(model.AvatarFile, "AvatarFile", true);
            }

            if (model.IdCardFrontFile != null && model.IdCardFrontFile.Length > 0)
            {
                await ValidateUploadedFileSecurityAsync(model.IdCardFrontFile, "IdCardFrontFile", false);
            }

            if (model.IdCardBackFile != null && model.IdCardBackFile.Length > 0)
            {
                await ValidateUploadedFileSecurityAsync(model.IdCardBackFile, "IdCardBackFile", false);
            }

            if (model.CardScanFile != null && model.CardScanFile.Length > 0)
            {
                await ValidateUploadedFileSecurityAsync(model.CardScanFile, "CardScanFile", false);
            }

            if (model.LanguageCertificates != null && model.LanguageCertificates.Count > 0)
            {
                foreach (var certFile in model.LanguageCertificates)
                {
                    if (certFile != null && certFile.Length > 0)
                    {
                        await ValidateUploadedFileSecurityAsync(certFile, "LanguageCertificates", false);
                    }
                }
            }

            if (ModelState.IsValid == false)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors.Select(e => new {
                        field = x.Key,
                        message = e.ErrorMessage
                    }))
                    .ToList();

                return Json(new { success = false, message = "Vui lòng kiểm tra lại các trường không hợp lệ.", errors = errors });
            }

            try
            {
                bool isEmailExisting = await _context.Users!.AnyAsync(u => u.Email == model.Email);
                if (isEmailExisting)
                {
                     return Json(new { success = false, 

                        message = "Email này đã được sử dụng để đăng ký tài khoản. Vui lòng chọn email khác.", 
                        errors = new[] { new { field = "Email", message = "Email này đã được đăng ký trên hệ thống." } }
                    });
                }

                using (var transaction = await _context.Database!.BeginTransactionAsync())
                {
                    try
                    {
                        string plainPassword = model.Password;
                        string passwordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);

                        User newUser = new User();
                        newUser.Email = model.Email;
                        newUser.Password = passwordHash; 
                        newUser.RoleId = 2;
                        newUser.Status = 0; 
                        newUser.CreatedAt = DateTime.Now;

                        _context.Users!.Add(newUser);
                        await _context.SaveChangesAsync(); 

                        string secureUploadsFolder = Path.Combine(_environment.ContentRootPath, "ProtectedUploads");
                        
                        if (Directory.Exists(secureUploadsFolder) == false)
                        {
                            Directory.CreateDirectory(secureUploadsFolder);
                        }

                        string avatarFileName = string.Empty;
                        if (model.AvatarFile != null && model.AvatarFile.Length > 0)
                        {
                            avatarFileName = await SaveFileSecureAsync(model.AvatarFile, secureUploadsFolder);
                        }

                        string idFrontFileName = string.Empty;
                        if (model.IdCardFrontFile != null && model.IdCardFrontFile.Length > 0)
                        {
                            idFrontFileName = await SaveFileSecureAsync(model.IdCardFrontFile, secureUploadsFolder);
                        }

                        string idBackFileName = string.Empty;
                        if (model.IdCardBackFile != null && model.IdCardBackFile.Length > 0)
                        {
                            idBackFileName = await SaveFileSecureAsync(model.IdCardBackFile, secureUploadsFolder);
                        }

                        string cardScanFileName = string.Empty;
                        if (model.CardScanFile != null && model.CardScanFile.Length > 0)
                        {
                            cardScanFileName = await SaveFileSecureAsync(model.CardScanFile, secureUploadsFolder);
                        }

                        List<string> savedDegreeFiles = new List<string>();
                        if (model.LanguageCertificates != null && model.LanguageCertificates.Count > 0)
                        {
                            foreach (var certFile in model.LanguageCertificates)
                            {
                                if (certFile != null && certFile.Length > 0)
                                {
                                    string certFileName = await SaveFileSecureAsync(certFile, secureUploadsFolder);
                                    savedDegreeFiles.Add(certFileName);
                                }
                            }
                        }

                        GuideProfile profile = new GuideProfile();
                        profile.UserId = newUser.UserId; 
                        profile.FullName = model.FullName;
                        profile.Gender = model.Gender;
                        profile.Email = model.Email; 
                        profile.DateOfBirth = model.DateOfBirth;
                        profile.Phone = model.Phone;
                        profile.Address = model.Address;
                        profile.IdCard = model.IdCard;
                        profile.IssueDate = model.IssueDate;
                        profile.IssuePlace = model.IssuePlace;
                        profile.CardType = model.CardType;
                        profile.CardNumber = model.CardNumber;
                        profile.ExpiryDate = model.ExpiryDate;
                        profile.ExperienceYears = model.ExperienceYears;
                        profile.Status = 0; 
                        profile.RejectReason = string.Empty;
                        profile.Avatar = avatarFileName;
                        profile.IdCardFront = idFrontFileName;
                        profile.IdCardBack = idBackFileName;
                        profile.CardScan = cardScanFileName;
                        profile.DegreeScan = string.Join(";", savedDegreeFiles); 

                        _context.GuideProfiles!.Add(profile);
                        await _context.SaveChangesAsync();

                        if (model.ProvinceId > 0)
                        {
                            GuideCategory provinceCategory = new GuideCategory();
                            provinceCategory.GuideId = profile.GuideId;
                            provinceCategory.CategoryId = model.ProvinceId;

                            _context.GuideCategories!.Add(provinceCategory);
                        }

                        if (!string.IsNullOrEmpty(model.CardType) && model.CardType.Contains("Quốc tế", StringComparison.OrdinalIgnoreCase))
                        {
                            if (model.LanguageNames != null && model.LanguageNames.Count > 0)
                            {
                                foreach (var langName in model.LanguageNames)
                                {
                                    if (!string.IsNullOrWhiteSpace(langName))
                                    {
                                        string cleanLangName = langName.Trim();
                                        
                                        var category = await _context.Categories!
                                            .FirstOrDefaultAsync(c => c.CategoryName == cleanLangName && c.CategoryType == "Language");
                                            
                                        if (category == null)
                                        {
                                            category = new Category { CategoryName = cleanLangName, CategoryType = "Language" };
                                            _context.Categories!.Add(category);
                                            await _context.SaveChangesAsync(); 
                                        }

                                        GuideCategory languageCategory = new GuideCategory();
                                        languageCategory.GuideId = profile.GuideId;
                                        languageCategory.CategoryId = category.CategoryId;

                                        _context.GuideCategories!.Add(languageCategory);
                                    }
                                }
                            }
                        }

                        if (model.ProvinceId > 0 || (model.LanguageNames != null && model.LanguageNames.Count > 0))
                        {
                            await _context.SaveChangesAsync();
                        }

                        await transaction.CommitAsync();

                        return Json(new { success = true,
    
                            message = "Nộp hồ sơ hội viên thành công! Hồ sơ đang chờ Ban quản lý xét duyệt.", 
                            redirectUrl = "/"
                        });
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw; 
                    }
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Đã xảy ra sự cố trong quá trình lưu hồ sơ. Vui lòng thử lại sau." });
            }
        }

        private async Task ValidateUploadedFileSecurityAsync(IFormFile file, string modelKey, bool isImageOnly)
        {
            if (file.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError(modelKey, "Tệp vượt quá kích thước 10MB cho phép.");
                return;
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            string[] allowedExts;
            if (isImageOnly)
            {
                allowedExts = new string[] { ".jpg", ".jpeg", ".png" };
            }
            else
            {
                allowedExts = new string[] { ".jpg", ".jpeg", ".png", ".pdf" };
            }

            if (allowedExts.Contains(extension) == false)
            {
                ModelState.AddModelError(modelKey, "Định dạng tệp không được hỗ trợ.");
                return;
            }

            if (FileSignatures.ContainsKey(extension) == false)
            {
                ModelState.AddModelError(modelKey, "Định dạng tệp không được hỗ trợ.");
                return;
            }

            using (BinaryReader reader = new BinaryReader(file.OpenReadStream()))
            {
                List<byte[]> signatures = FileSignatures[extension];
                
                int maxLength = 0;
                foreach (byte[] sig in signatures)
                {
                    if (sig.Length > maxLength)
                    {
                        maxLength = sig.Length;
                    }
                }

                byte[] headerBytes = reader.ReadBytes(maxLength);
                bool isValid = false;

                foreach (byte[] sig in signatures)
                {
                    bool match = true;
                    for (int i = 0; i < sig.Length; i++)
                    {
                        if (headerBytes.Length <= i || headerBytes[i] != sig[i])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        isValid = true;
                        break;
                    }
                }
                
                if (isValid == false)
                {
                    ModelState.AddModelError(modelKey, "Tệp có dấu hiệu bị can thiệp hoặc giả mạo định dạng.");
                }
            }
            
            await Task.CompletedTask;
        }

        private async Task<string> SaveFileSecureAsync(IFormFile file, string uploadDir)
        {
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string secureFileName = Guid.NewGuid().ToString("N") + extension;
            string destinationPath = Path.Combine(uploadDir, secureFileName);

            using (FileStream stream = new FileStream(destinationPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return secureFileName;
        }
    }
}