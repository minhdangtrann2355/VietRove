using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities;

[Table("tblGuideProvinces")]
public class GuideProvince
{
    public int GuideId { get; set; }
    public int ProvinceId { get; set; }

    [ForeignKey(nameof(GuideId))]
    public virtual GuideProfile GuideProfile { get; set; } = null!;

    [ForeignKey(nameof(ProvinceId))]
    public virtual Category Province { get; set; } = null!;
}
