using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Streaming.Api;
using Wisdom.Streaming.Application.Abstractions;
using Wisdom.Streaming.IntegrationTests.Mocks;
using Wisdom.Tests.Common.Factories;
namespace Wisdom.Streaming.IntegrationTests.Fixtures;

public class StreamingApiFixture : WebApplicationFactoryBase<StreamingProgram>
{
    public FakeMediaStatusReader StatusReader { get; } = new();
    public FakeVideoStorage Storage { get; } = new();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.RemoveAll<IMediaStatusReader>();
        services.AddSingleton<IMediaStatusReader>(StatusReader);

        services.RemoveAll<IVideoStorage>();
        services.AddSingleton<IVideoStorage>(Storage);
    }

    protected override void ConfigureWebHostAdditional(IWebHostBuilder builder)
        => builder.UseSetting("Streaming:DisableAuth", "false");
}
