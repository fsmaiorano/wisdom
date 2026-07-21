using System.Net.Http.Json;

namespace Wisdom.Bff.Clients;

public sealed class OrderApiClient(HttpClient httpClient) : DownstreamClientBase(httpClient), IOrderApiClient
{
    public Task<BackendCheckEnrollment> CheckEnrollmentAsync(Guid courseId, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"/api/orders/enrollments/{courseId}/check");
        return SendAsync<BackendCheckEnrollment>(req, ct)!;
    }

    public async Task<IReadOnlyList<BackendEnrollment>> GetEnrollmentsAsync(CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/orders/enrollments");
        return await SendAsync<List<BackendEnrollment>>(req, ct) ?? [];
    }

    public Task<BackendPurchase> PurchaseCourseAsync(PurchaseCourseRequest request, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/orders/purchase")
        {
            Content = JsonContent.Create(request, options: JsonOptions),
        };
        return SendAsync<BackendPurchase>(req, ct)!;
    }
}
