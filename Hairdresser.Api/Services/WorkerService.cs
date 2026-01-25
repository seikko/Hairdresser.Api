using Hairdresser.Api.Models;
using Hairdresser.Api.Models.ViewModels;
using Hairdresser.Api.Repositories;

namespace Hairdresser.Api.Services;

public class WorkerService(IUnitOfWork unitOfWork, ILogger<WorkerService> logger) : IWorkerService
{
    public async Task<IEnumerable<WorkerListViewModel>> GetAllWorkersAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var workers = await unitOfWork.Workers.GetAllAsync();
            
        var viewModels = new List<WorkerListViewModel>();
            
        foreach (var worker in workers.OrderBy(w => w.Name))
        {
            var schedules = await unitOfWork.WorkerSchedules.GetByWorkerIdAsync(worker.Id);
            var appointments = await unitOfWork.Appointments.FindAsync(a => a.WorkerId == worker.Id);
                
            viewModels.Add(new WorkerListViewModel
            {
                Id = worker.Id,
                Name = worker.Name,
                Specialty = worker.Specialty,
                IsActive = worker.IsActive,
                TotalAppointments = appointments.Count(a => a.Status != "cancelled"),
                TodayAppointments = appointments.Count(a => a.AppointmentDate == today && a.Status != "cancelled"),
                WorkingDays = string.Join(", ", schedules
                    .Where(s => s.IsWorking)
                    .OrderBy(s => s.DayOfWeek)
                    .Select(s => GetShortDayName(s.DayOfWeek)))
            });
        }
            
        return viewModels;
    }
    // <summary>
    public async Task<bool> CreateWorkerServiceEntityAsync(ServiceViewModel workerServiceViewModel)
    {
        try
        {
            var workerServiceEntity = new WorkerServiceEntity
            {
                ServiceName = workerServiceViewModel.ServiceName,
                DurationMinutes = workerServiceViewModel.DurationMinutes,
                Price = workerServiceViewModel.Price
            };

            await unitOfWork.WorkerService.AddAsync(workerServiceEntity);
            await unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create worker service: {ServiceName}", workerServiceViewModel.ServiceName);
            return false;
        }
    }

    public async Task<bool> UpdateWorkerServicesAsync(int workerId, IEnumerable<int> selectedServiceIds)
    {
        try
        {
            // 1️⃣ Mevcut mappingleri al
            var existingMappings = await unitOfWork.WorkerServiceMapping
                .FindAsync(m => m.WorkerId == workerId);

            // 2️⃣ Eski mappingleri sil
            if (existingMappings.Any())
            {
                unitOfWork.WorkerServiceMapping.RemoveRange(existingMappings);
            }

            // 3️⃣ Yeni seçilen hizmetleri mapping tablosuna ekle
            if (selectedServiceIds != null && selectedServiceIds.Any())
            {
                var newMappings = selectedServiceIds.Select(serviceId => new WorkerServiceMapping
                {
                    WorkerId = workerId,
                    ServiceId = serviceId
                });

                await unitOfWork.WorkerServiceMapping.AddRangeAsync(newMappings);
            }

            // 4️⃣ Kaydet
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Updated services for worker {WorkerId}", workerId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update services for worker {WorkerId}", workerId);
            return false;
        }
    }


    /// Tüm hizmetleri listeler (çalışan seçmeden önce)
    /// </summary>
    public async Task<List<WorkerServiceViewModel>> GetAllServicesAsync()
    {
        var services = await unitOfWork.WorkerService.GetAllAsync();

        return services
            .OrderBy(s => s.ServiceName)
            .Select(s => new WorkerServiceViewModel
            {
                Id = s.Id,
                ServiceName = s.ServiceName,
                DurationMinutes = s.DurationMinutes,
                Price = s.Price
            })
            .ToList();
    }

    public async Task<List<WorkerServiceEntity>> GetWorkerServiceEntityByIdAsync(int workerId)
    {
        var workerMappings = await unitOfWork.WorkerServiceMapping
            .FindAsync(ws => ws.WorkerId == workerId);

        if (workerMappings == null || !workerMappings.Any())
            return new List<WorkerServiceEntity>();

        var serviceIds = workerMappings.Select(m => m.ServiceId).ToList();

        var services = await unitOfWork.WorkerService
            .FindAsync(s => serviceIds.Contains(s.Id));

        return services.ToList();
    }

    public async Task<List<WorkerServiceEntity>> GetWorkerServiceEntitiesAsync(List<int> workerIds)
    {
        if (workerIds == null || !workerIds.Any())
            return new List<WorkerServiceEntity>();

        var allMappings = await unitOfWork.WorkerService.FindAsync(s => workerIds.Contains(s.Id));

        return allMappings.ToList();

    }

    public async Task<List<Worker>> GetWorkerServiceIdsAsync(List<int> workerIds)
    {
        // workerIds listesinde olan tüm WorkerId'leri çek
        var workers = await unitOfWork.Workers
            .FindAsync(y => workerIds.Contains(y.Id)); // FindAsync Expression alıyor

        return workers.ToList(); // List<Worker> döndür
    }



    public async Task<WorkerServiceEntity> GetWorkerServiceByIdAsync(int serviceId)
    {
        var service = await unitOfWork.WorkerService.GetByIdAsync(serviceId);
        return service;
    }

    public async Task<WorkerServiceEntity> UpdateWorkerServiceEntityAsync(WorkerServiceEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var existingService = await unitOfWork.WorkerService.GetByIdAsync(entity.Id);
        if (existingService == null)
            throw new KeyNotFoundException($"Service with Id {entity.Id} not found.");

        existingService.ServiceName = entity.ServiceName;
        existingService.DurationMinutes = entity.DurationMinutes;
        existingService.Price = entity.Price;
        unitOfWork.WorkerService.Update(existingService);
        await unitOfWork.SaveChangesAsync();
        return existingService;
    }

    public async Task<bool> DeleteWorkerServiceEntityAsync(int id)
    {
        try
        {
            var entity = await unitOfWork.WorkerService.GetByIdAsync(id);
            if (entity == null)
            {
                return false;  
            }

            unitOfWork.WorkerService.Remove(entity);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Deleted WorkerService: {ServiceId} - {ServiceName}", entity.Id, entity.ServiceName);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete WorkerService with Id {ServiceId}", id);
            return false;
        }
    }


    public async Task<IEnumerable<Worker>> GetActiveWorkersAsync()
    {
        var workers = await unitOfWork.Workers.GetActiveWorkersAsync();
        return workers;
    }

    public async Task<Worker?> GetWorkerByIdAsync(int id)
    {
        return await unitOfWork.Workers.GetByIdAsync(id);
    }

    public async Task<Worker?> GetWorkerWithSchedulesAsync(int id)
    {
        return await unitOfWork.Workers.GetWorkerWithSchedulesAsync(id);
    }

   

    public async Task<Worker> CreateWorkerAsync(
        Worker worker,
        IEnumerable<WorkerSchedule> schedules,
        IEnumerable<int> selectedServiceIds) // formdan gelen seçili hizmetler
    {
        try
        {
            // 1️⃣ Çalışanı ekle
            worker.CreatedAt = DateTime.UtcNow;
            await unitOfWork.Workers.AddAsync(worker);
            await unitOfWork.SaveChangesAsync();

            // 2️⃣ Çalışma saatlerini ekle
            foreach (var schedule in schedules)
            {
                schedule.WorkerId = worker.Id;
                await unitOfWork.WorkerSchedules.AddAsync(schedule);
            }

            // 3️⃣ Seçilen hizmetleri mapping tablosuna ekle
            if (selectedServiceIds != null && selectedServiceIds.Any())
            {
                var workerServices = selectedServiceIds.Select(serviceId => new WorkerServiceMapping
                {
                     WorkerId= worker.Id,
                    ServiceId = serviceId
                }).ToList();

                await unitOfWork.WorkerServiceMapping.AddRangeAsync(workerServices);
            }
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Created worker: {WorkerId} - {WorkerName}", worker.Id, worker.Name);
            return worker;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create worker");
            throw;
        }
    }
    public async Task<bool> UpdateWorkerAsync(int id, Worker worker, IEnumerable<WorkerSchedule> schedules)
    {
        try
        {
            var existingWorker = await unitOfWork.Workers.GetByIdAsync(id);
            if (existingWorker == null)
                return false;

            existingWorker.Name = worker.Name;
            existingWorker.Specialty = worker.Specialty;
            existingWorker.IsActive = worker.IsActive;

            unitOfWork.Workers.Update(existingWorker);

            var existingSchedules = await unitOfWork.WorkerSchedules.GetByWorkerIdAsync(id);
            unitOfWork.WorkerSchedules.RemoveRange(existingSchedules);

            foreach (var schedule in schedules)
            {
                schedule.WorkerId = id;
                await unitOfWork.WorkerSchedules.AddAsync(schedule);
            }

            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Updated worker: {WorkerId} - {WorkerName}", id, worker.Name);
                
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update worker {WorkerId}", id);
            throw;
        }
    }

    public async Task<bool> ToggleWorkerActiveStatusAsync(int id)
    {
        try
        {
            var worker = await unitOfWork.Workers.GetByIdAsync(id);
            if (worker == null)
                return false;

            worker.IsActive = !worker.IsActive;
            unitOfWork.Workers.Update(worker);
            await unitOfWork.SaveChangesAsync();
                
            logger.LogInformation("Toggled worker active status: {WorkerId} - IsActive: {IsActive}", id, worker.IsActive);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to toggle worker status {WorkerId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteWorkerAsync(int id)
    {
        try
        {
            var worker = await unitOfWork.Workers.GetByIdAsync(id);
            if (worker == null)
                return false;

            unitOfWork.Workers.Remove(worker);
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Deleted worker: {WorkerId}", id);
                
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete worker {WorkerId}", id);
            throw;
        }
    }

    private static string GetShortDayName(int dayOfWeek)
    {
        return dayOfWeek switch
        {
            0 => "Paz",
            1 => "Pzt",
            2 => "Sal",
            3 => "Çar",
            4 => "Per",
            5 => "Cum",
            6 => "Cmt",
            _ => ""
        };
    }
}