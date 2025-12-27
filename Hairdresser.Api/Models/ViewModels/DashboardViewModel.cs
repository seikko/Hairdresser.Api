namespace Hairdresser.Api.Models.ViewModels
{
    public class DashboardViewModel
    {
        public DateOnly SelectedDate { get; set; }
        public List<AppointmentViewModel> Appointments { get; set; } = new();
        public int TotalAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CancelledAppointments { get; set; }

        public List<CalendarDayViewModel> CalendarDays { get; set; } = new();
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public string MonthName { get; set; } = string.Empty;

        public List<WorkerFilterViewModel> Workers { get; set; } = new();
        public int? SelectedWorkerId { get; set; }
    }

    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public string Time { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ServiceType { get; set; }
        public string? Notes { get; set; }
        public int DurationMinutes { get; set; }

        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;

        public string StatusBadgeClass => Status switch
        {
            "confirmed" => "badge bg-success",
            "pending" => "badge bg-warning",
            "cancelled" => "badge bg-danger",
            "completed" => "badge bg-info",
            _ => "badge bg-secondary"
        };

        public string StatusText => Status switch
        {
            "confirmed" => "Onaylandı",
            "pending" => "Bekliyor",
            "cancelled" => "İptal",
            "completed" => "Tamamlandı",
            _ => Status
        };
    }

    public class CalendarDayViewModel
    {
        public DateOnly Date { get; set; }
        public int Day { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public bool IsSelected { get; set; }
        public int AppointmentCount { get; set; }
    }

    public class WorkerFilterViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

