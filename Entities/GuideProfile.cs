using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities;

[Table("tblGuideProfiles")]
public class GuideProfile
{
    [Key]
    public int GuideId { get; set; }

    public int UserId { get; set; }

    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(500)]
    public string? Avatar { get; set; }

    public int? CardTypeId { get; set; }

    [MaxLength(100)]
    public string? CardNumber { get; set; }

    public DateTime? IssueDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(50)]
    public string? IdentityNumber { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    [MaxLength(255)]
    public string? ContactEmail { get; set; }

    public int? ExperienceYears { get; set; }

    public int Status { get; set; } = 0;

    [MaxLength(1000)]
    public string? RejectReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(CardTypeId))]
    public virtual Category? CardType { get; set; }

    public virtual ICollection<GuideProvince> GuideProvinces { get; set; } = new List<GuideProvince>();

    public virtual ICollection<GuideLanguage> GuideLanguages { get; set; } = new List<GuideLanguage>();

    public virtual ICollection<GuideExperience> GuideExperiences { get; set; } = new List<GuideExperience>();

    public virtual ICollection<GuideDocument> GuideDocuments { get; set; } = new List<GuideDocument>();

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<ViolationReport> ViolationReports { get; set; } = new List<ViolationReport>();
}