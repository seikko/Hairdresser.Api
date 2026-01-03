using Hairdresser.Api.Models;
using Hairdresser.Api.Repositories;

namespace Hairdresser.Api.Services;

public class BookingService(IUnitOfWork unitOfWork, ILogger<BookingService> logger) : IBookingService
{
    private static TimeZoneInfo? _turkeyTimeZone;

    private DateTime GetTurkeyTime()
    {
        if (_turkeyTimeZone == null)
        {
            try
            {
                _turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
            }
            catch
            {
                try
                {
                    _turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                }
                catch
                {
                    logger.LogWarning("Turkey timezone not found on this system. Using fixed UTC+3 offset.");
                    return DateTime.UtcNow.AddHours(3);
                }
            }
        }

        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _turkeyTimeZone);
    }

    public async Task<User> GetOrCreateUserAsync(string phoneNumber, string? name)
    {
        var user = await unitOfWork.Users.GetByPhoneNumberAsync(phoneNumber);

        if (user == null)
        {
            user = new User
            {
                PhoneNumber = phoneNumber,
                Name = name,
                CreatedAt = DateTime.UtcNow,
                LastContact = DateTime.UtcNow
            };

            await unitOfWork.Users.AddAsync(user);
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Created new user: {PhoneNumber}", phoneNumber);
        }
        else
        {
            user.LastContact = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(user.Name))
            {
                user.Name = name;
            }

            await unitOfWork.SaveChangesAsync();
        }

        return user;
    }

    public async Task<List<Worker>> GetActiveWorkersAsync()
    {
        var workers = await unitOfWork.Workers.GetActiveWorkersAsync();
        return workers.ToList();
    }

    public async Task<Worker?> GetWorkerByIdAsync(int workerId)
    {
        var worker = await unitOfWork.Workers.GetWorkerWithSchedulesAsync(workerId);
        return worker?.IsActive == true ? worker : null;
    }

    public async Task<List<TimeOnly>> GetAvailableTimeSlotsForWorkerAsync(int workerId, DateOnly date)
    {
        int dayOfWeek = (int)date.DayOfWeek;

        var workerSchedule = await unitOfWork.WorkerSchedules.GetByWorkerAndDayAsync(workerId, dayOfWeek);

        if (workerSchedule == null)
        {
            logger.LogInformation(
                "Worker {WorkerId} is not working on {DayOfWeek}",
                workerId,
                date.DayOfWeek);

            return new List<TimeOnly>();
        }

        var slotDurationConfig = await unitOfWork.BusinessConfigs.GetByKeyAsync("slot_duration_minutes");

        int slotDuration = int.TryParse(slotDurationConfig?.ConfigValue, out var parsed)
            ? parsed
            : 60;

        var allSlots = new List<TimeOnly>();
        var currentTime = workerSchedule.StartTime;
        var endTime = workerSchedule.EndTime;

        while (currentTime < endTime)
        {
            allSlots.Add(currentTime);
            currentTime = currentTime.AddMinutes(slotDuration);
        }

        var bookedAppointments = await unitOfWork.Appointments.GetByWorkerAndDateAsync(workerId, date);
        var bookedTimes = bookedAppointments.Select(a => a.AppointmentTime).ToList();

        var availableSlots = allSlots
            .Where(slot => !bookedTimes.Contains(slot))
            .ToList();

        var nowInTurkey = GetTurkeyTime();
        var todayInTurkey = DateOnly.FromDateTime(nowInTurkey);

        if (date == todayInTurkey)
        {
            var currentTimeInTurkey = TimeOnly.FromDateTime(nowInTurkey);

            int remainder = currentTimeInTurkey.Minute % slotDuration;
            if (remainder != 0)
            {
                currentTimeInTurkey =
                    currentTimeInTurkey.AddMinutes(slotDuration - remainder);
            }

            availableSlots = availableSlots
                .Where(slot => slot >= currentTimeInTurkey)
                .ToList();

            logger.LogInformation(
                "Filtering past slots. WorkerId={WorkerId}, Date={Date}, Now={Now}, AvailableCount={Count}",
                workerId,
                date,
                currentTimeInTurkey,
                availableSlots.Count);
        }

        return availableSlots
            .OrderBy(t => t)
            .ToList();
    }

    public async Task<Appointment?> CreateAppointmentAsync(int userId, int workerId, DateOnly date, TimeOnly time,
        string? serviceType)
    {
        try
        {
            var nowInTurkey = GetTurkeyTime();
            var todayInTurkey = DateOnly.FromDateTime(nowInTurkey);
            var currentTimeInTurkey = TimeOnly.FromDateTime(nowInTurkey);

            if (date < todayInTurkey || (date == todayInTurkey && time <= currentTimeInTurkey))
            {
                logger.LogWarning(
                    "Rejected past appointment. UserId={UserId} WorkerId={WorkerId} RequestedDate={Date} RequestedTime={Time} TurkeyNow={Now}",
                    userId, workerId, date, time, nowInTurkey);
                return null;
            }

            var existingAppointment = await unitOfWork.Appointments.GetByWorkerDateAndTimeAsync(workerId, date, time);

            if (existingAppointment != null)
            {
                logger.LogWarning("Time slot already booked for worker {WorkerId}: {Date} {Time}", workerId, date,
                    time);
                return null;
            }

            var appointment = new Appointment
            {
                UserId = userId,
                WorkerId = workerId,
                AppointmentDate = date,
                AppointmentTime = time,
                ServiceType = serviceType,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await unitOfWork.Appointments.AddAsync(appointment);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation(
                "Created appointment for user {UserId} with worker {WorkerId} on {Date} at {Time}", userId,
                workerId, date, time);
            return appointment;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create appointment for user {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> CancelAppointmentAsync(int userId, int appointmentId)
    {
        try
        {
            var appointment = await unitOfWork.Appointments.FirstOrDefaultAsync(
                a => a.Id == appointmentId && a.UserId == userId);

            if (appointment == null)
            {
                logger.LogWarning("Appointment not found: {AppointmentId} for user {UserId}", appointmentId,
                    userId);
                return false;
            }

            appointment.Status = "cancelled";
            appointment.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Cancelled appointment {AppointmentId} for user {UserId}", appointmentId,
                userId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to cancel appointment {AppointmentId}", appointmentId);
            return false;
        }
    }

    public async Task<List<Appointment>> GetUserAppointmentsAsync(int userId)
    {
        var appointments = await unitOfWork.Appointments.GetByUserIdForCancelAsync(userId);
        return appointments.ToList();
    }
}