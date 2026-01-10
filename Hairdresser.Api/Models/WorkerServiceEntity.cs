using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hairdresser.Api.Models;

public class WorkerServiceEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("service_name")]
    [Required]
    public string ServiceName { get; set; } = null!;

    [Column("duration_minutes")]
    [Required]
    public int DurationMinutes { get; set; }

    [Column("price")]
    [Required]
    public decimal Price { get; set; }

    
}