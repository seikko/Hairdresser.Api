using System.Globalization;
using Hairdresser.Api.Models.ViewModels;
using Hairdresser.Api.Repositories;

namespace Hairdresser.Api.Services;

public class DashboardService(IUnitOfWork unitOfWork, ILogger<DashboardService> logger) : IDashboardService
{
    public async Task<DashboardViewModel> GetDashboardDataAsync(DateOnly selectedDate, int? workerId, int calendarMonth, int calendarYear)
    {
        var appointments = await GetDayAppointmentsAsync(selectedDate, workerId);

        var workers = await unitOfWork.Workers.GetActiveWorkersAsync();
        var workerFilters = workers.Select(w => new WorkerFilterViewModel
        {
            Id = w.Id,
            Name = w.Name
        }).ToList();

        var calendarDays = await BuildCalendarDaysAsync(calendarYear, calendarMonth, selectedDate, workerId);

        var culture = new CultureInfo("tr-TR");
        var monthName = culture.DateTimeFormat.GetMonthName(calendarMonth);

        var firstDayOfMonth = new DateOnly(calendarYear, calendarMonth, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        var monthlyAppointments = await unitOfWork.Appointments.GetByDateRangeAsync(firstDayOfMonth, lastDayOfMonth);
            
        var filteredMonthly = workerId.HasValue && workerId.Value > 0
            ? monthlyAppointments.Where(a => a.WorkerId == workerId.Value)
            : monthlyAppointments;

        var stats = filteredMonthly
            .GroupBy(a => a.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var totalMonthlyAppointments = stats.Values.Sum();
        var confirmedMonthly = stats.GetValueOrDefault("confirmed", 0);
        var pendingMonthly = stats.GetValueOrDefault("pending", 0);
        var cancelledMonthly = stats.GetValueOrDefault("cancelled", 0);
        var completedMonthly = stats.GetValueOrDefault("completed", 0);

        return new DashboardViewModel
        {
            SelectedDate = selectedDate,
            Appointments = appointments.ToList(),
            TotalAppointments = totalMonthlyAppointments,
            ConfirmedAppointments = confirmedMonthly + completedMonthly,
            PendingAppointments = pendingMonthly,
            CancelledAppointments = cancelledMonthly,
            CalendarDays = calendarDays,
            CurrentMonth = calendarMonth,
            CurrentYear = calendarYear,
            MonthName = monthName,
            Workers = workerFilters,
            SelectedWorkerId = workerId
        };
    }

    public async Task<IEnumerable<AppointmentViewModel>> GetDayAppointmentsAsync(
        DateOnly date,
        int? workerId)
    {
        var appointments = await unitOfWork.Appointments
            .GetByDateRangeAsync(date, date);

        var filtered = workerId.HasValue && workerId.Value > 0
            ? appointments.Where(a => a.WorkerId == workerId.Value).ToList()
            : appointments.ToList();

        // 👉 ServiceId'leri al
        var serviceIds = filtered
            .Where(a => a.ServiceId != null)
            .Select(a => a.ServiceId!.Value)
            .Distinct()
            .ToList();

        // 👉 İlgili servisleri tek seferde çek
        var services = await unitOfWork.WorkerService
            .FindAsync(ws => serviceIds.Contains(ws.Id));

        return filtered
            .OrderBy(a => a.AppointmentTime)
            .Select(a =>
            {
                var service = services.FirstOrDefault(s => s.Id == a.ServiceId);

                return new AppointmentViewModel
                {
                    Id = a.Id,
                    Time = a.AppointmentTime.ToString("HH:mm"),
                    CustomerName = a.User?.Name ?? "Misafir",
                    PhoneNumber = a.User?.PhoneNumber ?? "",
                    Status = a.Status,
                    ServiceType = service?.ServiceName,
                    Notes = a.Notes,
                    DurationMinutes = a.DurationMinutes,
                    WorkerId = a.WorkerId,
                    WorkerName = a.Worker?.Name ?? "Atanmamış",
                    Price = service?.Price??0
                };
            })
            .ToList();
    }


    public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string status)
    {
        try
        {
            var appointment = await unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return false;

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.UtcNow;
                
            unitOfWork.Appointments.Update(appointment);
            await unitOfWork.SaveChangesAsync();
                
            logger.LogInformation("Updated appointment {AppointmentId} status to {Status}", appointmentId, status);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update appointment status {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task<bool> UpdateAppointmentNotesAsync(int appointmentId, string notes)
    {
        try
        {
            var appointment = await unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return false;

            appointment.Notes = notes;
            appointment.UpdatedAt = DateTime.UtcNow;
                
            unitOfWork.Appointments.Update(appointment);
            await unitOfWork.SaveChangesAsync();
                
            logger.LogInformation("Updated appointment {AppointmentId} notes", appointmentId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update appointment notes {AppointmentId}", appointmentId);
            throw;
        }
    }

    private async Task<List<CalendarDayViewModel>> BuildCalendarDaysAsync(int year, int month, DateOnly selectedDate, int? workerId)
    {
        var days = new List<CalendarDayViewModel>();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var firstDayOfMonth = new DateOnly(year, month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        var startDate = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek + 1);
        var endDate = lastDayOfMonth.AddDays(7 - (int)lastDayOfMonth.DayOfWeek);

        var appointments = await unitOfWork.Appointments.GetByDateRangeAsync(startDate, endDate);
        var filtered = appointments.Where(a => a.Status != "cancelled");

        if (workerId.HasValue && workerId.Value > 0)
        {
            filtered = filtered.Where(a => a.WorkerId == workerId.Value);
        }

        var appointmentCounts = filtered
            .GroupBy(a => a.AppointmentDate)
            .ToDictionary(g => g.Key, g => g.Count());

        var currentDate = startDate;
        if ((int)firstDayOfMonth.DayOfWeek == 0)
        {
            currentDate = firstDayOfMonth.AddDays(-6);
        }
        else
        {
            currentDate = firstDayOfMonth.AddDays(-((int)firstDayOfMonth.DayOfWeek - 1));
        }

        for (int i = 0; i < 42; i++)
        {
            days.Add(new CalendarDayViewModel
            {
                Date = currentDate,
                Day = currentDate.Day,
                IsCurrentMonth = currentDate.Month == month,
                IsToday = currentDate == today,
                IsSelected = currentDate == selectedDate,
                AppointmentCount = appointmentCounts.GetValueOrDefault(currentDate, 0)
            });
            currentDate = currentDate.AddDays(1);
        }

        return days;
    }
}