using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities;

[Table("tblGuideLanguages")]
public class GuideLanguage
{
    [Key]
    public int GuideLanguageId { get; set; }

    public int GuideId { get; set; }

    public int LanguageId { get; set; }

    [MaxLength(500)]
    public string? CertificateFile { get; set; }

    public int VerificationStatus { get; set; } = 0;

    [MaxLength(1000)]
    public string? VerificationReason { get; set; }

    public int? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(GuideId))]
    public virtual GuideProfile GuideProfile { get; set; } = null!;

    [ForeignKey(nameof(LanguageId))]
    public virtual Category Language { get; set; } = null!;

    [ForeignKey(nameof(VerifiedBy))]
    public virtual User? Verifier { get; set; }
}