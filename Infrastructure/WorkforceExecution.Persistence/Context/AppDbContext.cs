using Microsoft.EntityFrameworkCore;
using WorkforceExecution.Domain.Entities;

namespace WorkforceExecution.Persistence.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<CrewRegion> CrewRegions => Set<CrewRegion>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TypeOfWork> TypeOfWorks => Set<TypeOfWork>();
    public DbSet<SubTypeOfWork> SubTypeOfWorks => Set<SubTypeOfWork>();
    public DbSet<SubSubTypeOfWork> SubSubTypeOfWorks => Set<SubSubTypeOfWork>();
    public DbSet<WorkerType> WorkerTypes => Set<WorkerType>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<CrewAssignment> CrewAssignments => Set<CrewAssignment>();
    public DbSet<ApprovalLog> ApprovalLogs => Set<ApprovalLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(e =>
        {
            //sorgu hýzlanýr ayný kullanýcý db seviyesinde engellenir 
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(150);
            e.HasOne(u => u.Location).WithMany().HasForeignKey(u => u.LocationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(u => u.CrewRegion).WithMany().HasForeignKey(u => u.CrewRegionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Location>(e =>
        {
            e.HasIndex(l => l.Code).IsUnique();
            e.Property(l => l.Code).HasMaxLength(20);
        });

        modelBuilder.Entity<WorkItem>(e =>
        {
            e.Property(w => w.PlannedQuantity).HasPrecision(18, 3);
            e.Property(w => w.PlannedManday).HasPrecision(18, 2);
            e.Property(w => w.FactQuantity).HasPrecision(18, 3);
            e.Property(w => w.FactManday).HasPrecision(18, 2);
            e.Property(w => w.Overtime).HasPrecision(18, 2);
            e.HasIndex(w => new { w.ReportDate, w.Status }); // rapor/kuyruk sorgulari icin
            e.HasOne(w => w.SubmittedBy).WithMany().HasForeignKey(w => w.SubmittedById).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(w => w.AssignedHom).WithMany().HasForeignKey(w => w.AssignedHomId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(w => w.Location).WithMany().HasForeignKey(w => w.LocationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(w => w.Project).WithMany().HasForeignKey(w => w.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApprovalLog>(e =>
        {
            e.HasOne(a => a.Approver).WithMany().HasForeignKey(a => a.ApproverId).OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}
