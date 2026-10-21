using System;
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
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public int Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}