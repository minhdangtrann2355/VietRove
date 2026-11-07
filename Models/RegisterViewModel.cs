
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace VietRove.Models
{
    public class RegisterViewModel : IValidatableObject
    {
        public string? Website { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 128 ký tự.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Range(1, 2, ErrorMessage = "Vui lòng chọn giới tính.")]
        public int Gender { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày sinh.")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 số và bắt đầu bằng số 0.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số CCCD/CMND.")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Số CCCD/CMND phải đúng 12 chữ số.")]
        public string IdCard { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nơi cấp.")]
        [StringLength(100, ErrorMessage = "Nơi cấp tối đa 100 ký tự.")]
        public string IssuePlace { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại thẻ.")]
        public string CardType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số thẻ HDV.")]
        [StringLength(50, ErrorMessage = "Số thẻ tối đa 50 ký tự.")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập ngày hết hạn thẻ.")]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }

        [Range(0, 40, ErrorMessage = "Số năm kinh nghiệm không hợp lệ.")]
        public int ExperienceYears { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn tỉnh/thành phố hoạt động.")]
        public int ProvinceId { get; set; }

        public List<int> LanguageIds { get; set; } = new List<int>();

        public List<string> LanguageNames { get; set; } = new List<string>();

        public List<IFormFile> LanguageCertificates { get; set; } = new List<IFormFile>();

        [Required(ErrorMessage = "Vui lòng tải lên ảnh đại diện.")]
        public IFormFile AvatarFile { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng tải ảnh CCCD mặt trước.")]
        public IFormFile IdCardFrontFile { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng tải ảnh CCCD mặt sau.")]
        public IFormFile IdCardBackFile { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng tải bản scan thẻ HDV.")]
        public IFormFile CardScanFile { get; set; } = null!;

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            // kiem tra mat khau
            if (string.IsNullOrWhiteSpace(Password) == false)
            {
                if (string.IsNullOrWhiteSpace(Email) == false)
                {
                    string cleanEmail = Email.Trim();

                    if (Password.Contains(
                        cleanEmail,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        yield return new ValidationResult(
                            "Mật khẩu không được phép chứa địa chỉ email.",
                            new[] { nameof(Password) });
                    }
                    else
                    {
                        string[] parts = cleanEmail.Split('@');

                        if (parts.Length > 0)
                        {
                            string emailPrefix = parts[0];

                            if (emailPrefix.Length >= 3 &&
                                Password.Contains(
                                    emailPrefix,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                yield return new ValidationResult(
                                    "Mật khẩu không được phép chứa phần tên định danh của email.",
                                    new[] { nameof(Password) });
                            }
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(FullName) == false)
                {
                    string[] names = FullName.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries);

                    foreach (string part in names)
                    {
                        if (part.Length >= 3 &&
                            Password.Contains(
                                part,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            yield return new ValidationResult(
                                "Mật khẩu không được chứa thông tin họ tên của bạn.",
                                new[] { nameof(Password) });

                            break;
                        }
                    }
                }
            }

            // kiem tra ngay sinh
            if (DateOfBirth == default ||
                DateOfBirth.Date >= DateTime.Today)
            {
                yield return new ValidationResult(
                    "Ngày sinh không hợp lệ.",
                    new[] { nameof(DateOfBirth) });
            }

            // kiem tra ngay cap the
            if (IssueDate == default ||
                IssueDate.Date > DateTime.Today)
            {
                yield return new ValidationResult(
                    "Ngày cấp thẻ không được lớn hơn ngày hiện tại.",
                    new[] { nameof(IssueDate) });
            }

            // kiem tra ngay het han
            if (ExpiryDate == default ||
                ExpiryDate.Date <= DateTime.Today ||
                (IssueDate != default && ExpiryDate.Date <= IssueDate.Date))
            {
                yield return new ValidationResult(
                    "Ngày hết hạn phải sau ngày hiện tại và ngày cấp thẻ.",
                    new[] { nameof(ExpiryDate) });
            }

            // kiem tra ngoai ngu va chung chi
            bool isInternationalCard = false;

            if (string.IsNullOrWhiteSpace(CardType) == false)
            {
                string cleanCard = CardType.Trim().ToLowerInvariant();

                if (cleanCard.Contains("quoc te") ||
                    cleanCard.Contains("quốc tế") ||
                    cleanCard.Contains("qu?c t?") ||
                    string.Equals(cleanCard, "hướng dẫn viên quốc tế", StringComparison.OrdinalIgnoreCase))
                {
                    isInternationalCard = true;
                }
            }

            if (isInternationalCard)
            {
                if (LanguageIds == null || LanguageIds.Count == 0)
                {
                    yield return new ValidationResult(
                        "Vui lòng chọn ít nhất một ngoại ngữ.",
                        new[] { nameof(LanguageIds) });
                }
                else
                {
                    if (LanguageIds.Any(id => id <= 0))
                    {
                        yield return new ValidationResult(
                            "Vui lòng chọn đầy đủ ngoại ngữ.",
                            new[] { nameof(LanguageIds) });
                    }

                    if (LanguageIds.Distinct().Count() != LanguageIds.Count)
                    {
                        yield return new ValidationResult(
                            "Không được chọn trùng ngoại ngữ.",
                            new[] { nameof(LanguageIds) });
                    }
                }

                if (LanguageCertificates == null ||
                    LanguageIds == null ||
                    LanguageCertificates.Count != LanguageIds.Count)
                {
                    yield return new ValidationResult(
                        "Thông tin ngoại ngữ và chứng chỉ không khớp.",
                        new[] { nameof(LanguageCertificates) });
                }
                else
                {
                    for (int i = 0; i < LanguageIds.Count; i++)
                    {
                        IFormFile certificate = LanguageCertificates[i];

                        if (certificate == null || certificate.Length == 0)
                        {
                            yield return new ValidationResult(
                                "Vui lòng tải chứng chỉ cho từng ngoại ngữ đã chọn.",
                                new[] { nameof(LanguageCertificates) });
                        }
                    }
                }
            }
        }
    }
}
