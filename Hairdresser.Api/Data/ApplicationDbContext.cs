using Hairdresser.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Hairdresser.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Worker> Workers { get; set; }
    public DbSet<WorkerSchedule> WorkerSchedules { get; set; }
    public DbSet<BusinessConfig> BusinessConfigs { get; set; }
    public DbSet<WorkerServiceEntity> WorkerServices { get; set; }
    public DbSet<WorkerServiceMapping>  WorkerServiceMapping { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.LastContact).HasDefaultValueSql("NOW()");
        });
        modelBuilder.Entity<WorkerServiceEntity>(entity =>
        {
            entity.ToTable("worker_services");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ServiceName).IsRequired();
            entity.Property(e => e.DurationMinutes).IsRequired();
            entity.Property(e => e.Price).IsRequired();
        });

        // WorkerServiceMapping tablosu (many-to-many)
        modelBuilder.Entity<WorkerServiceMapping>(entity =>
        {
            entity.ToTable("worker_service_mapping");
            entity.HasKey(e => new { e.WorkerId, e.ServiceId }); // composite key

            entity.HasOne(e => e.Worker)
                .WithMany()
                .HasForeignKey(e => e.WorkerId);

            entity.HasOne(e => e.Service)
                .WithMany()
                .HasForeignKey(e => e.ServiceId);
        });
        modelBuilder.Entity<Worker>(entity =>
        {
            entity.ToTable("workers");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<WorkerSchedule>(entity =>
        {
            entity.ToTable("worker_schedules");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WorkerId, e.DayOfWeek }).IsUnique();
            entity.Property(e => e.IsWorking).HasDefaultValue(true);

            entity.HasOne(ws => ws.Worker)
                .WithMany(w => w.Schedules)
                .HasForeignKey(ws => ws.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WorkerId, e.AppointmentDate, e.AppointmentTime }).IsUnique();
            entity.Property(e => e.Status).HasDefaultValue("pending");
            entity.Property(e => e.DurationMinutes).HasDefaultValue(60);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Worker)
                .WithMany(w => w.Appointments)
                .HasForeignKey(a => a.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BusinessConfig>(entity =>
        {
            entity.ToTable("business_config");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConfigKey).IsUnique();
        });
    }
}