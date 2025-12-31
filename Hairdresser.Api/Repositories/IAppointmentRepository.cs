using Hairdresser.Api.Models;

namespace Hairdresser.Api.Repositories;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Appointment>> GetByWorkerAndDateAsync(int workerId, DateOnly date);
    Task<Appointment?> GetByWorkerDateAndTimeAsync(int workerId, DateOnly date, TimeOnly time);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    Task<Appointment?> GetByIdWithDetailsAsync(int id);
}