using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities;

[Table("tblViolationReports")]
public class ViolationReport
{
    [Key]
    public int ReportId { get; set; }

    public int GuideId { get; set; }

    [Required, MaxLength(150)]
    public string ReporterName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? ReporterPhone { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public int Status { get; set; } = 0;

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    [ForeignKey(nameof(GuideId))]
    public virtual GuideProfile GuideProfile { get; set; } = null!;

    public virtual ICollection<ViolationReportEvidence> Evidence { get; set; } = new List<ViolationReportEvidence>();

    public virtual ICollection<ViolationReportAction> Actions { get; set; } = new List<ViolationReportAction>();
}