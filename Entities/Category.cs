using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities;

[Table("tblCategories")]
public class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required, MaxLength(50)]
    public string CategoryType { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string CategoryName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<GuideProfile> CardTypeProfiles { get; set; } = new List<GuideProfile>();
    public virtual ICollection<GuideProvince> GuideProvinces { get; set; } = new List<GuideProvince>();
    public virtual ICollection<GuideLanguage> GuideLanguages { get; set; } = new List<GuideLanguage>();
}
