using Microsoft.AspNetCore.Authentication;

namespace Wisdom.BuildingBlocks.Auth;

public sealed class InternalAuthOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = string.Empty;
}
