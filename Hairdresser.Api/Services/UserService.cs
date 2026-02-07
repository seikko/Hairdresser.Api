using Hairdresser.Api.Models;
using Hairdresser.Api.Models.ViewModels;
using Hairdresser.Api.Repositories;

namespace Hairdresser.Api.Services;

public class UserService(IUnitOfWork unitOfWork):IUserService
{
    public async Task<List<UserListModel>> GetAllUsersAsync()
    {
        // Tüm kullanıcıları Appointments + Service + Worker ile birlikte çek
        var users = await unitOfWork.Users.GetAllUsersWithAppointmentsAsync();

        // Telefon numarasına göre duplicate yok
        var distinctUsers = users
            .GroupBy(u => u.PhoneNumber)
            .Select(g => g.First())
            .ToList();

        var result = new List<UserListModel>();

        foreach (var user in distinctUsers)
        {
            // ✅ Artık user.Appointments üzerinden count alıyoruz
            var confirmCount = user.Appointments?.Count(a => a.Status == "confirmed" || a.Status == "completed") ?? 0;
            var cancelCount = user.Appointments?.Count(a => a.Status == "cancelled") ?? 0;

            result.Add(new UserListModel
            {
                User = user,
                ConfirmCount = confirmCount,
                CancelCount = cancelCount
            });
        }

        return result;
    }


    public async Task<User> GetUserByIdAsync(int id)
    {
      return    await unitOfWork.Users.GetByIdAsync(id);

    }
}