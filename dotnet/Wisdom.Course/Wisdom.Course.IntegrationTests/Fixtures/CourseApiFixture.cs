using Course.Api;
using Microsoft.Extensions.DependencyInjection;
using Wisdom.Course.Infrastructure.Data;
using Wisdom.Tests.Common.Factories;
namespace Wisdom.Course.IntegrationTests.Fixtures;

public class CourseApiFixture : WebApplicationFactoryBase<CourseProgram>, IAsyncLifetime
{
    public CourseDbContext DbContext { get; private set; } = null!;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        TestDbFactory.ReplaceWithInMemory<CourseDbContext>(services);
    }

    public Task InitializeAsync()
    {
        DbContext = TestDbFactory.GetContext<CourseDbContext>(Services);
        DbContext.Database.EnsureCreated();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
        return Task.CompletedTask;
    }
}
