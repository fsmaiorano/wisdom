using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wisdom.BuildingBlocks.Auth;

public static class InternalAuthExtensions
{
    /// <summary>
    /// Replaces JWT Bearer authentication with internal service-to-service auth.
    /// Backend services call this instead of AddJwtBearer — the BFF owns JWT validation.
    /// </summary>
    public static IServiceCollection AddInternalAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiKey = configuration["InternalAuth:ApiKey"]
            ?? throw new InvalidOperationException(
                "InternalAuth:ApiKey is required. Add it to appsettings.json.");

        services
            .AddAuthentication(InternalAuthHandler.SchemeName)
            .AddScheme<InternalAuthOptions, InternalAuthHandler>(
                InternalAuthHandler.SchemeName,
                opts => opts.ApiKey = apiKey);

        services.AddAuthorization();

        return services;
    }
}
