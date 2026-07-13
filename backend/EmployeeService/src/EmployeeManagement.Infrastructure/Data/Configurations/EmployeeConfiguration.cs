using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.Department).IsRequired().HasMaxLength(100).HasDefaultValue("General");

        builder.OwnsOne(e => e.Credentials, c =>
        {
            c.Property(vo => vo.Username).HasColumnName("Credentials_Username").IsRequired().HasMaxLength(50);
            c.Property(vo => vo.PasswordHash).HasColumnName("Credentials_Password").IsRequired();
            c.HasIndex(vo => vo.Username).IsUnique(); // Ensure unique usernames
        });

        // TPH (Table-Per-Hierarchy) setup
        builder.HasDiscriminator<string>("Role")
            .HasValue<Employee>("Employee")
            .HasValue<Manager>("Manager")
            .HasValue<Admin>("Admin");

        // Self-referencing relationship for Manager
        builder.HasOne(e => e.Manager)
            .WithMany()
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
