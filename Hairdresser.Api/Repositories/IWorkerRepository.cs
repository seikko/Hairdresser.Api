using Hairdresser.Api.Models;

namespace Hairdresser.Api.Repositories
{
    public interface IWorkerRepository : IRepository<Worker>
    {
        Task<IEnumerable<Worker>> GetActiveWorkersAsync();
        Task<Worker?> GetWorkerWithSchedulesAsync(int id);
    }
}

