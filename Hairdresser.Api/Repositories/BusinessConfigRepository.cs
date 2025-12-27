using Hairdresser.Api.Data;
using Hairdresser.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Hairdresser.Api.Repositories
{
    public class BusinessConfigRepository : Repository<BusinessConfig>, IBusinessConfigRepository
    {
        public BusinessConfigRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<BusinessConfig?> GetByKeyAsync(string key)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.ConfigKey == key);
        }
    }
}

