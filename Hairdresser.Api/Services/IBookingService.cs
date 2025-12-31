using Hairdresser.Api.Models;

namespace Hairdresser.Api.Services;

public interface IBookingService
{
    Task<User> GetOrCreateUserAsync(string phoneNumber, string? name);

    Task<List<Worker>> GetActiveWorkersAsync();
    Task<Worker?> GetWorkerByIdAsync(int workerId);

    Task<List<TimeOnly>> GetAvailableTimeSlotsForWorkerAsync(int workerId, DateOnly date);

    Task<Appointment?> CreateAppointmentAsync(int userId, int workerId, DateOnly date, TimeOnly time, string? serviceType);
    Task<bool> CancelAppointmentAsync(int userId, int appointmentId);
    Task<List<Appointment>> GetUserAppointmentsAsync(int userId);
}