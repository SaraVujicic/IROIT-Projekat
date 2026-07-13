using LeaveBalanceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveBalanceService.Infrastructure.Data.Configurations;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalances");

        builder.HasKey(x => x.LeaveBalanceId);

        builder.Property(x => x.EmployeeId)
            .IsRequired();

        builder.Property(x => x.Year)
            .IsRequired();
        
        // Ensure there is only one leave balance per employee per year
        builder.HasIndex(x => new { x.EmployeeId, x.Year })
            .IsUnique();

        builder.Property(x => x.TotalDays)
            .IsRequired();

        builder.Property(x => x.CarriedOverDays)
            .IsRequired();

        builder.Property(x => x.RemainingDays)
            .IsRequired();

        builder.Property(x => x.ExpirationDate)
            .HasColumnType("date")
            .IsRequired();
    }
}
