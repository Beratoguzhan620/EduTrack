using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EduTrack.Web.Filters;

/// <summary>
/// Runs any registered FluentValidation validator for the action's arguments and adds the
/// failures to ModelState, so controllers keep using <c>ModelState.IsValid</c>.
/// Replaces the deprecated FluentValidation.AspNetCore auto-validation.
/// </summary>
public sealed class FluentValidationFilter(IServiceProvider services) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (services.GetService(validatorType) is not IValidator validator)
                continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
            foreach (var error in result.Errors)
            {
                context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }

        await next();
    }
}
