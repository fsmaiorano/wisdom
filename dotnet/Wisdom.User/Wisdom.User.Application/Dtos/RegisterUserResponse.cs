using Wisdom.User.Domain.Enums;
namespace Wisdom.User.Application.Dtos;

public record RegisterUserResponse(Guid UserId, string KeycloakId, string Email, string Name, UserRoleEnum Role);
