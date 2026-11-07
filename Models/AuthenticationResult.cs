using VietRove.Entities;

namespace VietRove.Models
{
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
        public GuideProfile? GuideProfile { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string DashboardController { get; set; } = string.Empty;
    }
}