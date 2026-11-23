using MediaWorker.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Wisdom.BuildingBlocks.Events.Interfaces;
using Wisdom.MediaWorker.Application.Abstractions;
using Wisdom.MediaWorker.IntegrationTests.Mocks;
using Wisdom.Tests.Common.Factories;
namespace Wisdom.MediaWorker.IntegrationTests.Fixtures;

public class MediaWorkerFixture : WebApplicationFactoryBase<MediaWorkerProgram>
{
    public FakeMediaRepository Repository { get; } = new();
    public FakeObjectStorage Storage { get; } = new();
    public FakeVideoTranscoder Transcoder { get; } = new();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.RemoveAll<IHostedService>();

        services.RemoveAll<IEventBus>();
        services.AddSingleton<IEventBus>(new FakeEventBus());

        services.RemoveAll<IMediaRepository>();
        services.AddSingleton<IMediaRepository>(Repository);

        services.RemoveAll<IObjectStorage>();
        services.AddSingleton<IObjectStorage>(Storage);

        services.RemoveAll<IVideoTranscoder>();
        services.AddSingleton<IVideoTranscoder>(Transcoder);
    }
}
