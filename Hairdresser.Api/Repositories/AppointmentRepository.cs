using Hairdresser.Api.Data;
using Hairdresser.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Hairdresser.Api.Repositories;

public class AppointmentRepository(ApplicationDbContext context)
    : Repository<Appointment>(context), IAppointmentRepository
{
    public async Task<IEnumerable<Appointment>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(a => a.Worker)
            .Where(a => a.UserId == userId && a.Status != "cancelled")
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByWorkerAndDateAsync(int workerId, DateOnly date)
    {
        return await _dbSet
            .Where(a => a.WorkerId == workerId && a.AppointmentDate == date && a.Status != "cancelled")
            .Select(a => a)
            .ToListAsync();
    }

    public async Task<Appointment?> GetByWorkerDateAndTimeAsync(int workerId, DateOnly date, TimeOnly time)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.WorkerId == workerId 
                                      && a.AppointmentDate == date 
                                      && a.AppointmentTime == time 
                                      && a.Status != "cancelled");
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.Worker)
            .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<Appointment?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.Worker)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}