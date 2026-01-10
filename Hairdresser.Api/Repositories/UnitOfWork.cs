using Hairdresser.Api.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hairdresser.Api.Repositories;

public class UnitOfWork(
    ApplicationDbContext context,
    IUserRepository users,
    IWorkerRepository workers,
    IWorkerScheduleRepository workerSchedules,
    IAppointmentRepository appointments,
    IBusinessConfigRepository businessConfigs,IWorkerServiceEntityRepository workerService,IWorkerServiceMappingRepository workerServiceMapping)
    : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IUserRepository Users { get; } = users;
    public IWorkerRepository Workers { get; } = workers;
    public IWorkerScheduleRepository WorkerSchedules { get; } = workerSchedules;
    public IAppointmentRepository Appointments { get; } = appointments;
    public IBusinessConfigRepository BusinessConfigs { get; } = businessConfigs;
    public IWorkerServiceEntityRepository WorkerService { get; }  = workerService;
    public IWorkerServiceMappingRepository  WorkerServiceMapping { get; }  = workerServiceMapping;

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
    }
}