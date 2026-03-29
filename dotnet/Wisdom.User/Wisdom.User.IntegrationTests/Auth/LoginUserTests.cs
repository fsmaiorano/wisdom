using System.Net;
using System.Net.Http.Json;
using Wisdom.Tests.Common.Builders;
using Wisdom.User.Application.Dtos;
using Wisdom.User.IntegrationTests.Fixtures;
namespace Wisdom.User.IntegrationTests.Auth;

public class LoginUserTests(UserApiFixture fixture) : IClassFixture<UserApiFixture>
{
    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithTokens()
    {
        var request = FakeUserBuilder.CreateLoginRequest(BogusFactory.Email());

        var response = await fixture.DoPost("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LoginUserResponse>();
        Assert.NotNull(body);
        Assert.Equal(fixture.KeycloakService.FakeAccessToken, body.AccessToken);
        Assert.Equal(fixture.KeycloakService.FakeRefreshToken, body.RefreshToken);
        Assert.True(body.ExpiresIn > 0);
    }

    [Fact]
    public async Task Login_WhenKeycloakFails_Returns400()
    {
        fixture.KeycloakService.LoginShouldFail = true;
        try
        {
            var response = await fixture.DoPost("/api/auth/login", FakeUserBuilder.CreateLoginRequest("bad@test.com", "wrong"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        finally { fixture.KeycloakService.LoginShouldFail = false; }
    }
}
