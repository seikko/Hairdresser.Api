namespace Hairdresser.Api.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IWorkerRepository Workers { get; }
        IWorkerScheduleRepository WorkerSchedules { get; }
        IAppointmentRepository Appointments { get; }
        IBusinessConfigRepository BusinessConfigs { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}

