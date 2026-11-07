using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities;

[Table("tblViolationReportEvidence")]
public class ViolationReportEvidence
{
    [Key]
    public int EvidenceId { get; set; }

    public int ReportId { get; set; }

    [Required, MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? OriginalFileName { get; set; }

    public DateTime UploadedAt { get; set; }

    [ForeignKey(nameof(ReportId))]
    public virtual ViolationReport Report { get; set; } = null!;
}