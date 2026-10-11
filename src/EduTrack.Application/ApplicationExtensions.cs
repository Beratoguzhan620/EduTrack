using BlogApp.Application.Services.Concrete;
using Core.BusinessRules;
using EduTrack.Application.Services.Abstract;
using EduTrack.Application.Services.Concrete;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        //AutMapper Ekler
        services.AddAutoMapper(_ => { }, Assembly.GetExecutingAssembly());

        //FluentValidation Ekler
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddFluentValidationClientsideAdapters(); //TODO: deprecated


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
