using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities;

[Table("tblViolationReportActions")]
public class ViolationReportAction
{
    [Key]
    public int ActionId { get; set; }

    public int ReportId { get; set; }

    public int AdminId { get; set; }

    public int ActionType { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(ReportId))]
    public virtual ViolationReport Report { get; set; } = null!;

    [ForeignKey(nameof(AdminId))]
    public virtual User Admin { get; set; } = null!;
}