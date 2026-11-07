using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietRove.Entities;

[Table("tblLoginHistory")]
public class LoginHistory
{
    [Key]
    public int LogId { get; set; }

    public int UserId { get; set; }

    public DateTime LoginTime { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}