using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VietRove.Models.Admin;

public class AdminViolationReportSearchViewModel
{
    [MaxLength(254)]
    public string? Search { get; set; }

    public int? Status { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    [Range(1, 1000000)]
    public int Page { get; set; } = 1;
}

public class AdminViolationReportListItemViewModel
{
    public int ReportId { get; set; }

    public string ReporterName { get; set; } = string.Empty;

    public string? ReporterPhone { get; set; }

    public string GuideName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public int Status { get; set; }

    public int EvidenceCount { get; set; }
}

public class AdminViolationReportResultViewModel
{
    public List<AdminViolationReportListItemViewModel> Items { get; set; } = new();

    public int TotalRecords { get; set; }

    public int CurrentPage { get; set; }

    public int PageSize { get; set; } = 10;

    public int TotalPages { get; set; }
}

public class AdminViolationReportDetailViewModel
{
    public int ReportId { get; set; }

    public string ReporterName { get; set; } = string.Empty;

    public string? ReporterPhone { get; set; }

    public string Description { get; set; } = string.Empty;

    public string GuideName { get; set; } = string.Empty;

    public string? GuideEmail { get; set; }

    public string? GuidePhone { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public int Status { get; set; }

    public List<AdminViolationReportEvidenceViewModel> Evidence { get; set; } = new();

    public List<AdminViolationReportActionViewModel> Actions { get; set; } = new();
}

public class AdminViolationReportEvidenceViewModel
{
    public int EvidenceId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string Extension { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }
}

public class AdminViolationReportActionViewModel
{
    public int ActionId { get; set; }

    public int ActionType { get; set; }

    public string? Note { get; set; }

    public string AdminEmail { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class AdminViolationReportUpdateStatusViewModel
{
    [Range(0, 3)]
    public int Status { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }
}

public class AdminViolationReportEvidenceFileViewModel
{
    public string FileName { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;
}