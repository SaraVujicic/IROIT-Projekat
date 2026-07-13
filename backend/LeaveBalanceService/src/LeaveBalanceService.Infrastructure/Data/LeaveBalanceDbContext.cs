using LeaveBalanceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LeaveBalanceService.Infrastructure.Data;

public class LeaveBalanceDbContext : DbContext
{
    public LeaveBalanceDbContext(DbContextOptions<LeaveBalanceDbContext> options) : base(options)
    {
    }

    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
