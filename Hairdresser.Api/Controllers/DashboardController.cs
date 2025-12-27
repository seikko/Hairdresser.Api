using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hairdresser.Api.Services;

namespace BookingAPI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index(string? date, int? workerId, int? month, int? year)
        {
            DateOnly selectedDate;
            if (!string.IsNullOrEmpty(date) && DateOnly.TryParse(date, out var parsedDate))
            {
                selectedDate = parsedDate;
            }
            else
            {
                selectedDate = DateOnly.FromDateTime(DateTime.Today);
            }

            int calendarMonth = month ?? selectedDate.Month;
            int calendarYear = year ?? selectedDate.Year;

            var viewModel = await _dashboardService.GetDashboardDataAsync(selectedDate, workerId, calendarMonth, calendarYear);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? date, int? workerId)
        {
            var success = await _dashboardService.UpdateAppointmentStatusAsync(id, status);
            
            if (!success)
                return NotFound();

            return RedirectToAction(nameof(Index), new { date, workerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotes(int id, string notes, string? date, int? workerId)
        {
            var success = await _dashboardService.UpdateAppointmentNotesAsync(id, notes);
            
            if (!success)
                return NotFound();

            return RedirectToAction(nameof(Index), new { date, workerId });
        }

        [HttpGet]
        public async Task<IActionResult> GetDayAppointmentsPartial(string date, int? workerId)
        {
            if (!DateOnly.TryParse(date, out var selectedDate))
            {
                return BadRequest();
            }

            ViewData["SelectedDate"] = selectedDate.ToString("yyyy-MM-dd");
            ViewData["SelectedWorkerId"] = workerId;

            var appointments = await _dashboardService.GetDayAppointmentsAsync(selectedDate, workerId);

            return PartialView("_DayAppointments", appointments);
        }
    }
}
