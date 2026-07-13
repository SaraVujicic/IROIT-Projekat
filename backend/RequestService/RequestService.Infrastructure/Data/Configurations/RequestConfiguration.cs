using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RequestService.Domain.Entities;

namespace RequestService.Infrastructure.Data.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.HasKey(r => r.Id);
        
        // Map RequestId (from domain specifically requested) to Id since Entity already has Id
        builder.Ignore(r => r.RequestId);

        builder.Property(r => r.Description).IsRequired().HasMaxLength(500);
        builder.Property(r => r.Type).IsRequired();
        
        // Owns one StatusVO
        builder.OwnsOne(r => r.Status, statusBuilder =>
        {
            statusBuilder.Property(s => s.Name).HasColumnName("Status_Name").IsRequired().HasMaxLength(50);
            statusBuilder.Property(s => s.Description).HasColumnName("Status_Description").HasMaxLength(255);
        });

        // Owns credential audit VOs
        builder.OwnsOne(r => r.ManagerAudit, managerBuilder =>
        {
            managerBuilder.Property(m => m.Username).HasColumnName("ManagerAudit_Username");
            managerBuilder.Property(m => m.Password).HasColumnName("ManagerAudit_Password");
        });

        builder.OwnsOne(r => r.AdminAudit, adminBuilder =>
        {
            adminBuilder.Property(a => a.Username).HasColumnName("AdminAudit_Username");
            adminBuilder.Property(a => a.Password).HasColumnName("AdminAudit_Password");
        });

        // Configure relations 1:N enforcing own tables
        builder.HasMany(r => r.TravelOrders)
               .WithOne()
               .HasForeignKey("RequestId")    // Shadow foreign key pointing back to Request
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Internships)
               .WithOne()
               .HasForeignKey("RequestId")
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Educations)
               .WithOne()
               .HasForeignKey("RequestId")
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TravelOrderConfiguration : IEntityTypeConfiguration<TravelOrder>
{
    public void Configure(EntityTypeBuilder<TravelOrder> builder)
    {
        builder.ToTable("TravelOrders");
        builder.HasKey(t => t.Id);
        builder.Ignore(t => t.TravelOrderId);
        builder.Property(t => t.Destination).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Costs).HasColumnType("decimal(18,2)");
    }
}

public class InternshipConfiguration : IEntityTypeConfiguration<Internship>
{
    public void Configure(EntityTypeBuilder<Internship> builder)
    {
        builder.ToTable("Internships");
        builder.HasKey(i => i.Id);
        builder.Ignore(i => i.InternshipId);
        builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
        
        // MentorVO is Owned Type
        builder.OwnsOne(i => i.Mentor, mentorBuilder =>
        {
            mentorBuilder.Property(m => m.FirstName).HasColumnName("Mentor_FirstName").IsRequired();
            mentorBuilder.Property(m => m.LastName).HasColumnName("Mentor_LastName").IsRequired();
            mentorBuilder.Property(m => m.Position).HasColumnName("Mentor_Position").IsRequired();
        });
    }
}

public class EducationConfiguration : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> builder)
    {
        builder.ToTable("Educations");
        builder.HasKey(e => e.Id);
        builder.Ignore(e => e.EducationId);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
    }
}
