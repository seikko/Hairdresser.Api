using Hairdresser.Api.Models;
using Hairdresser.Api.Models.ViewModels;

namespace Hairdresser.Api.Services;

public interface IWorkerService
{
    Task<IEnumerable<WorkerListViewModel>> GetAllWorkersAsync();
    Task<IEnumerable<Worker>> GetActiveWorkersAsync();
    Task<Worker?> GetWorkerByIdAsync(int id);
    Task<Worker?> GetWorkerWithSchedulesAsync(int id);

    Task<Worker> CreateWorkerAsync(
        Worker worker,
        IEnumerable<WorkerSchedule> schedules,
        IEnumerable<int> selectedServiceIds);
    Task<bool> UpdateWorkerAsync(int id, Worker worker, IEnumerable<WorkerSchedule> schedules);
    Task<bool> ToggleWorkerActiveStatusAsync(int id);
    Task<bool> DeleteWorkerAsync(int id);
    
    

    Task<bool> CreateWorkerServiceEntityAsync(ServiceViewModel workerServiceViewModel);
    Task<bool> UpdateWorkerServicesAsync(int workerId, IEnumerable<int> selectedServiceIds);
    Task<List<WorkerServiceViewModel>> GetAllServicesAsync();
    Task<List<WorkerServiceEntity>> GetWorkerServiceEntityByIdAsync(int workerId);
    Task<List<WorkerServiceEntity>> GetWorkerServiceEntitiesAsync(List<int> workerId);
    Task<WorkerServiceEntity> GetWorkerServiceByIdAsync(int serviceId);
    Task<WorkerServiceEntity> UpdateWorkerServiceEntityAsync(WorkerServiceEntity entity);
    Task<bool> DeleteWorkerServiceEntityAsync(int id);
}