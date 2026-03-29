using System.Net;
using System.Net.Http.Json;
using Wisdom.Tests.Common.Auth;
using Wisdom.Tests.Common.Builders;
using Wisdom.User.IntegrationTests.Fixtures;
namespace Wisdom.User.IntegrationTests.Users;

public class MeUserTests(UserApiFixture fixture) : IClassFixture<UserApiFixture>
{
    [Fact]
    public async Task Me_AuthenticatedWithSeededUser_Returns200WithUserData()
    {
        var email = BogusFactory.Email();
        var user = FakeUserBuilder.CreateUser(email: email);
        fixture.DbContext.Users.Add(user);
        await fixture.DbContext.SaveChangesAsync();

        var response = await fixture.DoGet("/api/users/me", AuthHelper.TokenForEmail(email));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<MeResponse>();
        Assert.NotNull(body);
        Assert.Equal(email, body.Email);
        Assert.NotEqual(Guid.Empty, body.UserId ?? Guid.Empty);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await fixture.DoGet("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_AuthenticatedButUserNotInDatabase_Returns200()
    {
        var ghostEmail = BogusFactory.Email();
        var response = await fixture.DoGet("/api/users/me", AuthHelper.TokenForEmail(ghostEmail));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed record MeResponse(Guid? UserId, string? KeycloakId, string? Email);
}
