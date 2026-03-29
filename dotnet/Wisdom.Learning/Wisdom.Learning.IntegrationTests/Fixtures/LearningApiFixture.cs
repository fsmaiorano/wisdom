using Learning.Api;
using Microsoft.Extensions.DependencyInjection;
using Wisdom.Tests.Common.Factories;
using Wisdom.Learning.Infrastructure.Data;
namespace Wisdom.Learning.IntegrationTests.Fixtures;

public class LearningApiFixture : WebApplicationFactoryBase<LearningProgram>, IAsyncLifetime
{
    public LearningDbContext DbContext { get; private set; } = null!;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        TestDbFactory.ReplaceWithInMemory<LearningDbContext>(services);
    }

    public Task InitializeAsync()
    {
        DbContext = TestDbFactory.GetContext<LearningDbContext>(Services);
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
