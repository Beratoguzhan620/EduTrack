using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduTrack.Persistence.Configurations;

public class CourseMaterialConfiguration : IEntityTypeConfiguration<CourseMaterial>
{
    public void Configure(EntityTypeBuilder<CourseMaterial> builder)
    {
        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(cm => cm.Course)
            .WithMany(c => c.Materials)
            .HasForeignKey(cm => cm.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}