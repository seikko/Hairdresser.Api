using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hairdresser.Api.Models;
using Hairdresser.Api.Models.ViewModels;
using Hairdresser.Api.Services;

namespace BookingAPI.Controllers
{
    [Authorize]
    public class WorkersController : Controller
    {
        private readonly IWorkerService _workerService;

        public WorkersController(IWorkerService workerService)
        {
            _workerService = workerService;
        }

        public async Task<IActionResult> Index()
        {
            var workers = await _workerService.GetAllWorkersAsync();
            return View(workers);
        }

        public IActionResult Create()
        {
            var viewModel = new WorkerViewModel
            {
                Schedules = GetDefaultSchedules()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkerViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var worker = new Worker
                {
                    Name = viewModel.Name,
                    Specialty = viewModel.Specialty,
                    IsActive = viewModel.IsActive
                };

                var schedules = viewModel.Schedules
                    .Where(s => s.IsWorking)
                    .Select(s => new WorkerSchedule
                    {
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        IsWorking = true
                    });

                await _workerService.CreateWorkerAsync(worker, schedules);

                TempData["Success"] = $"{worker.Name} başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }

            viewModel.Schedules = GetDefaultSchedules();
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var worker = await _workerService.GetWorkerWithSchedulesAsync(id.Value);

            if (worker == null)
            {
                return NotFound();
            }

            var viewModel = new WorkerViewModel
            {
                Id = worker.Id,
                Name = worker.Name,
                Specialty = worker.Specialty,
                IsActive = worker.IsActive,
                Schedules = GetSchedulesForWorker(worker)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkerViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var worker = new Worker
                {
                    Name = viewModel.Name,
                    Specialty = viewModel.Specialty,
                    IsActive = viewModel.IsActive
                };

                var schedules = viewModel.Schedules
                    .Where(s => s.IsWorking)
                    .Select(s => new WorkerSchedule
                    {
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        IsWorking = true
                    });

                var success = await _workerService.UpdateWorkerAsync(id, worker, schedules);

                if (!success)
                {
                    return NotFound();
                }

                TempData["Success"] = $"{worker.Name} başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var worker = await _workerService.GetWorkerWithSchedulesAsync(id.Value);

            if (worker == null)
            {
                return NotFound();
            }

            return View(worker);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var worker = await _workerService.GetWorkerByIdAsync(id);
            if (worker != null)
            {
                var name = worker.Name;
                await _workerService.DeleteWorkerAsync(id);
                TempData["Success"] = $"{name} başarıyla silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var worker = await _workerService.GetWorkerByIdAsync(id);
            if (worker == null)
            {
                return NotFound();
            }

            var workerName = worker.Name;
            await _workerService.ToggleWorkerActiveStatusAsync(id);

            TempData["Success"] = $"{workerName} durumu güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        private List<WorkerScheduleViewModel> GetDefaultSchedules()
        {
            var schedules = new List<WorkerScheduleViewModel>();
            var dayNames = new[] { "Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi" };

            for (int i = 0; i < 7; i++)
            {
                schedules.Add(new WorkerScheduleViewModel
                {
                    DayOfWeek = i,
                    DayName = dayNames[i],
                    IsWorking = i >= 1 && i <= 6,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(18, 0)
                });
            }

            return schedules;
        }

        private List<WorkerScheduleViewModel> GetSchedulesForWorker(Worker worker)
        {
            var schedules = new List<WorkerScheduleViewModel>();
            var dayNames = new[] { "Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi" };

            for (int i = 0; i < 7; i++)
            {
                var existingSchedule = worker.Schedules.FirstOrDefault(s => s.DayOfWeek == i);
                schedules.Add(new WorkerScheduleViewModel
                {
                    Id = existingSchedule?.Id ?? 0,
                    DayOfWeek = i,
                    DayName = dayNames[i],
                    IsWorking = existingSchedule?.IsWorking ?? false,
                    StartTime = existingSchedule?.StartTime ?? new TimeOnly(9, 0),
                    EndTime = existingSchedule?.EndTime ?? new TimeOnly(18, 0)
                });
            }

            return schedules;
        }
    }
}
