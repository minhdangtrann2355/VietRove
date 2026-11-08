using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace VietRove.Models.Guest
{
    public class ViolationReportCreateViewModel
    {
        public int GuideId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(150, ErrorMessage = "Họ tên không được vượt quá 150 ký tự.")]
        public string ReporterName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        public string? ReporterPhone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung báo cáo.")]
        public string Description { get; set; } = string.Empty;

        public List<IFormFile> EvidenceFiles { get; set; } = new List<IFormFile>();
    }
}