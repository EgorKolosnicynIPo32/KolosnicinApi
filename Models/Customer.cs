using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kolosnycin.Models;

[Table("customers")]
public class Customer
{
    [Key]
    [Column("id")]
    public long Id { get; set; }  // bigint в БД -> long в C#

    [Column("name")]
    [StringLength(255)]
    public string? Name { get; set; }  // nullable в БД

    [Column("email")]
    [StringLength(255)]
    public string? Email { get; set; }  // nullable в БД

    [Column("address")]
    [StringLength(255)]
    public string? Address { get; set; }  // nullable в БД
}