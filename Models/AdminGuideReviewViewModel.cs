namespace VietRove.Models
{
    public class AdminGuideReviewListItemViewModel
    {
        public int GuideId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Provinces { get; set; } = string.Empty;
    }

    public class AdminGuideReviewDetailViewModel
    {
        public int GuideId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public int? ExperienceYears { get; set; }
        public string CardType { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public List<string> Provinces { get; set; } = new();
        public List<AdminGuideReviewDocumentViewModel> Documents { get; set; } = new();
        public List<AdminGuideReviewLanguageViewModel> Languages { get; set; } = new();
    }

    public class AdminGuideReviewDocumentViewModel
    {
        public int DocumentId { get; set; }
        public int DocumentType { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public int VerificationStatus { get; set; }
        public string? VerificationReason { get; set; }
    }

    public class AdminGuideReviewLanguageViewModel
    {
        public int GuideLanguageId { get; set; }
        public string LanguageName { get; set; } = string.Empty;
        public string? CertificateFile { get; set; }
        public int VerificationStatus { get; set; }
        public string? VerificationReason { get; set; }
    }
}