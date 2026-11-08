
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities;

[Table("tblContracts")]
public class Contract
{
    [Key]
    public int ContractId { get; set; }

    public int GuideId { get; set; }

    [Required, MaxLength(150)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? CustomerPhone { get; set; }

    public string? Itinerary { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Fee { get; set; } = 0;

    [MaxLength(500)]
    public string? PdfFile { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(GuideId))]
    public virtual GuideProfile GuideProfile { get; set; } = null!;
}
