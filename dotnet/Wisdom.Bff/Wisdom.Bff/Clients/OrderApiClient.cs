using System.Net.Http.Json;

namespace Wisdom.Bff.Clients;

public interface IOrderApiClient
{
    Task<BackendCheckEnrollment> CheckEnrollmentAsync(Guid courseId, CancellationToken ct);
    Task<IReadOnlyList<BackendEnrollment>> GetEnrollmentsAsync(CancellationToken ct);

    Task<BackendPurchase> PurchaseCourseAsync(PurchaseCourseRequest request, CancellationToken ct);
}

// Mirrors Wisdom.Order.Application.Dtos
public record BackendCheckEnrollment(bool IsEnrolled);

public record BackendEnrollment(
    Guid EnrollmentId,
    Guid UserId,
    Guid CourseId,
    DateTime? CreatedAt);

public record BackendPurchase(Guid PurchaseId, Guid UserId, Guid CourseId, decimal Price, DateTime? CreatedAt);
public record PurchaseCourseRequest(Guid CourseId, decimal Price);

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
