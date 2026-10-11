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
        services.AddDbContext<AppDbContext>(options =>
             options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));


        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseStudentRepository, CourseStudentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IStreamPostRepository, StreamPostRepository>();
        services.AddScoped<ICourseMaterialRepository, CourseMaterialRepository>();

        return services;
    }

}
