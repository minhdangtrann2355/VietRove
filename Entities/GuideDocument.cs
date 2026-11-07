using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities;

[Table("tblGuideDocuments")]
public class GuideDocument
{
    [Key]
    public int DocumentId { get; set; }
    public int GuideId { get; set; }
    public int DocumentType { get; set; }

    [Required, MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? OriginalFileName { get; set; }

    public int VerificationStatus { get; set; } = 0;

    [MaxLength(1000)]
    public string? VerificationReason { get; set; }

    public DateTime UploadedAt { get; set; }
    public int? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }

    [ForeignKey(nameof(GuideId))]
    public virtual GuideProfile GuideProfile { get; set; } = null!;

    [ForeignKey(nameof(VerifiedBy))]
    public virtual User? Verifier { get; set; }
}
