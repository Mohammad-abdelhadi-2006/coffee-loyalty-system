using System.Text.Json;

namespace CoffeeLoyalty.Api.Common;

/// <summary>
/// The one and only error body in this API:
/// <c>{ "code": "MACHINE_READABLE_CODE", "message": "نص عربي" }</c>.
/// </summary>
/// <param name="Code">Stable English constant from the registry.</param>
/// <param name="Message">Arabic display text.</param>
public sealed record ApiError(string Code, string Message)
{
    /// <summary>camelCase, unescaped Arabic — the shape clients actually receive.</summary>
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Writes this error to the response as JSON with the given status code.
    /// No-ops the status if the response has already started.
    /// </summary>
    /// <param name="response">The response to write to.</param>
    /// <param name="statusCode">The HTTP status to send.</param>
    /// <param name="cancellationToken">Cancellation token for the write.</param>
    public async Task WriteToAsync(HttpResponse response, int statusCode, CancellationToken cancellationToken = default)
    {
        if (!response.HasStarted)
        {
            response.Clear();
            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
        }

        await response.WriteAsync(JsonSerializer.Serialize(this, SerializerOptions), cancellationToken);
    }
}
