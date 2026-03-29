using System.Net;
using System.Net.Http.Json;
using Wisdom.Tests.Common.Builders;
using Wisdom.User.Application.Dtos;
using Wisdom.User.IntegrationTests.Fixtures;
namespace Wisdom.User.IntegrationTests.Auth;

public class RefreshTokenTests(UserApiFixture fixture) : IClassFixture<UserApiFixture>
{
    [Fact]
    public async Task RefreshToken_WithValidToken_Returns200WithNewTokens()
    {
        var refreshToken = BogusFactory.UniqueId();
        var response = await fixture.DoPost("/api/auth/refresh", new RefreshTokenRequest(refreshToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
        Assert.NotNull(body);
        Assert.Equal(fixture.KeycloakService.FakeAccessToken, body.AccessToken);
        Assert.Equal(fixture.KeycloakService.FakeRefreshToken, body.RefreshToken);
        Assert.True(body.ExpiresIn > 0);
    }

    [Fact]
    public async Task RefreshToken_WhenKeycloakFails_Returns400()
    {
        fixture.KeycloakService.RefreshShouldFail = true;
        try
        {
            var expiredToken = BogusFactory.UniqueId();
            var response = await fixture.DoPost("/api/auth/refresh", new RefreshTokenRequest(expiredToken));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        finally { fixture.KeycloakService.RefreshShouldFail = false; }
    }
}
