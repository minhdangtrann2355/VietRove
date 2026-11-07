using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities
{
    [Table("tblUsers")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(255)")]
        public string Password { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public int Status { get; set; } = 1;

        public DateTime CreatedAt { get; set; }

        [MaxLength(500)]
        public string? LockReason { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; } = null!;

        public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();

        public virtual ICollection<GuideLanguage> VerifiedLanguages { get; set; } = new List<GuideLanguage>();

        public virtual ICollection<GuideExperience> VerifiedExperiences { get; set; } = new List<GuideExperience>();

        public virtual ICollection<GuideDocument> VerifiedDocuments { get; set; } = new List<GuideDocument>();

        public virtual ICollection<ViolationReportAction> ViolationReportActions { get; set; } = new List<ViolationReportAction>();

        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}