using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hairdresser.Api.Models
{
    [Table("worker_schedules")]
    public class WorkerSchedule
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("worker_id")]
        public int WorkerId { get; set; }

        [Column("day_of_week")]
        [Required]
        [Display(Name = "Gün")]
        public int DayOfWeek { get; set; }

        [Column("start_time")]
        [Required]
        [Display(Name = "Başlangıç Saati")]
        public TimeOnly StartTime { get; set; }

        [Column("end_time")]
        [Required]
        [Display(Name = "Bitiş Saati")]
        public TimeOnly EndTime { get; set; }

        [Column("is_working")]
        [Display(Name = "Çalışıyor")]
        public bool IsWorking { get; set; } = true;

        [ForeignKey("WorkerId")]
        public Worker Worker { get; set; } = null!;

        [NotMapped]
        public string DayName => DayOfWeek switch
        {
            0 => "Pazar",
            1 => "Pazartesi",
            2 => "Salı",
            3 => "Çarşamba",
            4 => "Perşembe",
            5 => "Cuma",
            6 => "Cumartesi",
            _ => "Bilinmeyen"
        };
    }
}

