using Wisdom.BuildingBlocks.Services.Correlation;

namespace Wisdom.Tests.Common.Fakes;

public sealed class FakeCorrelationIdService : ICorrelationIdService
{
    public const string DefaultCorrelationId = "test-correlation-id";

    private string _correlationId = DefaultCorrelationId;

    public string GetCorrelationId() => _correlationId;

    public void SetCorrelationId(string correlationId) => _correlationId = correlationId;
}
