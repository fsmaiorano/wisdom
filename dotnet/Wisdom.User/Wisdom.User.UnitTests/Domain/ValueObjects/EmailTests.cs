using Wisdom.BuildingBlocks.Exceptions;
using Wisdom.User.Domain.ValueObjects;
namespace Wisdom.User.UnitTests.Domain.ValueObjects;

public class EmailTests
{

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("User.Name+tag@sub.domain.org")]
    [InlineData("a@b.io")]
    [InlineData("123@numbers.net")]
    public void Of_WithValidEmail_ReturnsEmailWithCorrectValue(string raw)
    {
        var email = Email.Of(raw);
        Assert.Equal(raw, email.Value);
    }

    [Fact]
    public void Of_WithValidEmail_RecordEqualityHoldsForSameValue()
    {
        const string raw = "same@test.com";
        Assert.Equal(Email.Of(raw), Email.Of(raw));
    }

    [Fact]
    public void Of_TwoEmailsWithDifferentValues_AreNotEqual()
    {
        Assert.NotEqual(Email.Of("a@test.com"), Email.Of("b@test.com"));
    }

    [Fact]
    public void Of_WithEmptyString_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() => Email.Of(""));
        Assert.Contains("empty", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Of_WithWhitespaceOnly_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Email.Of("   "));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@tld")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@email.com")]
    [InlineData("double@@at.com")]
    public void Of_WithInvalidFormat_ThrowsDomainException(string raw)
    {
        var ex = Assert.Throws<DomainException>(() => Email.Of(raw));
        Assert.Contains("Invalid email", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
