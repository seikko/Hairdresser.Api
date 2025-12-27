using Hairdresser.Api.Models.ViewModels;

namespace Hairdresser.Api.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(DateOnly selectedDate, int? workerId, int calendarMonth, int calendarYear);
        Task<IEnumerable<AppointmentViewModel>> GetDayAppointmentsAsync(DateOnly date, int? workerId);
        Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string status);
        Task<bool> UpdateAppointmentNotesAsync(int appointmentId, string notes);
    }
}

