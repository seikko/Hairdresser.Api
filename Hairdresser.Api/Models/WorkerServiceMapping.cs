using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hairdresser.Api.Models;

public class WorkerServiceMapping
{
    [Key]
    [Column("worker_id", Order = 0)]
    public int WorkerId { get; set; }

    [Key]
    [Column("service_id", Order = 1)]
    public int ServiceId { get; set; }

    [ForeignKey("WorkerId")]
    public Worker? Worker { get; set; }

    [ForeignKey("ServiceId")]
    public WorkerServiceEntity Service { get; set; }
}