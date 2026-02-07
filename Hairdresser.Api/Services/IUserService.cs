using Hairdresser.Api.Models;
using Hairdresser.Api.Models.ViewModels;

namespace Hairdresser.Api.Services;

public interface IUserService
{
    Task<List<UserListModel>> GetAllUsersAsync();
    Task<User> GetUserByIdAsync(int id);
}