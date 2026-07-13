using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Aggregates.NotificationAggregate;
using NotificationService.Infrastructure.Persistence.Configurations;

namespace NotificationService.Infrastructure.Persistence
{
    public class NotificationDbContext : DbContext
    {
        public DbSet<Notification> Notifications { get; set; }

        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        }
    }
}
