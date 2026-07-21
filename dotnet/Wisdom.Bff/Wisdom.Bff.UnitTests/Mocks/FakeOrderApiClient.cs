using Wisdom.Bff.Clients;

namespace Wisdom.Bff.UnitTests.Mocks;

public sealed class FakeOrderApiClient : IOrderApiClient
{
    public BackendCheckEnrollment EnrollmentResponse { get; set; } = new(false);
    public List<BackendEnrollment> EnrollmentsResponse { get; set; } = [];

    public int CheckEnrollmentCallCount { get; private set; }
    public int GetEnrollmentsCallCount { get; private set; }

    public Task<BackendCheckEnrollment> CheckEnrollmentAsync(Guid courseId, CancellationToken ct)
    {
        CheckEnrollmentCallCount++;
        return Task.FromResult(EnrollmentResponse);
    }

    public Task<IReadOnlyList<BackendEnrollment>> GetEnrollmentsAsync(CancellationToken ct)
    {
        GetEnrollmentsCallCount++;
        return Task.FromResult<IReadOnlyList<BackendEnrollment>>(EnrollmentsResponse);
    }

    public Task<BackendPurchase> PurchaseCourseAsync(PurchaseCourseRequest request, CancellationToken ct) => throw new NotImplementedException();
}
