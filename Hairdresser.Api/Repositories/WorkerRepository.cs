using Hairdresser.Api.Data;
using Hairdresser.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Hairdresser.Api.Repositories;

public class WorkerRepository(ApplicationDbContext context) : Repository<Worker>(context), IWorkerRepository
{
    public async Task<IEnumerable<Worker>> GetActiveWorkersAsync()
    {
        return await _dbSet
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<Worker?> GetWorkerWithSchedulesAsync(int id)
    {
        return await _dbSet
            .Include(w => w.Schedules)
            .FirstOrDefaultAsync(w => w.Id == id);
    }
}