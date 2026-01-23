namespace Udemy.Infrastructure.Filters;

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

/// <summary>
/// Action filter for validating request models using FluentValidation.
/// </summary>
public class ValidationFilterAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Executes before the action method.
    /// </summary>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            context.Result = new BadRequestObjectResult(new
            {
                StatusCode = 400,
                Message = "Validation failed",
                Errors = errors
            });

            return;
        }

        await next().ConfigureAwait(false);
    }
}

/// <summary>
/// Validation filter for models with FluentValidation support.
/// </summary>
public class FluentValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FluentValidationFilter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationFilter"/> class.
    /// </summary>
    public FluentValidationFilter(IServiceProvider serviceProvider, ILogger<FluentValidationFilter> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the validation filter.
    /// </summary>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var validationFailures = new Dictionary<string, string[]>();

        foreach (var (paramName, paramValue) in context.ActionArguments)
        {
            if (paramValue == null)
            {
                continue;
            }

            var paramType = paramValue.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(paramType);

            var validator = _serviceProvider.GetService(validatorType) as IValidator;
            if (validator == null)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(paramValue);
            var result = await validator.ValidateAsync(validationContext).ConfigureAwait(false);

            if (!result.IsValid)
            {
                var failures = result.Errors
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(f => f.ErrorMessage).ToArray());

                foreach (var (key, messages) in failures)
                {
                    validationFailures[key] = messages;
                }

                _logger.LogWarning("Validation failed for {ParameterName}: {Errors}",
                    paramName, string.Join(", ", result.Errors.Select(e => e.ErrorMessage)));
            }
        }

        if (validationFailures.Count > 0)
        {
            context.Result = new BadRequestObjectResult(new
            {
                StatusCode = 400,
                Message = "Validation failed",
                Errors = validationFailures
            });
            return;
        }

        await next().ConfigureAwait(false);
    }
}
