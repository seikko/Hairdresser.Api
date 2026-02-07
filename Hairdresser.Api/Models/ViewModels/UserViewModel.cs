namespace Hairdresser.Api.Models.ViewModels;

public class UserViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    
    public int ConfirmedAppointments => Appointments.Count(a => a.Status == "confirmed" || a.Status == "completed");
    public int CancelledAppointments => Appointments.Count(a => a.Status == "cancelled");
    
    public List<AppointmentViewModel> Appointments { get; set; } = new();
}