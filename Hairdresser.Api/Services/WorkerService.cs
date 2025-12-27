using Hairdresser.Api.Models;
using Hairdresser.Api.Models.ViewModels;
using Hairdresser.Api.Repositories;

namespace Hairdresser.Api.Services
{
    public class WorkerService : IWorkerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(IUnitOfWork unitOfWork, ILogger<WorkerService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<WorkerListViewModel>> GetAllWorkersAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var workers = await _unitOfWork.Workers.GetAllAsync();
            
            var viewModels = new List<WorkerListViewModel>();
            
            foreach (var worker in workers.OrderBy(w => w.Name))
            {
                var schedules = await _unitOfWork.WorkerSchedules.GetByWorkerIdAsync(worker.Id);
                var appointments = await _unitOfWork.Appointments.FindAsync(a => a.WorkerId == worker.Id);
                
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

        public async Task<IEnumerable<Worker>> GetActiveWorkersAsync()
        {
            var workers = await _unitOfWork.Workers.GetActiveWorkersAsync();
            return workers;
        }

        public async Task<Worker?> GetWorkerByIdAsync(int id)
        {
            return await _unitOfWork.Workers.GetByIdAsync(id);
        }

        public async Task<Worker?> GetWorkerWithSchedulesAsync(int id)
        {
            return await _unitOfWork.Workers.GetWorkerWithSchedulesAsync(id);
        }

        public async Task<Worker> CreateWorkerAsync(Worker worker, IEnumerable<WorkerSchedule> schedules)
        {
            try
            {
                worker.CreatedAt = DateTime.UtcNow;
                await _unitOfWork.Workers.AddAsync(worker);
                await _unitOfWork.SaveChangesAsync();

                foreach (var schedule in schedules)
                {
                    schedule.WorkerId = worker.Id;
                    await _unitOfWork.WorkerSchedules.AddAsync(schedule);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Created worker: {WorkerId} - {WorkerName}", worker.Id, worker.Name);
                
                return worker;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create worker");
                throw;
            }
        }

        public async Task<bool> UpdateWorkerAsync(int id, Worker worker, IEnumerable<WorkerSchedule> schedules)
        {
            try
            {
                var existingWorker = await _unitOfWork.Workers.GetByIdAsync(id);
                if (existingWorker == null)
                    return false;

                existingWorker.Name = worker.Name;
                existingWorker.Specialty = worker.Specialty;
                existingWorker.IsActive = worker.IsActive;

                _unitOfWork.Workers.Update(existingWorker);

                var existingSchedules = await _unitOfWork.WorkerSchedules.GetByWorkerIdAsync(id);
                _unitOfWork.WorkerSchedules.RemoveRange(existingSchedules);

                foreach (var schedule in schedules)
                {
                    schedule.WorkerId = id;
                    await _unitOfWork.WorkerSchedules.AddAsync(schedule);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Updated worker: {WorkerId} - {WorkerName}", id, worker.Name);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update worker {WorkerId}", id);
                throw;
            }
        }

        public async Task<bool> ToggleWorkerActiveStatusAsync(int id)
        {
            try
            {
                var worker = await _unitOfWork.Workers.GetByIdAsync(id);
                if (worker == null)
                    return false;

                worker.IsActive = !worker.IsActive;
                _unitOfWork.Workers.Update(worker);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Toggled worker active status: {WorkerId} - IsActive: {IsActive}", id, worker.IsActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle worker status {WorkerId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteWorkerAsync(int id)
        {
            try
            {
                var worker = await _unitOfWork.Workers.GetByIdAsync(id);
                if (worker == null)
                    return false;

                _unitOfWork.Workers.Remove(worker);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Deleted worker: {WorkerId}", id);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete worker {WorkerId}", id);
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
}

