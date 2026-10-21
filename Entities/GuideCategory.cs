using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities
{
    [Table("tblGuideCategories")]
    public class GuideCategory
    {
        public int GuideId { get; set; }
        
        public int CategoryId { get; set; }
    }
}