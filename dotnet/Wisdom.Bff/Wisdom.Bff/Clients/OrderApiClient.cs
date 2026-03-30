using System.Text.Json;

namespace Wisdom.Bff.Clients;

public sealed class OrderApiClient(HttpClient httpClient) : IOrderApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<BackendCheckEnrollment?> CheckEnrollmentAsync(Guid courseId, CancellationToken ct)
    {
        var response = await httpClient.GetAsync($"/api/orders/enrollments/{courseId}/check", ct);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<BackendCheckEnrollment>(JsonOptions, ct);
    }

    public async Task<IReadOnlyList<BackendEnrollment>> GetEnrollmentsAsync(CancellationToken ct)
    {
        var response = await httpClient.GetAsync("/api/orders/enrollments", ct);

        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<BackendEnrollment>>(JsonOptions, ct) ?? [];
    }
}
