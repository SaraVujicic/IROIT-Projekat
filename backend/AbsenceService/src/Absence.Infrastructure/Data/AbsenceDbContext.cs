namespace Absence.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Absence.Domain.Entities;

public class AbsenceDbContext : DbContext
{
    public DbSet<global::Absence.Domain.Entities.Absence> Absences { get; set; }

    public AbsenceDbContext(DbContextOptions<AbsenceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<global::Absence.Domain.Entities.Absence>()
            .ToTable("Absences")
            .HasKey(a => a.AbsenceId);

        // TPH Configuration
        modelBuilder.Entity<global::Absence.Domain.Entities.Absence>()
            .HasDiscriminator<string>("AbsenceType")
            .HasValue<AnnualLeave>("Annual")
            .HasValue<SickLeave>("Sick")
            .HasValue<DayOff>("DayOff");
    }
}
