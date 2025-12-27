using Hairdresser.Api.Models;

namespace Hairdresser.Api.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    }
}

