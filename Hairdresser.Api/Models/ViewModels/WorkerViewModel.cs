using System.ComponentModel.DataAnnotations;

namespace Hairdresser.Api.Models.ViewModels
{
    public class WorkerViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "İsim zorunludur")]
        [StringLength(100)]
        [Display(Name = "İsim")]
        public string Name { get; set; } = null!;

        [StringLength(100)]
        [Display(Name = "Uzmanlık Alanı")]
        public string? Specialty { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        public List<WorkerScheduleViewModel> Schedules { get; set; } = new();
    }

    public class WorkerScheduleViewModel
    {
        public int Id { get; set; }
        public int DayOfWeek { get; set; }
        public string DayName { get; set; } = null!;
        
        [Display(Name = "Çalışıyor mu?")]
        public bool IsWorking { get; set; }

        [Display(Name = "Başlangıç")]
        public TimeOnly StartTime { get; set; } = new TimeOnly(9, 0);

        [Display(Name = "Bitiş")]
        public TimeOnly EndTime { get; set; } = new TimeOnly(18, 0);
    }

    public class WorkerListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Specialty { get; set; }
        public bool IsActive { get; set; }
        public int TotalAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public string WorkingDays { get; set; } = "";
    }
}

