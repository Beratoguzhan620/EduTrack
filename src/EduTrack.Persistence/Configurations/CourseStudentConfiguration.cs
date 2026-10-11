using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduTrack.Persistence.Configurations;

public class CourseStudentConfiguration : IEntityTypeConfiguration<CourseStudent>
{
    public void Configure(EntityTypeBuilder<CourseStudent> builder)
    {
        builder.HasKey(cs => cs.Id);

        builder.HasIndex(cs => new { cs.CourseId, cs.UserId })
            .IsUnique();

        builder.Property(cs => cs.EnrolledAt)
            .IsRequired();

        builder.HasOne(cs => cs.Course)
            .WithMany(c => c.Students)
            .HasForeignKey(cs => cs.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.User)
            .WithMany()
            .HasForeignKey(cs => cs.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
