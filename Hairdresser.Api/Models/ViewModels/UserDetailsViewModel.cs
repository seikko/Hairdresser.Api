namespace Hairdresser.Api.Models.ViewModels;

public class UserDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<AppointmentViewModel> Appointments { get; set; } = new();
    public WorkerServiceEntity Service { get; set; } = new();
}