using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Aggregates.NotificationAggregate;

namespace NotificationService.Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            // 1. Table Mapping
            builder.ToTable("Notifications");

            // 2. Primary Key
            builder.HasKey(n => n.NotificationId);
            
            builder.Property(n => n.NotificationId)
                .UseIdentityByDefaultColumn()
                .ValueGeneratedOnAdd();

            // 3. Properties
            builder.Property(n => n.RecipientId)
                .IsRequired()
                .HasComment("Reference to the external Employee/User aggregate.");

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1500) // Safety boundary for PostgreSQL varchar mapping
                .HasColumnType("text");

            builder.Property(n => n.Date)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            builder.Property(n => n.ReadStatus)
                .IsRequired()
                .HasColumnType("boolean")
                .HasDefaultValue(false);
                
            // Ensures fast lookup for a user's notifications
            builder.HasIndex(x => x.RecipientId);
            
            // Order by date descending index
            builder.HasIndex(x => x.Date).IsDescending();
        }
    }
}
