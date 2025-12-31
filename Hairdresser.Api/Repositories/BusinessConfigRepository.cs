using Hairdresser.Api.Data;
using Hairdresser.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Hairdresser.Api.Repositories;

public class BusinessConfigRepository(ApplicationDbContext context)
    : Repository<BusinessConfig>(context), IBusinessConfigRepository
{
    public async Task<BusinessConfig?> GetByKeyAsync(string key)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.ConfigKey == key);
    }
}