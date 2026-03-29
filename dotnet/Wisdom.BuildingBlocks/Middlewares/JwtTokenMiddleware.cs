using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Serilog;
namespace Wisdom.BuildingBlocks.Middlewares;

public class JwtTokenMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                JwtSecurityTokenHandler handler = new();
                var jwtToken = handler.ReadJwtToken(token);
                foreach (var claim in jwtToken.Claims)
                {
                    Log.Information($"JWT Claim: {claim.Type} = {claim.Value}");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"JWT token parsing failed: {ex.Message}");
            }
        }
        await next(context);
    }
}

