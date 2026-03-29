using Wisdom.User.Domain.Enums;
namespace Wisdom.User.Application.Dtos;

public record RegisterUserRequest(string Email, string Name, string Password, UserRoleEnum Role = UserRoleEnum.Customer);
