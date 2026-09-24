using App.Application.Common.Exceptions;
using App.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            AppException app => Create(httpContext, app.StatusCode, app.Title, app.Message),
            DomainException domain => Create(httpContext, StatusCodes.Status400BadRequest, domain.Title, domain.Message),
            ValidationException validation => CreateValidation(httpContext, validation),
            UnauthorizedAccessException => Create(httpContext, StatusCodes.Status401Unauthorized, "Unauthorized", "Authentication is required."),
            _ => Create(httpContext, StatusCodes.Status500InternalServerError, "Server Error", "An unexpected error occurred.")
        };

        if (problem.Status >= 500)
            _logger.LogError(exception, "Unhandled exception");
        else
            _logger.LogWarning(exception, "Request failed with {StatusCode}: {Title}", problem.Status, problem.Title);

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static ProblemDetails Create(HttpContext context, int status, string title, string detail)
        => new()
        {
            Type = $"https://httpstatuses.com/{status}",
            Title = title,
            Status = status,
            Detail = detail,
            Instance = context.Request.Path
        };

    private static ValidationProblemDetails CreateValidation(HttpContext context, ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Type = "https://httpstatuses.com/400",
            Title = "Validation Failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred.",
            Instance = context.Request.Path
        };
    }
}
