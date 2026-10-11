using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EduTrack.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseStudent> CourseStudent { get; set; }
    public DbSet<StreamPost> StreamPosts { get; set; }
    public DbSet<CourseMaterial> CourseMaterials { get; set; }
    


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
