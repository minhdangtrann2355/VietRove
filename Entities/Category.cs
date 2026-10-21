using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities
{
    [Table("tblCategories")]
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        
        [MaxLength(50)]
        public string CategoryType { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;
    }
}