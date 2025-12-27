using Hairdresser.Api.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hairdresser.Api.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(
            ApplicationDbContext context,
            IUserRepository users,
            IWorkerRepository workers,
            IWorkerScheduleRepository workerSchedules,
            IAppointmentRepository appointments,
            IBusinessConfigRepository businessConfigs)
        {
            _context = context;
            Users = users;
            Workers = workers;
            WorkerSchedules = workerSchedules;
            Appointments = appointments;
            BusinessConfigs = businessConfigs;
        }

        public IUserRepository Users { get; }
        public IWorkerRepository Workers { get; }
        public IWorkerScheduleRepository WorkerSchedules { get; }
        public IAppointmentRepository Appointments { get; }
        public IBusinessConfigRepository BusinessConfigs { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
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
            _context.Dispose();
        }
    }
}

