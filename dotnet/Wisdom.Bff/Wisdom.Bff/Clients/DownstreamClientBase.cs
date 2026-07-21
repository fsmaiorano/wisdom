using System.Net;
using System.Text.Json;

namespace Wisdom.Bff.Clients;

/// <summary>
/// Raised when a downstream service returns a non-success status. Carries the
/// downstream status/body/content-type verbatim so BFF endpoints can pass the
/// exact error shape through to the caller without re-modeling every service's
/// error contract (they differ: some return { error }, some a full Result&lt;T&gt;,
/// some a raw string).
/// </summary>
public sealed class DownstreamHttpException(int statusCode, string? body, string? contentType)
    : Exception($"Downstream call failed with status {statusCode}.")
{
    public int StatusCode { get; } = statusCode;
    public string? Body { get; } = body;
    public string? ContentType { get; } = contentType;
}

/// <summary>
/// Shared plumbing for every typed downstream client: send a request, either
/// deserialize a success DTO or throw <see cref="DownstreamHttpException"/>
/// with the raw downstream response. This is the single call path used both
/// by the explicit per-operation endpoints and by the aggregation handlers —
/// there is no separate "swallow failures" variant; a downstream failure is
/// always a real error, never silently mapped to null/empty.
/// </summary>
public abstract class DownstreamClientBase(HttpClient httpClient)
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected HttpClient Http { get; } = httpClient;

    protected async Task<T?> SendAsync<T>(HttpRequestMessage request, CancellationToken ct)
    {
        var response = await Http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            await ThrowAsync(response, ct);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return default;

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    /// <summary>Like <see cref="SendAsync{T}"/>, but a 404 yields null instead of throwing — for
    /// "get by id" lookups where "doesn't exist" is an expected outcome, not an error.</summary>
    protected async Task<T?> SendOrDefaultAsync<T>(HttpRequestMessage request, CancellationToken ct)
    {
        var response = await Http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;
        if (!response.IsSuccessStatusCode)
            await ThrowAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    protected async Task SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var response = await Http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            await ThrowAsync(response, ct);
    }

    private static async Task ThrowAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);
        throw new DownstreamHttpException(
            (int)response.StatusCode, body, response.Content.Headers.ContentType?.ToString());
    }
}
