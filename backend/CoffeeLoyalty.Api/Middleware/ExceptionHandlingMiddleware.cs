using CoffeeLoyalty.Api.Common;
using CoffeeLoyalty.Api.Constants;

namespace CoffeeLoyalty.Api.Middleware;

/// <summary>
/// The single place every failure becomes the contract's error body
/// <c>{ "code", "message" }</c>. Deliberate failures arrive as
/// <see cref="ApiException"/> and keep their code and status; anything else is a
/// bug, is logged in full, and is returned as a bare 500 with no internal detail.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Creates the middleware.
    /// </summary>
    /// <param name="next">The next component in the pipeline.</param>
    /// <param name="logger">Where unhandled failures are recorded.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Runs the rest of the pipeline and converts any escaping exception.
    /// </summary>
    /// <param name="context">The current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            _logger.LogInformation(
                "Request {Method} {Path} rejected with {Code}.",
                context.Request.Method,
                context.Request.Path,
                ex.Code);

            await WriteAsync(context, ex.StatusCode, new ApiError(ex.Code, ex.ArabicMessage));
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client hung up; there is nobody left to answer.
            _logger.LogDebug("Request {Path} was aborted by the client.", context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception for {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            await WriteAsync(
                context,
                StatusCodes.Status500InternalServerError,
                new ApiError(ErrorCodes.InternalError, ErrorMessages.InternalError));
        }
    }

    /// <summary>
    /// Writes the error body, unless the response has already been flushed —
    /// in which case the status line is long gone and appending JSON would corrupt it.
    /// </summary>
    /// <param name="context">The current request.</param>
    /// <param name="statusCode">The HTTP status to send.</param>
    /// <param name="error">The error body.</param>
    private async Task WriteAsync(HttpContext context, int statusCode, ApiError error)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Cannot write error {Code}: the response has already started.", error.Code);
            return;
        }

        await error.WriteToAsync(context.Response, statusCode, context.RequestAborted);
    }
}
