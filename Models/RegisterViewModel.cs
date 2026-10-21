using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace VietRove.Models
{
    public class RegisterViewModel : IValidatableObject
    {
        public string Website { get; set; } = string.Empty;

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

        [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
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
        [RegularExpression(@"^(\d{12})$", ErrorMessage = "Số CCCD/CMND phải đúng 12 chữ số.")]
        public string IdCard { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; }

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

        [Required(ErrorMessage = "Vui lòng nhập số năm kinh nghiệm.")]
        [Range(0, 40, ErrorMessage = "Số năm kinh nghiệm không hợp lệ.")]
        public int ExperienceYears { get; set; }

        public int ProvinceId { get; set; }

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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                yield break;
            }

            if (string.IsNullOrWhiteSpace(Email) == false)
            {
                string cleanEmail = Email.Trim();

                if (Password.Contains(cleanEmail, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new ValidationResult("Mật khẩu không được phép chứa địa chỉ email.", new[] { "Password" });
                }
                else
                {
                    string[] parts = cleanEmail.Split('@');
                    if (parts.Length > 0)
                    {
                        string emailPrefix = parts[0];
                        if (emailPrefix.Length >= 3)
                        {
                            if (Password.Contains(emailPrefix, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return new ValidationResult("Mật khẩu không được phép chứa phần tên định danh của email.", new[] { "Password" });
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(FullName) == false)
            {
                string[] names = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in names)
                {
                    if (part.Length >= 3)
                    {
                        if (Password.Contains(part, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return new ValidationResult("Mật khẩu không được chứa thông tin họ tên của bạn.", new[] { "Password" });
                            break;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(CardType) && CardType.Equals("Quốc tế", StringComparison.OrdinalIgnoreCase))
            {
                if (LanguageNames == null || LanguageNames.Count == 0 || LanguageNames.All(string.IsNullOrWhiteSpace))
                {
                    yield return new ValidationResult("Vui lòng nhập Ngoại ngữ hướng dẫn đối với Thẻ Quốc tế.", new[] { nameof(LanguageNames) });
                }

                if (LanguageCertificates == null || LanguageCertificates.Count == 0 || LanguageCertificates.All(f => f == null || f.Length == 0))
                {
                    yield return new ValidationResult("Vui lòng tải lên Văn bằng/Chứng chỉ ngoại ngữ đối với Thẻ Quốc tế.", new[] { nameof(LanguageCertificates) });
                }
            }
        }
    }
}