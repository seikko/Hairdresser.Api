using System.ComponentModel.DataAnnotations;

namespace Hairdresser.Api.Models.ViewModels;

public class AppointmentCreateViewModel
{
    [Required(ErrorMessage = "Telefon zorunludur")]
    [StringLength(20)]
    [Display(Name = "Telefon")]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Müşteri Adı")]
    public string? CustomerName { get; set; }

    [Required(ErrorMessage = "Çalışan seçiniz")]
    [Display(Name = "Çalışan")]
    public int WorkerId { get; set; }

    [Required(ErrorMessage = "Tarih zorunludur")]
    [Display(Name = "Tarih")]
    public string AppointmentDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Saat zorunludur")]
    [Display(Name = "Saat")]
    public string AppointmentTime { get; set; } = string.Empty;

    [Range(5, 600, ErrorMessage = "Süre 5-600 dakika aralığında olmalıdır")]
    [Display(Name = "Süre (dk)")]
    public int DurationMinutes { get; set; } = 60;

    [Required]
    [StringLength(20)]
    [Display(Name = "Durum")]
    public string Status { get; set; } = "pending";

    [StringLength(100)]
    [Display(Name = "Hizmet")]
    public string? ServiceType { get; set; }

    [Display(Name = "Notlar")]
    public string? Notes { get; set; }
}

public class AppointmentEditViewModel
{
    public int Id { get; set; }

    [Display(Name = "Müşteri")]
    public string CustomerDisplay { get; set; } = string.Empty;

    [Required(ErrorMessage = "Çalışan seçiniz")]
    [Display(Name = "Çalışan")]
    public int WorkerId { get; set; }

    [Required(ErrorMessage = "Tarih zorunludur")]
    [Display(Name = "Tarih")]
    public string AppointmentDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Saat zorunludur")]
    [Display(Name = "Saat")]
    public string AppointmentTime { get; set; } = string.Empty;

    [Range(5, 600, ErrorMessage = "Süre 5-600 dakika aralığında olmalıdır")]
    [Display(Name = "Süre (dk)")]
    public int DurationMinutes { get; set; } = 60;

    [Required]
    [StringLength(20)]
    [Display(Name = "Durum")]
    public string Status { get; set; } = "pending";

    [StringLength(100)]
    [Display(Name = "Hizmet")]
    public string? ServiceType { get; set; }

    [Display(Name = "Notlar")]
    public string? Notes { get; set; }
}

public class AppointmentDeleteViewModel
{
    public int Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string WorkerName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public string? ServiceType { get; set; }
}

public class AppointmentIndexViewModel
{
    public string SelectedDate { get; set; } = string.Empty;
    public int? SelectedWorkerId { get; set; }
    public string? SelectedStatus { get; set; }
    public string? Search { get; set; }

    public List<WorkerFilterViewModel> Workers { get; set; } = new();
    public List<AppointmentRowViewModel> Appointments { get; set; } = new();
}

public class AppointmentRowViewModel
{
    public int Id { get; set; }
    public string Time { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string WorkerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public string StatusBadgeClass { get; set; } = "badge bg-secondary";
    public int DurationMinutes { get; set; }
    public string? ServiceType { get; set; }
}