using Hairdresser.Api.Data;
using Hairdresser.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Hairdresser.Api.Repositories;

public class UserRepository(ApplicationDbContext context) : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
    }

    public async Task<IEnumerable<User>> GetAllUsersWithAppointmentsAsync()
    {
        var query = _context.Users
            .Include(u => u.Appointments)
            .ThenInclude(a => a.Service)
            .Include(u => u.Appointments);
        return await query.ToListAsync();
    }
    }
 