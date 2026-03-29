namespace Wisdom.BuildingBlocks.Loggers;
public interface IAppLogger<T>
{

    void LogInformation(LogTypeEnum logTypeEnum, string messageTemplate, params object?[] propertyValues);

    void LogInformation(string messageTemplate, params object?[] propertyValues);

    void LogWarning(LogTypeEnum logTypeEnum, string messageTemplate, params object?[] propertyValues);

    void LogWarning(string messageTemplate, params object?[] propertyValues);

    void LogError(LogTypeEnum logTypeEnum, Exception? exception, string messageTemplate, params object?[] propertyValues);

    void LogError(Exception exception, string messageTemplate, params object?[] propertyValues);

    void LogError(string messageTemplate, params object?[] propertyValues);

    void LogDebug(LogTypeEnum logTypeEnum, string messageTemplate, params object?[] propertyValues);

    void LogDebug(string messageTemplate, params object?[] propertyValues);
}