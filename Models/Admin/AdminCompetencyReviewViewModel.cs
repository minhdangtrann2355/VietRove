using System;
using System.Collections.Generic;

namespace VietRove.Models.Admin
{
    public class AdminCompetencyReviewViewModel
    {
        public List<AdminPendingLanguageViewModel> Languages { get; set; } = new List<AdminPendingLanguageViewModel>();

        public List<AdminPendingExperienceViewModel> Experiences { get; set; } = new List<AdminPendingExperienceViewModel>();
    }

    public class AdminPendingLanguageViewModel
    {
        public int GuideLanguageId { get; set; }

        public int GuideId { get; set; }

        public string GuideName { get; set; } = string.Empty;

        public string GuideEmail { get; set; } = string.Empty;

        public string LanguageName { get; set; } = string.Empty;

        public string CertificateFile { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class AdminPendingExperienceViewModel
    {
        public int ExperienceId { get; set; }

        public int GuideId { get; set; }

        public string GuideName { get; set; } = string.Empty;

        public string GuideEmail { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string EvidenceFile { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}