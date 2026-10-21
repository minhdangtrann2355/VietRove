using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities
{
    [Table("tblGuideProfiles")]
    public class GuideProfile
    {
        [Key]
        public int GuideId { get; set; }
        
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public int Gender { get; set; }
        
        public DateTime DateOfBirth { get; set; }

        [MaxLength(255)]
        public string Avatar { get; set; } = string.Empty;

        [MaxLength(50)]
        public string CardType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string CardNumber { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        [MaxLength(50)]
        public string IdCard { get; set; } = string.Empty;

        public DateTime IssueDate { get; set; }

        [MaxLength(100)]
        public string IssuePlace { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        public int ExperienceYears { get; set; }

        public int Status { get; set; } 

        [MaxLength(255)]
        public string RejectReason { get; set; } = string.Empty;

        [MaxLength(255)]
        public string IdCardFront { get; set; } = string.Empty;

        [MaxLength(255)]
        public string IdCardBack { get; set; } = string.Empty;

        [MaxLength(255)]
        public string CardScan { get; set; } = string.Empty;

        [MaxLength(255)]
        public string DegreeScan { get; set; } = string.Empty;
    }
}