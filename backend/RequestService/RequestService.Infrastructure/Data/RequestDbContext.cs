using Microsoft.EntityFrameworkCore;
using RequestService.Domain.Entities;
using RequestService.Domain.Interfaces;

namespace RequestService.Infrastructure.Data;

public class RequestDbContext : DbContext, IUnitOfWork
{
    public RequestDbContext(DbContextOptions<RequestDbContext> options) : base(options)
    {
    }

    public DbSet<Request> Requests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RequestDbContext).Assembly);
        
        // Ensure child entities get their own tables despite NOT being DbSets on the context.
        // We configure them in RequestConfiguration primarily, but verify they are recognized.
        modelBuilder.Entity<TravelOrder>();
        modelBuilder.Entity<Internship>();
        modelBuilder.Entity<Education>();

        base.OnModelCreating(modelBuilder);
    }
}
