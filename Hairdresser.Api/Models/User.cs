using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hairdresser.Api.Models;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("phone_number")]
    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("name")]
    [StringLength(100)]
    public string? Name { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("last_contact")]
    public DateTime LastContact { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}