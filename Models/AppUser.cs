using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kolosnycin.Models;

[Table("app_users")]
public class AppUser
{
    [Key]
    [Column("id")]
    public long Id { get; set; }  // bigint в БД

    [Required]
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;  // NOT NULL в БД

    [Required]
    [Column("email")]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;  // NOT NULL, уникальный

    [Required]
    [Column("password_hash")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;  // NOT NULL

    [Column("address")]
    [StringLength(255)]
    public string? Address { get; set; }  // nullable в БД

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }  // timestamp with time zone, default now()

    [Column("is_active")]
    public bool? IsActive { get; set; } = true;  // boolean, default true
}