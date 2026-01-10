using System.Linq.Expressions;
using Hairdresser.Api.Data;
using Hairdresser.Api.Models;

namespace Hairdresser.Api.Repositories;

public class WorkerServiceEntityRepository(ApplicationDbContext context):Repository<WorkerServiceEntity>(context), IWorkerServiceEntityRepository
{
    
}