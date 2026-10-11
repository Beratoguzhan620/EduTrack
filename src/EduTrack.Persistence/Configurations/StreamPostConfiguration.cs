using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduTrack.Persistence.Configurations;

public class StreamPostConfiguration : IEntityTypeConfiguration<StreamPost>
{
    public void Configure(EntityTypeBuilder<StreamPost> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Content)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Title).IsRequired().HasMaxLength(100);

        builder.HasOne(u => u.Course).WithMany(c => c.StreamPosts)
            .HasForeignKey(u => u.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Instructor).WithMany(i => i.StreamPosts)
            .HasForeignKey(u => u.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);
    }

}

