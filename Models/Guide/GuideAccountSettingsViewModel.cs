
using System;
using System.ComponentModel.DataAnnotations;

namespace VietRove.Models.Guide
{
    public class GuideAccountSettingsViewModel
    {
        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải có từ 8 đến 128 ký tự.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
