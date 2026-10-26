using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities
{
    [Table("tblArticles")]
    public class Article
    {
        [Key]
        public int ArticleId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Thumbnail { get; set; }

        public int AuthorId { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey("AuthorId")]
        public virtual User Author { get; set; } = null!;
    }
}