using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Clients;

public class OrderApiClient(string baseUrl) : ApiClientBase(baseUrl)
{
    public Task<PurchaseCourseResponse?> PurchaseAsync(PurchaseCourseRequest request, CancellationToken ct = default)
        => PostAsync<PurchaseCourseResponse>("/api/orders/purchase", request, ct);
}
