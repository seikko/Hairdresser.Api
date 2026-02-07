using Hairdresser.Api.Models.ViewModels;
using Hairdresser.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hairdresser.Api.Controllers;

public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly IAppointmentService _appointmentService;

    public UserController(IUserService userService, IAppointmentService appointmentService)
    {
        _userService = userService;
        _appointmentService = appointmentService;
    }

    // Kullanıcı listesi
    public async Task<IActionResult> Index()
    {
        var model = await _userService.GetAllUsersAsync();
        return View(model); // View modeli List<UserListModel>
    }

    // Kullanıcı detayları ve randevuları
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        var appointments = await _appointmentService.GetAppointmentByUserIdAsync(id);

        var model = new UserDetailsViewModel
        {
            Id = user.Id,
            Name = user.Name ?? "",
            PhoneNumber = user.PhoneNumber,
            Appointments = appointments.Select(a => new AppointmentViewModel
            {
                Id = a.Id,
                Date = a.AppointmentDate.ToString("yyyy-MM-dd"), // Tarih
                Time = a.AppointmentTime.ToString("HH:mm"),      // Saat
                CustomerName = user.Name ?? "",
                PhoneNumber = user.PhoneNumber,
                Status = a.Status,
                ServiceType = a.Service?.ServiceName ?? "-",     // null-check
                Notes = a.Notes,
                DurationMinutes = a.DurationMinutes,
                Price = a.Service?.Price ?? 0,                   // null-check
                WorkerId = a.WorkerId,
                WorkerName = a.Worker?.Name ?? "-",             // null-check
            }).ToList()
        };

        return View(model);
    }
}
