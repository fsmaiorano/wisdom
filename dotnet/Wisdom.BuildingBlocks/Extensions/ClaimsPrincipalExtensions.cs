using System.Security.Claims;
namespace Wisdom.BuildingBlocks.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the user ID from the "sub" claim (Keycloak UUID).
    /// Falls back to <see cref="ClaimTypes.NameIdentifier"/> for compatibility.
    /// Returns <c>null</c> when the claim is absent or not a valid GUID.
    /// </summary>
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue("userId")
               ?? user.FindFirstValue("sub")
               ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(raw, out var id) ? id : null;
    }

    /// <summary>Returns the user's email claim, or <c>null</c> if absent.</summary>
    public static string? GetEmail(this ClaimsPrincipal user) =>
        user.FindFirstValue("email")
        ?? user.FindFirstValue(ClaimTypes.Email);
}
