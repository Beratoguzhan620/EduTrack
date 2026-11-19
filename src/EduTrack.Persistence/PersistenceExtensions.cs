using EduTrack.Application.Repositories;
using EduTrack.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduTrack.Persistence;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // SQL Server by default; "Sqlite" (Database:Provider) runs the public demo, with the database in a file that
        // starts again from the demo content on every restart.
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.Equals(configuration["Database:Provider"], "Sqlite", StringComparison.OrdinalIgnoreCase))
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        else
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));


        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseStudentRepository, CourseStudentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IStreamPostRepository, StreamPostRepository>();
        services.AddScoped<ICourseMaterialRepository, CourseMaterialRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<IAssignmentSubmissionRepository, AssignmentSubmissionRepository>();

        return services;
    }

}
