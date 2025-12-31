using Hairdresser.Api.Data;
using Hairdresser.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Hairdresser.Api.Repositories;

public class WorkerScheduleRepository(ApplicationDbContext context)
    : Repository<WorkerSchedule>(context), IWorkerScheduleRepository
{
    public async Task<WorkerSchedule?> GetByWorkerAndDayAsync(int workerId, int dayOfWeek)
    {
        return await _dbSet
            .FirstOrDefaultAsync(ws => ws.WorkerId == workerId && ws.DayOfWeek == dayOfWeek && ws.IsWorking);
    }

    public async Task<IEnumerable<WorkerSchedule>> GetByWorkerIdAsync(int workerId)
    {
        return await _dbSet
            .Where(ws => ws.WorkerId == workerId)
            .OrderBy(ws => ws.DayOfWeek)
            .ToListAsync();
    }
}