namespace VietRove.Models.Admin
{
    public class AdminLoginHistorySearchViewModel
    {
        public string? Search { get; set; }
        public int? RoleId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
    }

    public class LoginHistoryItemViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
    }

    public class AdminLoginHistoryResultViewModel
    {
        public List<LoginHistoryItemViewModel> Items { get; set; } = new();
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}