using Hairdresser.Api.Models;
using Hairdresser.Api.Models.ViewModels;

namespace Hairdresser.Api.Services;

public interface IAppointmentService
{
    Task<AppointmentIndexViewModel> GetAppointmentsForIndexAsync(DateOnly selectedDate, int? workerId, string? status, string? search);
    Task<Appointment?> GetAppointmentByIdAsync(int id);
    Task<List<Appointment>?> GetAppointmentByUserIdAsync(int userId);
    Task<Appointment?> GetAppointmentWithDetailsAsync(int id);
    Task<Appointment> CreateAppointmentFromAdminAsync(string phoneNumber, string? customerName, int workerId, 
        DateOnly date, TimeOnly time, int durationMinutes, string status, string? serviceType, string? notes,int selectedServiceId);
    Task<bool> UpdateAppointmentAsync(int id, int workerId, DateOnly date, TimeOnly time, 
        int durationMinutes, string status, string? serviceType, string? notes,int selectedServiceId);
    Task<bool> DeleteAppointmentAsync(int id);
    Task<bool> HasSlotConflictAsync(int workerId, DateOnly date, TimeOnly time, int? excludeAppointmentId = null);

    Task<List<Appointment>> GetAppointmentsForReportAsync(
        int workerId,
        DateOnly startDate,
        DateOnly endDate);
}