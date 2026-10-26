using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities
{
    [Table("tblViolationReports")]
    public class ViolationReport
    {
        [Key]
        public int ReportId { get; set; }

        public int GuideId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ReporterName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ReporterPhone { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? EvidenceFile { get; set; }

        public int? Status { get; set; } = 0;

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("GuideId")]
        public virtual GuideProfile GuideProfile { get; set; } = null!;
    }
}