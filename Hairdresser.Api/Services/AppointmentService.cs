using Hairdresser.Api.Models;
using Hairdresser.Api.Models.ViewModels;
using Hairdresser.Api.Repositories;

namespace Hairdresser.Api.Services;

public class AppointmentService(IUnitOfWork unitOfWork, ILogger<AppointmentService> logger) : IAppointmentService
{
    public async Task<AppointmentIndexViewModel> GetAppointmentsForIndexAsync(
        DateOnly selectedDate, 
        int? workerId,
        string? status, 
        string? search)
    {
        var allAppointments = await unitOfWork.Appointments.GetByDateRangeAsync(selectedDate, selectedDate);

        var filtered = allAppointments.AsEnumerable();

        if (workerId is > 0)
        {
            filtered = filtered.Where(a => a.WorkerId == workerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filtered = filtered.Where(a => a.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            filtered = filtered.Where(a =>
                (a.User?.Name != null && a.User.Name.ToLower().Contains(searchLower)) ||
                (a.User?.PhoneNumber?.Contains(search.Trim()) == true) ||
                (a.ServiceType != null && a.ServiceType.ToLower().Contains(searchLower)));
        }

        var appointmentRows = filtered
            .OrderBy(a => a.AppointmentTime)
            .Select(a => new AppointmentRowViewModel
            {
                Id = a.Id,
                Time = a.AppointmentTime.ToString("HH:mm"),
                CustomerName = a.User?.Name ?? "Misafir",
                PhoneNumber = a.User?.PhoneNumber ?? "",
                WorkerName = a.Worker?.Name ?? "Atanmamış",
                Status = a.Status,
                StatusText = GetStatusText(a.Status),
                StatusBadgeClass = GetStatusBadgeClass(a.Status),
                DurationMinutes = a.DurationMinutes,
                ServiceType = a.ServiceType
            })
            .ToList();

        var activeWorkers = await unitOfWork.Workers.GetActiveWorkersAsync();
        var workerFilters = activeWorkers.Select(w => new WorkerFilterViewModel
        {
            Id = w.Id,
            Name = w.Name
        }).ToList();

        return new AppointmentIndexViewModel
        {
            SelectedDate = selectedDate.ToString("yyyy-MM-dd"),
            SelectedWorkerId = workerId,
            SelectedStatus = status,
            Search = search,
            Workers = workerFilters,
            Appointments = appointmentRows
        };
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int id)
    {
        return await unitOfWork.Appointments.GetByIdAsync(id);
    }

    public async Task<Appointment?> GetAppointmentWithDetailsAsync(int id)
    {
        return await unitOfWork.Appointments.GetByIdWithDetailsAsync(id);
    }

    public async Task<Appointment> CreateAppointmentFromAdminAsync(string phoneNumber, string? customerName,
        int workerId, DateOnly date, TimeOnly time, int durationMinutes, string status, string? serviceType,
        string? notes)
    {
        try
        {
            var normalizedPhone = NormalizePhoneNumber(phoneNumber);

            var user = await unitOfWork.Users.GetByPhoneNumberAsync(normalizedPhone);
            if (user == null)
            {
                user = new User
                {
                    PhoneNumber = normalizedPhone,
                    Name = string.IsNullOrWhiteSpace(customerName) ? null : customerName.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    LastContact = DateTime.UtcNow
                };
                await unitOfWork.Users.AddAsync(user);
                await unitOfWork.SaveChangesAsync();
            }
            else if (!string.IsNullOrWhiteSpace(customerName) && string.IsNullOrWhiteSpace(user.Name))
            {
                user.Name = customerName.Trim();
                user.LastContact = DateTime.UtcNow;
                await unitOfWork.SaveChangesAsync();
            }

            var appointment = new Appointment
            {
                UserId = user.Id,
                WorkerId = workerId,
                AppointmentDate = date,
                AppointmentTime = time,
                DurationMinutes = durationMinutes,
                Status = status,
                ServiceType = string.IsNullOrWhiteSpace(serviceType) ? null : serviceType.Trim(),
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await unitOfWork.Appointments.AddAsync(appointment);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Created appointment from admin: {AppointmentId}", appointment.Id);
            return appointment;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create appointment from admin");
            throw;
        }
    }

    public async Task<bool> UpdateAppointmentAsync(int id, int workerId, DateOnly date, TimeOnly time,
        int durationMinutes, string status, string? serviceType, string? notes)
    {
        try
        {
            var appointment = await unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null)
                return false;

            appointment.WorkerId = workerId;
            appointment.AppointmentDate = date;
            appointment.AppointmentTime = time;
            appointment.DurationMinutes = durationMinutes;
            appointment.Status = status;
            appointment.ServiceType = string.IsNullOrWhiteSpace(serviceType) ? null : serviceType.Trim();
            appointment.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            appointment.UpdatedAt = DateTime.UtcNow;

            unitOfWork.Appointments.Update(appointment);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Updated appointment: {AppointmentId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update appointment {AppointmentId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAppointmentAsync(int id)
    {
        try
        {
            var appointment = await unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null)
                return false;

            unitOfWork.Appointments.Remove(appointment);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Deleted appointment: {AppointmentId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete appointment {AppointmentId}", id);
            throw;
        }
    }

    public async Task<bool> HasSlotConflictAsync(int workerId, DateOnly date, TimeOnly time,
        int? excludeAppointmentId = null)
    {
        var appointments = await unitOfWork.Appointments.GetByWorkerAndDateAsync(workerId, date);

        var conflict = appointments.Any(a =>
            a.AppointmentTime == time &&
            a.Status != "cancelled" &&
            (!excludeAppointmentId.HasValue || a.Id != excludeAppointmentId.Value));

        return conflict;
    }

    private static string NormalizePhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Trim();
    }

    private static string GetStatusText(string status) => status switch
    {
        "confirmed" => "Onaylandı",
        "pending" => "Bekliyor",
        "cancelled" => "İptal",
        "completed" => "Tamamlandı",
        _ => status
    };

    private static string GetStatusBadgeClass(string status) => status switch
    {
        "confirmed" => "badge bg-success",
        "pending" => "badge bg-warning",
        "cancelled" => "badge bg-danger",
        "completed" => "badge bg-info",
        _ => "badge bg-secondary"
    };
}