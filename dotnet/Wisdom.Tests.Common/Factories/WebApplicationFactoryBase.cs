using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Wisdom.Tests.Common.Auth;
namespace Wisdom.Tests.Common.Factories;

public abstract class WebApplicationFactoryBase<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private HttpClient? _client;
    private HttpClient Client => _client ??= CreateClient();

    protected sealed override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {

            services.RemoveAll<IAuthenticationSchemeProvider>();
            services
                .AddAuthentication(FakeAuthHandler.SchemeName)
                .AddScheme<FakeAuthHandlerOptions, FakeAuthHandler>(FakeAuthHandler.SchemeName, _ => { });

            ConfigureTestServices(services);

            services.RemoveAll<ILoggerFactory>();
            services.RemoveAll(typeof(ILogger<>));
            services.AddLogging(lb =>
            {
                lb.ClearProviders();
                lb.AddConsole();
                lb.SetMinimumLevel(LogLevel.Warning);
            });
        });

        builder.UseDefaultServiceProvider((_, options) =>
        {
            options.ValidateScopes = false;
            options.ValidateOnBuild = false;
        });

        ConfigureWebHostAdditional(builder);
    }

    protected abstract void ConfigureTestServices(IServiceCollection services);

    protected virtual void ConfigureWebHostAdditional(IWebHostBuilder builder) { }

    public async Task<HttpResponseMessage> DoPost(string path, object body, string token = "")
    {
        SetAuth(token);
        return await Client.PostAsJsonAsync(path, body);
    }

    public async Task<HttpResponseMessage> DoGet(string path, string token = "")
    {
        SetAuth(token);
        return await Client.GetAsync(path);
    }

    public async Task<HttpResponseMessage> DoPut(string path, object body, string token = "")
    {
        SetAuth(token);
        return await Client.PutAsJsonAsync(path, body);
    }

    public async Task<HttpResponseMessage> DoPatch(string path, object body, string token = "")
    {
        SetAuth(token);
        return await Client.PatchAsJsonAsync(path, body);
    }

    public async Task<HttpResponseMessage> DoDelete(string path, string token = "")
    {
        SetAuth(token);
        return await Client.DeleteAsync(path);
    }

    private void SetAuth(string token)
    {
        Client.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }
}
