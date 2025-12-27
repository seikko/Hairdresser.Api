using Hairdresser.Api.Models;

namespace Hairdresser.Api.Repositories
{
    public interface IBusinessConfigRepository : IRepository<BusinessConfig>
    {
        Task<BusinessConfig?> GetByKeyAsync(string key);
    }
}

