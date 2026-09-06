using CameraOperationsManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CameraOperationsManagement.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Worker> Workers { get; set; }

        public DbSet<Site> Sites { get; set; }

        public DbSet<NetworkSwitch> NetworkSwitches { get; set; }

        public DbSet<Recorder> Recorders { get; set; }

        public DbSet<RecorderHardDrive> RecorderHardDrives { get; set; }

        public DbSet<Camera> Cameras { get; set; }

        public DbSet<SiteVisit> SiteVisits { get; set; }

        public DbSet<SiteVisitWorker> SiteVisitWorkers { get; set; }

        public DbSet<CameraVisit> CameraVisits { get; set; }

        public DbSet<CameraVisitWorker> CameraVisitWorkers { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<Visit> Visits { get; set; }

        public DbSet<VisitWorker> VisitWorkers { get; set; }

        public DbSet<NetworkSwitchPort> NetworkSwitchPorts { get; set; }
        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Site>()
                .HasIndex(s => s.Name)
                .IsUnique();

            builder.Entity<NetworkSwitch>()
                .HasIndex(s => new
                {
                    s.SiteId,
                    s.Name
                })
                .IsUnique();
            builder.Entity<Recorder>()
    .HasOne(r => r.NetworkSwitch)
    .WithMany()
    .HasForeignKey(r => r.NetworkSwitchId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Recorder>()
                .HasOne(r => r.Site)
                .WithMany()
                .HasForeignKey(r => r.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RecorderHardDrive>()
                .HasOne(h => h.Recorder)
                .WithMany(r => r.HardDrives)
                .HasForeignKey(h => h.RecorderId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Recorder>()
    .HasIndex(r => new
    {
        r.SiteId,
        r.Name
    })
    .IsUnique();
            builder.Entity<RecorderHardDrive>()
    .HasIndex(h => new
    {
        h.RecorderId,
        h.SerialNumber
    })
    .IsUnique()
    .HasFilter("[SerialNumber] IS NOT NULL");
            builder.Entity<Camera>()
    .HasOne(c => c.Recorder)
    .WithMany()
    .HasForeignKey(c => c.RecorderId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Camera>()
                .HasOne(c => c.NetworkSwitch)
                .WithMany()
                .HasForeignKey(c => c.NetworkSwitchId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<SiteVisit>()
    .HasOne(v => v.Site)
    .WithMany()
    .HasForeignKey(v => v.SiteId)
    .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<SiteVisitWorker>()
                .HasKey(vw => new
                {
                    vw.SiteVisitId,
                    vw.WorkerId
                });


            builder.Entity<SiteVisitWorker>()
                .HasOne(vw => vw.SiteVisit)
                .WithMany(v => v.SiteVisitWorkers)
                .HasForeignKey(vw => vw.SiteVisitId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<SiteVisitWorker>()
                .HasOne(vw => vw.Worker)
                .WithMany()
                .HasForeignKey(vw => vw.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CameraVisit>()
    .HasOne(v => v.Camera)
    .WithMany()
    .HasForeignKey(v => v.CameraId)
    .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<CameraVisitWorker>()
                .HasKey(vw => new
                {
                    vw.CameraVisitId,
                    vw.WorkerId
                });


            builder.Entity<CameraVisitWorker>()
                .HasOne(vw => vw.CameraVisit)
                .WithMany(v => v.CameraVisitWorkers)
                .HasForeignKey(vw => vw.CameraVisitId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<CameraVisitWorker>()
                .HasOne(vw => vw.Worker)
                .WithMany()
                .HasForeignKey(vw => vw.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // VISIT
            // =========================

            builder.Entity<Visit>()
                .HasOne(v => v.Site)
                .WithMany()
                .HasForeignKey(v => v.SiteId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Visit>()
                .HasOne(v => v.Recorder)
                .WithMany()
                .HasForeignKey(v => v.RecorderId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Visit>()
                .HasOne(v => v.NetworkSwitch)
                .WithMany()
                .HasForeignKey(v => v.NetworkSwitchId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Visit>()
                .HasOne(v => v.Camera)
                .WithMany()
                .HasForeignKey(v => v.CameraId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // VISIT WORKERS
            // =========================

            builder.Entity<VisitWorker>()
                .HasKey(vw => new
                {
                    vw.VisitId,
                    vw.WorkerId
                });


            builder.Entity<VisitWorker>()
                .HasOne(vw => vw.Visit)
                .WithMany(v => v.VisitWorkers)
                .HasForeignKey(vw => vw.VisitId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<VisitWorker>()
                .HasOne(vw => vw.Worker)
                .WithMany()
                .HasForeignKey(vw => vw.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Visit>()
    .ToTable(
        "Visits",
        table =>
        {
            table.HasCheckConstraint(
                "CK_Visits_Component",
                """
                (
                    [ComponentType] = 1
                    AND [RecorderId] IS NOT NULL
                    AND [NetworkSwitchId] IS NULL
                    AND [CameraId] IS NULL
                )
                OR
                (
                    [ComponentType] = 2
                    AND [RecorderId] IS NULL
                    AND [NetworkSwitchId] IS NOT NULL
                    AND [CameraId] IS NULL
                )
                OR
                (
                    [ComponentType] = 3
                    AND [RecorderId] IS NULL
                    AND [NetworkSwitchId] IS NULL
                    AND [CameraId] IS NOT NULL
                )
                """);
        });
            builder.Entity<Visit>()
    .HasIndex(v => new
    {
        v.SiteId,
        v.VisitDate
    });

            builder.Entity<NetworkSwitchPort>(entity =>
            {
                // A switch cannot have duplicate port numbers.
                entity.HasIndex(p => new
                {
                    p.NetworkSwitchId,
                    p.PortNumber
                })
                .IsUnique();


                // One camera can be connected to only one switch port.
                entity.HasIndex(p => p.CameraId)
                    .IsUnique()
                    .HasFilter("[CameraId] IS NOT NULL");


                entity.HasOne(p => p.NetworkSwitch)
                    .WithMany(s => s.Ports)
                    .HasForeignKey(p => p.NetworkSwitchId)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.HasOne(p => p.Camera)
                    .WithOne()
                    .HasForeignKey<NetworkSwitchPort>(p => p.CameraId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

    }
}