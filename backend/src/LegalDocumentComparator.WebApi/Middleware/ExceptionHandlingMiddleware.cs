using System.Net;
using System.Text.Json;
using LegalDocumentComparator.Application.Common.Exceptions;
using LegalDocumentComparator.Domain.Exceptions;
using ValidationException = LegalDocumentComparator.Application.Common.Exceptions.ValidationException;

namespace LegalDocumentComparator.WebApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        object problemDetails = exception switch
        {
            ValidationException validationEx => new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "One or more validation errors occurred.",
                status = (int)HttpStatusCode.BadRequest,
                errors = (object)validationEx.Errors,
                traceId = context.TraceIdentifier
            },
            NotFoundException notFoundEx => new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "The specified resource was not found.",
                status = (int)HttpStatusCode.NotFound,
                detail = notFoundEx.Message,
                traceId = context.TraceIdentifier
            },
            DomainException domainEx => new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "A domain error occurred.",
                status = (int)HttpStatusCode.BadRequest,
                detail = domainEx.Message,
                traceId = context.TraceIdentifier
            },
            _ => new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "An error occurred while processing your request.",
                status = (int)HttpStatusCode.InternalServerError,
                detail = exception.Message,
                traceId = context.TraceIdentifier
            }
        };

        var statusCode = exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            NotFoundException => (int)HttpStatusCode.NotFound,
            DomainException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
