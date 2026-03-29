using Media.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wisdom.Media.Application.Interfaces;
using Wisdom.Media.Infrastructure.Data;
using Wisdom.Media.IntegrationTests.Mocks;
using Wisdom.Tests.Common.Factories;
namespace Wisdom.Media.IntegrationTests.Fixtures;

public class MediaApiFixture : WebApplicationFactoryBase<MediaProgram>, IAsyncLifetime
{
    public TestMinioService MinioService { get; } = new();
    public MediaDbContext DbContext { get; private set; } = null!;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        TestDbFactory.ReplaceWithInMemory<MediaDbContext>(services);

        services.RemoveAll<IMinioService>();
        services.AddSingleton<IMinioService>(MinioService);
    }

    public Task InitializeAsync()
    {
        DbContext = TestDbFactory.GetContext<MediaDbContext>(Services);
        DbContext.Database.EnsureCreated();
        return Task.CompletedTask;
    }

    public new Task DisposeAsync()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
        return Task.CompletedTask;
    }
}
