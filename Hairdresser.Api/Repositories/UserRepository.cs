using Hairdresser.Api.Data;
using Hairdresser.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Hairdresser.Api.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }
    }
}

