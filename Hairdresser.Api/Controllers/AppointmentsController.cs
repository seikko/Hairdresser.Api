using System.Globalization;
using Hairdresser.Api.Models;
using Hairdresser.Api.Models.ViewModels;
using Hairdresser.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingAPI.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IWorkerService _workerService;

        public AppointmentsController(IAppointmentService appointmentService, IWorkerService workerService)
        {
            _appointmentService = appointmentService;
            _workerService = workerService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? date, int? workerId, string? status, string? search)
        {
            var selectedDate = ParseDateOrDefault(date, DateOnly.FromDateTime(DateTime.Today));
            var viewModel = await _appointmentService.GetAppointmentsForIndexAsync(selectedDate, workerId, status, search);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string? date, int? workerId, string? time)
        {
            var selectedDate = ParseDateOrDefault(date, DateOnly.FromDateTime(DateTime.Today));
            var selectedTime = ParseTimeOrDefault(time, new TimeOnly(9, 0));

            await PopulateWorkersAsync();

            var vm = new AppointmentCreateViewModel
            {
                AppointmentDate = selectedDate.ToString("yyyy-MM-dd"),
                AppointmentTime = selectedTime.ToString("HH:mm"),
                WorkerId = workerId.GetValueOrDefault() > 0 ? workerId!.Value : 0
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model)
        {
            await PopulateWorkersAsync();

            if (!TryParseDateTime(model.AppointmentDate, model.AppointmentTime, out var date, out var time))
            {
                ModelState.AddModelError(string.Empty, "Tarih veya saat formatı geçersiz.");
            }

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                ModelState.AddModelError(nameof(model.PhoneNumber), "Telefon zorunludur.");
            }

            if (model.WorkerId <= 0)
            {
                ModelState.AddModelError(nameof(model.WorkerId), "Çalışan seçiniz.");
            }

            if (!IsValidStatus(model.Status))
            {
                ModelState.AddModelError(nameof(model.Status), "Geçersiz durum.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var workerExists = await _workerService.GetWorkerByIdAsync(model.WorkerId) != null;
            if (!workerExists)
            {
                ModelState.AddModelError(nameof(model.WorkerId), "Çalışan bulunamadı.");
                return View(model);
            }

            var hasConflict = await _appointmentService.HasSlotConflictAsync(model.WorkerId, date, time);
            if (hasConflict)
            {
                ModelState.AddModelError(string.Empty, "Bu çalışan için seçilen tarih/saatte zaten bir randevu var.");
                return View(model);
            }

            var appointment = await _appointmentService.CreateAppointmentFromAdminAsync(
                model.PhoneNumber,
                model.CustomerName,
                model.WorkerId,
                date,
                time,
                model.DurationMinutes,
                model.Status,
                model.ServiceType,
                model.Notes
            );

            return RedirectToAction(nameof(Index), new { date = appointment.AppointmentDate.ToString("yyyy-MM-dd"), workerId = appointment.WorkerId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _appointmentService.GetAppointmentWithDetailsAsync(id);

            if (appointment == null) 
                return NotFound();

            await PopulateWorkersAsync();

            var vm = new AppointmentEditViewModel
            {
                Id = appointment.Id,
                CustomerDisplay = $"{(appointment.User?.Name ?? "Misafir")} (+{appointment.User?.PhoneNumber})",
                WorkerId = appointment.WorkerId,
                AppointmentDate = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                AppointmentTime = appointment.AppointmentTime.ToString("HH:mm"),
                DurationMinutes = appointment.DurationMinutes,
                Status = appointment.Status,
                ServiceType = appointment.ServiceType,
                Notes = appointment.Notes
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppointmentEditViewModel model)
        {
            await PopulateWorkersAsync();

            if (!TryParseDateTime(model.AppointmentDate, model.AppointmentTime, out var date, out var time))
            {
                ModelState.AddModelError(string.Empty, "Tarih veya saat formatı geçersiz.");
            }

            if (!IsValidStatus(model.Status))
            {
                ModelState.AddModelError(nameof(model.Status), "Geçersiz durum.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var hasConflict = await _appointmentService.HasSlotConflictAsync(model.WorkerId, date, time, model.Id);
            if (hasConflict)
            {
                ModelState.AddModelError(string.Empty, "Bu çalışan için seçilen tarih/saatte zaten bir randevu var.");
                return View(model);
            }

            var success = await _appointmentService.UpdateAppointmentAsync(
                model.Id,
                model.WorkerId,
                date,
                time,
                model.DurationMinutes,
                model.Status,
                model.ServiceType,
                model.Notes
            );

            if (!success)
                return NotFound();

            var appointment = await _appointmentService.GetAppointmentByIdAsync(model.Id);
            return RedirectToAction(nameof(Index), new { date = appointment!.AppointmentDate.ToString("yyyy-MM-dd"), workerId = appointment.WorkerId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _appointmentService.GetAppointmentWithDetailsAsync(id);

            if (appointment == null) 
                return NotFound();

            var vm = new AppointmentDeleteViewModel
            {
                Id = appointment.Id,
                Date = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                Time = appointment.AppointmentTime.ToString("HH:mm"),
                WorkerName = appointment.Worker?.Name ?? "Atanmamış",
                CustomerName = appointment.User?.Name ?? "Misafir",
                PhoneNumber = appointment.User?.PhoneNumber ?? "",
                StatusText = GetStatusText(appointment.Status),
                ServiceType = appointment.ServiceType
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null) 
                return NotFound();

            var redirectDate = appointment.AppointmentDate.ToString("yyyy-MM-dd");
            var redirectWorkerId = appointment.WorkerId;

            await _appointmentService.DeleteAppointmentAsync(id);

            return RedirectToAction(nameof(Index), new { date = redirectDate, workerId = redirectWorkerId });
        }

        private async Task PopulateWorkersAsync()
        {
            var workers = await _workerService.GetActiveWorkersAsync();
            ViewBag.Workers = workers.Select(w => new Worker 
            { 
                Id = w.Id, 
                Name = w.Name 
            }).ToList();
        }

        private static bool TryParseDateTime(string date, string time, out DateOnly d, out TimeOnly t)
        {
            var dateOk = DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
            var timeOk = TimeOnly.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out t);
            return dateOk && timeOk;
        }

        private static DateOnly ParseDateOrDefault(string? date, DateOnly fallback)
        {
            if (!string.IsNullOrWhiteSpace(date) &&
                DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            {
                return d;
            }
            return fallback;
        }

        private static TimeOnly ParseTimeOrDefault(string? time, TimeOnly fallback)
        {
            if (!string.IsNullOrWhiteSpace(time) &&
                TimeOnly.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
            {
                return t;
            }
            return fallback;
        }

        private static bool IsValidStatus(string status) =>
            status is "pending" or "confirmed" or "cancelled" or "completed";

        private static string GetStatusText(string status) => status switch
        {
            "confirmed" => "Onaylandı",
            "pending" => "Bekliyor",
            "cancelled" => "İptal",
            "completed" => "Tamamlandı",
            _ => status
        };
    }
}
