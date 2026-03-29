using Wisdom.BuildingBlocks;
using Wisdom.BuildingBlocks.Loggers;
using Wisdom.User.Application.Dtos;
using Wisdom.User.Application.Interfaces;
using Wisdom.User.Domain.Entities;
using Wisdom.User.Domain.Repositories;

namespace Wisdom.User.Application.UseCases;

public class RegisterUserUseCase(
    IKeycloakService keycloakService,
    IUserRepository userRepository,
    IAppLogger<RegisterUserUseCase> logger)
{
    public async Task<Result<RegisterUserResponse>> ExecuteAsync(RegisterUserRequest request, string actor = "system", CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Business, "Registering user {Email} requested by {Actor}", request.Email, actor);

        var keycloakResult = await keycloakService.RegisterAsync(request.Email, request.Name, request.Password, request.Role, cancellationToken);
        if (!keycloakResult.IsSuccess)
        {
            logger.LogWarning(LogTypeEnum.Business, "Keycloak registration failed for {Email}: {Error}", request.Email, keycloakResult.Error);
            return Result<RegisterUserResponse>.Failure(keycloakResult.Error!);
        }

        var user = UserEntity.Create(keycloakResult.Data!.KeycloakId, request.Email, request.Name, request.Password, request.Role, actor);
        var createResult = await userRepository.CreateAsync(user, cancellationToken);
        if (!createResult.IsSuccess)
        {
            logger.LogError(LogTypeEnum.Exception, null, "Failed to persist user {Email}: {Error}", request.Email, createResult.Error);
            return Result<RegisterUserResponse>.Failure(createResult.Error!);
        }

        var created = createResult.Data!;
        var internalUserId = created.Id.Value.ToString();

        await keycloakService.SetUserAttributeAsync(created.KeycloakId, "userId", internalUserId, cancellationToken);
        await keycloakService.EnsureUserIdClaimMapperAsync(cancellationToken);

        logger.LogInformation(LogTypeEnum.Business, "User {Email} registered successfully. UserId={UserId}", request.Email, internalUserId);

        return Result<RegisterUserResponse>.Success(
            new RegisterUserResponse(created.Id.Value, created.KeycloakId, created.Email, created.Name, created.Role));
    }
}
