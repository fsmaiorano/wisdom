using Wisdom.BuildingBlocks;
using Xunit;
namespace Wisdom.Tests.Common.Assertions;

public static class ResultAssertions
{

    public static T ShouldSucceed<T>(this Result<T> result)
    {
        Assert.True(result.IsSuccess, $"Expected Success but was Failure: {result.Error}");
        Assert.NotNull(result.Data);
        return result.Data!;
    }

    public static string ShouldFail<T>(this Result<T> result, string? containingMessage = null)
    {
        Assert.False(result.IsSuccess, "Expected Failure but was Success.");
        Assert.NotNull(result.Error);

        if (containingMessage is not null)
            Assert.Contains(containingMessage, result.Error, StringComparison.OrdinalIgnoreCase);

        return result.Error!;
    }
}
