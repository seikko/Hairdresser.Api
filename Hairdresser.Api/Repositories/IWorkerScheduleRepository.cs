using Hairdresser.Api.Models;

namespace Hairdresser.Api.Repositories;

public interface IWorkerScheduleRepository : IRepository<WorkerSchedule>
{
    Task<WorkerSchedule?> GetByWorkerAndDayAsync(int workerId, int dayOfWeek);
    Task<IEnumerable<WorkerSchedule>> GetByWorkerIdAsync(int workerId);
}