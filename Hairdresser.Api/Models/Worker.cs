using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hairdresser.Api.Models
{
    [Table("workers")]
    public class Worker
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required(ErrorMessage = "İsim zorunludur")]
        [StringLength(100)]
        [Display(Name = "İsim")]
        public string Name { get; set; } = null!;

        [Column("specialty")]
        [StringLength(100)]
        [Display(Name = "Uzmanlık Alanı")]
        public string? Specialty { get; set; }

        [Column("is_active")]
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WorkerSchedule> Schedules { get; set; } = new List<WorkerSchedule>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}

