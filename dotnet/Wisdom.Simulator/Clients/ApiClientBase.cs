using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Wisdom.Simulator.Clients;

public abstract class ApiClientBase
{
    protected readonly HttpClient Http;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected ApiClientBase(string baseUrl)
    {
        Http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public void SetBearerToken(string token)
    {
        Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearBearerToken()
    {
        Http.DefaultRequestHeaders.Authorization = null;
    }

    protected async Task<T?> PostAsync<T>(string path, object body, CancellationToken ct = default)
    {
        using var response = await Http.PostAsJsonAsync(path, body, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    protected async Task<T?> PostAsyncWithMultipartFallback<T>(string path, object body, CancellationToken ct = default)
    {
        using var jsonResponse = await Http.PostAsJsonAsync(path, body, ct);
        if (jsonResponse.StatusCode != HttpStatusCode.UnsupportedMediaType)
        {
            await EnsureSuccessAsync(jsonResponse, ct);
            return await jsonResponse.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
        }

        using var form = CreateMultipartFormData(body);
        using var formResponse = await Http.PostAsync(path, form, ct);
        await EnsureSuccessAsync(formResponse, ct);
        return await formResponse.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    protected async Task<T?> PostMultipartAsync<T>(string path, MultipartFormDataContent form, CancellationToken ct = default)
    {
        var response = await Http.PostAsync(path, form, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    protected async Task<T?> GetAsync<T>(string path, CancellationToken ct = default)
    {
        var response = await Http.GetAsync(path, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    protected async Task<T?> PatchAsync<T>(string path, object body, CancellationToken ct = default)
    {
        var response = await Http.PatchAsJsonAsync(path, body, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync(ct);
        throw new HttpRequestException(
            $"HTTP {(int)response.StatusCode} {response.ReasonPhrase} — {body}");
    }

    private static MultipartFormDataContent CreateMultipartFormData(object body)
    {
        var form = new MultipartFormDataContent();

        foreach (var property in body.GetType().GetProperties())
        {
            var value = property.GetValue(body);
            if (value is null)
                continue;

            form.Add(new StringContent(ConvertToString(value)), property.Name);
        }

        return form;
    }

    private static string ConvertToString(object value) => value switch
    {
        bool boolValue => boolValue ? "true" : "false",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };
}
