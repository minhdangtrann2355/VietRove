using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities
{
    [Table("tblContracts")]
    public class Contract
    {
        [Key]
        public int ContractId { get; set; }

        public int GuideId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? CustomerPhone { get; set; }

        [MaxLength(50)]
        public string? CustomerIdCard { get; set; }

        public string? Itinerary { get; set; }

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Fee { get; set; }

        [MaxLength(255)]
        public string? PdfFile { get; set; }

        [MaxLength(255)]
        public string? ScanFile { get; set; }

        public int? Status { get; set; } = 0;

        [MaxLength(255)]
        public string? RejectReason { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey("GuideId")]
        public virtual GuideProfile GuideProfile { get; set; } = null!;
    }
}