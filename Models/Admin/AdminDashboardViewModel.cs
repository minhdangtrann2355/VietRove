using System;
using System.Collections.Generic;

namespace VietRove.Models.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }

        public int TotalGuides { get; set; }

        public int PendingGuideProfiles { get; set; }

        public int PendingViolationReports { get; set; }

        public int PendingGuideLanguages { get; set; }

        public int PendingGuideExperiences { get; set; }

        public List<AdminDashboardRecentLoginViewModel> RecentLogins { get; set; } = new();
    }

    public class AdminDashboardRecentLoginViewModel
    {
        public string Email { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public DateTime LoginTime { get; set; }
    }
}