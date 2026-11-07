using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities;

[Table("tblGuideExperiences")]
public class GuideExperience
{
    [Key]
    public int ExperienceId { get; set; }

    public int GuideId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [MaxLength(500)]
    public string? EvidenceFile { get; set; }

    public int VerificationStatus { get; set; } = 0;

    [MaxLength(1000)]
    public string? VerificationReason { get; set; }

    public int? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(GuideId))]
    public virtual GuideProfile GuideProfile { get; set; } = null!;

    [ForeignKey(nameof(VerifiedBy))]
    public virtual User? Verifier { get; set; }
}