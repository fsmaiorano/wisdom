using Wisdom.BuildingBlocks;
using Wisdom.Tests.Common.Assertions;
using Wisdom.Tests.Common.Builders;
using Wisdom.Tests.Common.Fakes;
using Wisdom.User.Application.Dtos;
using Wisdom.User.Application.Interfaces;
using Wisdom.User.Application.UseCases;
using Wisdom.User.Domain.Enums;
using Wisdom.User.UnitTests.Mocks;
namespace Wisdom.User.UnitTests.Application.UseCases;

public class RegisterUserUseCaseTests
{
    private readonly FakeKeycloakService _keycloak = new();
    private readonly FakeUserRepository  _repo     = new();

    private RegisterUserUseCase CreateUseCase() => new(_keycloak, _repo, new FakeAppLogger<RegisterUserUseCase>());

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CallsKeycloakRegister()
    {
        var request = FakeUserBuilder.CreateRegisterRequest();
        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(request);

        Assert.Equal(1, _keycloak.RegisterCallCount);
        Assert.Equal(request.Email,    _keycloak.LastRegisterEmail);
        Assert.Equal(request.Name,     _keycloak.LastRegisterName);
        Assert.Equal(request.Password, _keycloak.LastRegisterPassword);
        Assert.Equal(request.Role,     _keycloak.LastRegisterRole);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_PersistsUserInRepository()
    {
        var request = FakeUserBuilder.CreateRegisterRequest(email: "persist@test.com");
        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(request);

        Assert.Equal(1, _repo.CreateCallCount);
        Assert.NotNull(_repo.LastCreatedUser);
        Assert.Equal("persist@test.com",                           _repo.LastCreatedUser!.Email);
        Assert.Equal(_keycloak.RegisterResult.Data!.KeycloakId, _repo.LastCreatedUser.KeycloakId);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ReturnsSuccessWithMappedResponse()
    {
        var keycloakId = Guid.NewGuid().ToString();
        _keycloak.RegisterResult = Result<KeycloakRegisterResult>.Success(new KeycloakRegisterResult(keycloakId));
        var request = FakeUserBuilder.CreateRegisterRequest(email: "resp@test.com", role: UserRoleEnum.Teacher);
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(request);

        var data = result.ShouldSucceed();
        Assert.NotEqual(Guid.Empty, data.UserId);
        Assert.Equal(keycloakId,        data.KeycloakId);
        Assert.Equal("resp@test.com",   data.Email);
        Assert.Equal(request.Name,      data.Name);
        Assert.Equal(UserRoleEnum.Teacher, data.Role);
    }

    [Fact]
    public async Task ExecuteAsync_NewUser_IsActiveByDefault()
    {
        var useCase = CreateUseCase();
        await useCase.ExecuteAsync(FakeUserBuilder.CreateRegisterRequest());

        Assert.True(_repo.LastCreatedUser!.IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_WhenKeycloakFails_ReturnsFailure()
    {
        _keycloak.RegisterResult = Result<KeycloakRegisterResult>.Failure("Keycloak error");
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(FakeUserBuilder.CreateRegisterRequest());

        result.ShouldFail("Keycloak error");
    }

    [Fact]
    public async Task ExecuteAsync_WhenKeycloakFails_DoesNotCallRepository()
    {
        _keycloak.RegisterResult = Result<KeycloakRegisterResult>.Failure("Keycloak error");
        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(FakeUserBuilder.CreateRegisterRequest());

        Assert.Equal(0, _repo.CreateCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryCreateFails_ReturnsFailure()
    {
        _repo.CreateShouldFail = true;
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(FakeUserBuilder.CreateRegisterRequest());

        result.ShouldFail("Repository error");
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsCancellationTokenToKeycloak()
    {
        using var cts = new CancellationTokenSource();
        var useCase   = CreateUseCase();

        await useCase.ExecuteAsync(FakeUserBuilder.CreateRegisterRequest(), cancellationToken: cts.Token);

        Assert.Equal(cts.Token, _keycloak.LastCancellationToken);
    }
}
