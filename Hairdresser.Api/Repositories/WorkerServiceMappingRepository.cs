using Hairdresser.Api.Data;
using Hairdresser.Api.Models;

namespace Hairdresser.Api.Repositories;

public class WorkerServiceMappingRepository(ApplicationDbContext context): Repository<WorkerServiceMapping>(context), IWorkerServiceMappingRepository
{
    
}