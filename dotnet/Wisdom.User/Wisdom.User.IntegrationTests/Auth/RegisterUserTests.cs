using System.Net;
using System.Net.Http.Json;
using Wisdom.Tests.Common.Builders;
using Wisdom.User.Application.Dtos;
using Wisdom.User.IntegrationTests.Fixtures;
namespace Wisdom.User.IntegrationTests.Auth;

public class RegisterUserTests(UserApiFixture fixture) : IClassFixture<UserApiFixture>
{
    [Fact]
    public async Task Register_WithValidRequest_Returns201WithUserData()
    {
        var request = FakeUserBuilder.CreateRegisterRequest();

        var response = await fixture.DoPost("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.UserId);
        Assert.Equal(request.Email, body.Email);
        Assert.Equal(request.Name, body.Name);
        Assert.Equal(request.Role, body.Role);
        Assert.Equal(fixture.KeycloakService.RegisteredKeycloakId, body.KeycloakId);
    }

    [Fact]
    public async Task Register_WhenKeycloakFails_Returns400()
    {
        fixture.KeycloakService.RegisterShouldFail = true;
        try
        {
            var response = await fixture.DoPost("/api/auth/register", FakeUserBuilder.CreateRegisterRequest());
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        finally { fixture.KeycloakService.RegisterShouldFail = false; }
    }

    [Fact]
    public async Task Register_UserIsPersisted_InDatabase()
    {
        var request = FakeUserBuilder.CreateRegisterRequest();

        await fixture.DoPost("/api/auth/register", request);

        var user = fixture.DbContext.Users.FirstOrDefault(u => u.Email == request.Email);
        Assert.NotNull(user);
        Assert.Equal(request.Name, user.Name);
        Assert.True(user.IsActive);
    }
}
