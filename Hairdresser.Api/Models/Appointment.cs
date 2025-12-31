using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hairdresser.Api.Models;

[Table("appointments")]
public class Appointment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("worker_id")]
    public int WorkerId { get; set; }

    [Column("appointment_date")]
    [Required]
    public DateOnly AppointmentDate { get; set; }

    [Column("appointment_time")]
    [Required]
    public TimeOnly AppointmentTime { get; set; }

    [Column("duration_minutes")]
    public int DurationMinutes { get; set; } = 60;

    [Column("status")]
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "pending";

    [Column("service_type")]
    [StringLength(100)]
    public string? ServiceType { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [ForeignKey("WorkerId")]
    public Worker? Worker { get; set; }
}