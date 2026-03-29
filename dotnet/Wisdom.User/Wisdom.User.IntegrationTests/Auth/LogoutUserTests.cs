using System.Net;
using Wisdom.Tests.Common.Builders;
using Wisdom.User.Application.Dtos;
using Wisdom.User.IntegrationTests.Fixtures;
namespace Wisdom.User.IntegrationTests.Auth;

public class LogoutUserTests(UserApiFixture fixture) : IClassFixture<UserApiFixture>
{
    [Fact]
    public async Task Logout_WithValidToken_Returns204()
    {
        var refreshToken = BogusFactory.UniqueId();
        var response = await fixture.DoPost("/api/auth/logout", new LogoutUserRequest(refreshToken));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WhenKeycloakFails_Returns400()
    {
        fixture.KeycloakService.LogoutShouldFail = true;
        try
        {
            var someToken = BogusFactory.UniqueId();
            var response = await fixture.DoPost("/api/auth/logout", new LogoutUserRequest(someToken));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        finally { fixture.KeycloakService.LogoutShouldFail = false; }
    }
}
