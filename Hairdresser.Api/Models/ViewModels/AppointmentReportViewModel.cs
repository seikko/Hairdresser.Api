namespace Hairdresser.Api.Models.ViewModels;

public class AppointmentReportViewModel
{
    // Filtreler
    public int? WorkerId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    // Dropdown data
    public List<WorkerDropdownViewModel> Workers { get; set; } = [];

    // Sonuçlar
    public List<AppointmentReportItemViewModel> Appointments { get; set; } = [];

    // Özet
    public int TotalAppointments { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class WorkerDropdownViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class AppointmentReportItemViewModel
{
    public int AppointmentId { get; set; }
    public string WorkerName { get; set; }
    public string CustomerName { get; set; }
    public string ServiceName { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }

    public decimal Price { get; set; }
    public string Status { get; set; }
}
