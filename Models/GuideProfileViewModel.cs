using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using VietRove.Entities;

namespace VietRove.Models
{
    public class GuideProfileViewModel
    {
        public GuideProfile Profile { get; set; } = null!;
        public List<Category> Provinces { get; set; } = new List<Category>();
        public List<Category> Languages { get; set; } = new List<Category>();
        public List<Category> SelectedCategories { get; set; } = new List<Category>();
        public List<GuideLanguage> GuideLanguages { get; set; } = new List<GuideLanguage>();
        public List<GuideExperience> GuideExperiences { get; set; } = new List<GuideExperience>();
        public string StatusText { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }
        public bool IsRevisionRequired { get; set; }
        public string ActivityProvinceName { get; set; } = string.Empty;
    }

    public class GuideContactEditModel
    {
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 số và bắt đầu bằng số 0.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email liên hệ.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự.")]
        public string Address { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn tỉnh/thành hoạt động.")]
        public int ProvinceId { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }

    public class GuideLanguageAddModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn ngoại ngữ.")]
        public int LanguageId { get; set; }

        [Required(ErrorMessage = "Vui lòng tải tệp minh chứng ngoại ngữ.")]
        public IFormFile CertificateFile { get; set; } = null!;
    }

    public class GuideExperienceAddModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên kinh nghiệm.")]
        [StringLength(150, ErrorMessage = "Tên kinh nghiệm tối đa 150 ký tự.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
        public string Description { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Vui lòng tải tệp minh chứng kinh nghiệm.")]
        public IFormFile EvidenceFile { get; set; } = null!;
    }
}
