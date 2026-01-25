using Hairdresser.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hairdresser.Api.Controllers;

[Authorize]
public class DashboardController(IDashboardService dashboardService,IWorkerService workerService) : Controller
{
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

        var calendarMonth = month ?? selectedDate.Month;
        var calendarYear = year ?? selectedDate.Year;

        var viewModel = await dashboardService.GetDashboardDataAsync(selectedDate, workerId, calendarMonth, calendarYear);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status, string? date, int? workerId)
    {
        var success = await dashboardService.UpdateAppointmentStatusAsync(id, status);
            
        if (!success)
            return NotFound();

        return RedirectToAction(nameof(Index), new { date, workerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNotes(int id, string notes, string? date, int? workerId)
    {
        var success = await dashboardService.UpdateAppointmentNotesAsync(id, notes);
            
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
        var workers = await workerService.GetAllWorkersAsync();
        ViewData["Workers"] = workers;
        var appointments = await dashboardService.GetDayAppointmentsAsync(selectedDate, workerId);

        return PartialView("_DayAppointments", appointments);
    }
}