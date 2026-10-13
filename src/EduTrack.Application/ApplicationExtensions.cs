using Core.BusinessRules;
using EduTrack.Application.Services.Abstract;
using EduTrack.Application.Services.Concrete;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
namespace EduTrack.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IStreamPostService, StreamPostService>();
        services.AddScoped<ICourseMaterialService, CourseMaterialService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.TryAddSingleton(TimeProvider.System);

        //AutMapper Ekler
        services.AddAutoMapper(_ => { }, Assembly.GetExecutingAssembly());

        // FluentValidation validators; the web layer runs them through an MVC filter
        // (FluentValidation.AspNetCore auto-validation is deprecated).
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());


        //Scrutor ile BusinessRule'ları DI'a ekler
        services.Scan(s =>
             s.FromAssemblies(Assembly.GetExecutingAssembly())
             .AddClasses(classes => classes.AssignableTo<IBusinessRule>())
             .AsSelf()
             .WithScopedLifetime()
             );

        return services;
    }

}
