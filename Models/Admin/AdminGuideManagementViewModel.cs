
using System.ComponentModel.DataAnnotations;

namespace VietRove.Models.Admin
{
    public class AdminGuideManagementSearchViewModel
    {
        [MaxLength(254)]
        public string? Search { get; set; }

        public int? AccountStatus { get; set; }

        public int? ProfileStatus { get; set; }

        [Range(1, 1000000)]
        public int Page { get; set; } = 1;
    }

    public class AdminGuideManagementListItemViewModel
    {
        public int UserId { get; set; }

        public int? GuideId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? ContactPhone { get; set; }

        public int AccountStatus { get; set; }

        public string? LockReason { get; set; }

        public int? ProfileStatus { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class AdminGuideManagementResultViewModel
    {
        public List<AdminGuideManagementListItemViewModel> Items { get; set; } = new List<AdminGuideManagementListItemViewModel>();

        public int TotalRecords { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }
    }

    public class AdminGuideManagementDetailViewModel
    {
        public int UserId { get; set; }

        public int? GuideId { get; set; }

        public string Email { get; set; } = string.Empty;

        public int AccountStatus { get; set; }

        public string? LockReason { get; set; }

        public DateTime AccountCreatedAt { get; set; }

        public string? FullName { get; set; }

        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Address { get; set; }

        public string? ContactPhone { get; set; }

        public string? ContactEmail { get; set; }

        public int? ExperienceYears { get; set; }

        public int? ProfileStatus { get; set; }

        public string? RejectReason { get; set; }

        public string? CardTypeName { get; set; }

        public string? CardNumber { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public List<string> Provinces { get; set; } = new List<string>();

        public List<AdminGuideLanguageItemViewModel> Languages { get; set; } = new List<AdminGuideLanguageItemViewModel>();

        public List<AdminGuideExperienceItemViewModel> Experiences { get; set; } = new List<AdminGuideExperienceItemViewModel>();
    }

    public class AdminGuideLanguageItemViewModel
    {
        public string LanguageName { get; set; } = string.Empty;

        public int VerificationStatus { get; set; }

        public string? VerificationReason { get; set; }

        public bool HasCertificate { get; set; }
    }

    public class AdminGuideExperienceItemViewModel
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int VerificationStatus { get; set; }

        public string? VerificationReason { get; set; }

        public bool HasEvidence { get; set; }
    }
}
