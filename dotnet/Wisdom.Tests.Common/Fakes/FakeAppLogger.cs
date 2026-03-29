using Wisdom.BuildingBlocks.Loggers;

namespace Wisdom.Tests.Common.Fakes;

/// <summary>No-op IAppLogger for use in unit tests.</summary>
public sealed class FakeAppLogger<T> : IAppLogger<T>
{
    public void LogInformation(LogTypeEnum logTypeEnum, string messageTemplate, params object?[] propertyValues) { }
    public void LogInformation(string messageTemplate, params object?[] propertyValues) { }

    public void LogWarning(LogTypeEnum logTypeEnum, string messageTemplate, params object?[] propertyValues) { }
    public void LogWarning(string messageTemplate, params object?[] propertyValues) { }

    public void LogError(LogTypeEnum logTypeEnum, Exception? exception, string messageTemplate, params object?[] propertyValues) { }
    public void LogError(Exception exception, string messageTemplate, params object?[] propertyValues) { }
    public void LogError(string messageTemplate, params object?[] propertyValues) { }

    public void LogDebug(LogTypeEnum logTypeEnum, string messageTemplate, params object?[] propertyValues) { }
    public void LogDebug(string messageTemplate, params object?[] propertyValues) { }
}
